using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Domain.Units;
using Tactical.Core.Systems;

namespace Tactical.Core.Domain.Battle
{
    internal class Battle
    {
        DBattle Definition { get; }

        Dictionary<string, Unit> team1, team2;

        int currentTeam;
        int turnNumber;

        bool battleOver;

        private List<string> turnMessages = new List<string>();

        public Battle(DBattle battle)
        {
            Definition = battle;
            team1 = new Dictionary<string, Unit>();
            team2 = new Dictionary<string, Unit>();
            foreach (KeyValuePair<string, Vector3> unit in battle.team1)
            {
                BattlePosition pos;
                if (!BattlePosition.TryFromVector3(unit.Value, out pos))
                    throw new ArgumentException("Unit " + unit.Key + " has an invalid position");
                AddUnit(1, unit.Key, pos);
            }
            foreach (KeyValuePair<string, Vector3> unit in battle.team2)
            {
                BattlePosition pos;
                if (!BattlePosition.TryFromVector3(unit.Value, out pos))
                    throw new ArgumentException("Unit " + unit.Key + " has an invalid position");
                AddUnit(2, unit.Key, pos);
            }
            currentTeam = 1;
            turnNumber = 1;
            battleOver = false;
        }

        public int CurrentTeam => currentTeam;
        public bool BattleOver => battleOver;

        public int WinnerTeam
        {
            get
            {
                if (HasLivingUnits(1) && !HasLivingUnits(2))
                    return 1;

                if (HasLivingUnits(2) && !HasLivingUnits(1))
                    return 2;

                return 0;
            }
        }

        public void AddUnit(int team, string unitID, BattlePosition position)
        {
            if (IsOccupied(position))
                throw new InvalidOperationException("Cannot place a unit on an occupied position.");
            switch (team)
            {
                case 1:
                    team1.Add(unitID, new Unit(AssetManager.Instance.GetUnit(unitID), position));
                    break;
                case 2:
                    team2.Add(unitID, new Unit(AssetManager.Instance.GetUnit(unitID), position));
                    break;
                default:
                    break;
            }
        }

        public Unit? GetUnit(string id)
        {
            Unit? u = null;
            if (team1.TryGetValue(id, out u))
                return u;
            if (team2.TryGetValue(id, out u))
                return u;
            return u;
        }

        public Unit? GetUnit(string id, out int team)
        {
            Unit? u = null;
            team = 1;
            if (team1.TryGetValue(id, out u))
                return u;
            team = 2;
            if (team2.TryGetValue(id, out u))
                return u;
            team = 0;
            return u;
        }

        public List<Unit> GetTeam(int team)
        {
            switch (team)
            {
                case 1:
                    return team1.Values.ToList();
                    break;
                case 2:
                    return team2.Values.ToList();
                    break;
                default:
                    return new List<Unit>();
                    break;
            }
        }
        //===============================================
        // MOVEMENT SYSTEM
        //===============================================
        public bool IsOccupied(BattlePosition position, int ignoreTeam = 0)
        {
            if (ignoreTeam != 1)
            {
                foreach (Unit u in team1.Values)
                {
                    if (u.IsAlive && u.Position.X == position.X && u.Position.Z == position.Z)
                        return true;
                }
            }

            if (ignoreTeam != 2)
            {
                foreach (Unit u in team2.Values)
                {
                    if (u.IsAlive && u.Position.X == position.X && u.Position.Z == position.Z)
                        return true;
                }
            }

            return false;
        }

        public bool IsTilePassable(Unit unit, BattlePosition pos)
        {
            DMap map = AssetManager.Instance.GetMap();
            if (!map.InBounds(pos.ToVector3()))
                return false;

            DTile tile = AssetManager.Instance.GetTile(map[pos.X, pos.Z]);
            return tile.IsPassableBy(AssetManager.Instance.GetClass(unit.Data.ClassID));
        }

        public int FindMovementCost(string unitID, BattlePosition destination)
        {
            Unit? unit = GetUnit(unitID, out int team);
            if (unit == null)
                return -1;

            DMap map = AssetManager.Instance.GetMap();

            if (destination.X < 0 || destination.X >= map.Width || destination.Z < 0 || destination.Z >= map.Height)
                return -1;

            if (destination.Y != unit.Position.Y)
                return -1;

            if (destination.X == unit.Position.X && destination.Z == unit.Position.Z)
                return 0;

            if (!IsTilePassable(unit, unit.Position) || !IsTilePassable(unit, destination))
                return -1;

            if (IsOccupied(destination))
                return -1;

            int[,] costs = new int[map.Width, map.Height];
            bool[,] visited = new bool[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                    costs[x, z] = int.MaxValue;
            }

            costs[unit.Position.X, unit.Position.Z] = 0;

            int[] offsetX = { 1, -1, 0, 0 };
            int[] offsetZ = { 0, 0, 1, -1 };

            while (true)
            {
                int currentX = -1;
                int currentZ = -1;
                int currentCost = int.MaxValue;

                for (int x = 0; x < map.Width; x++)
                {
                    for (int z = 0; z < map.Height; z++)
                    {
                        if (!visited[x, z] && costs[x, z] < currentCost)
                        {
                            currentX = x;
                            currentZ = z;
                            currentCost = costs[x, z];
                        }
                    }
                }

                if (currentX == -1)
                    return -1;

                if (currentX == destination.X && currentZ == destination.Z)
                    return currentCost;

                visited[currentX, currentZ] = true;

                for (int i = 0; i < 4; i++)
                {
                    int nextX = currentX + offsetX[i];
                    int nextZ = currentZ + offsetZ[i];

                    if (nextX < 0 || nextX >= map.Width || nextZ < 0 || nextZ >= map.Height)
                        continue;

                    if (visited[nextX, nextZ])
                        continue;

                    BattlePosition nextPosition = new BattlePosition(nextX, unit.Position.Y, nextZ);

                    // Allied units do not block the path; enemies do.
                    if (IsOccupied(nextPosition, team) || !IsTilePassable(unit, nextPosition))
                        continue;

                    DTile tile = AssetManager.Instance.GetTile(map[nextX, nextZ]);

                    if (tile.MovementCost <= 0)
                        continue;

                    if (currentCost > int.MaxValue - tile.MovementCost)
                        continue;

                    int newCost = currentCost + tile.MovementCost;

                    if (newCost < costs[nextX, nextZ])
                        costs[nextX, nextZ] = newCost;
                }
            }
        }

        public bool CanMove(string unitID, BattlePosition destination)
        {
            Unit? unit = GetUnit(unitID, out int team);

            if (unit == null)
                return false;

            if (team != currentTeam)
                return false;

            if (!unit.IsAlive || unit.HasMoved || unit.HasActed)
                return false;

            DMap map = AssetManager.Instance.GetMap();

            if (destination.X < 0 || destination.X >= map.Width || destination.Z < 0 || destination.Z >= map.Height)
                return false;

            if (destination.Y != unit.Position.Y)
                return false;

            if (destination.X == unit.Position.X && destination.Z == unit.Position.Z)
                return false;

            // The destination must be empty, regardless of team.
            if (IsOccupied(destination))
                return false;

            if (!IsTilePassable(unit, destination))
                return false;

            int movementCost = FindMovementCost(unitID, destination);

            if (movementCost < 0)
                return false;

            if (movementCost > unit.Stats.movement)
                return false;

            return true;
        }

        private JsonObject GetUnitJson(Unit unit, int team)
        {
            JsonObject json = unit.Data.ToJson();

            json["Team"] = team;

            json["Position"] = new JsonObject
            {
                ["X"] = unit.Position.X,
                ["Y"] = unit.Position.Y,
                ["Z"] = unit.Position.Z
            };

            json["CurrentHP"] = unit.CurrentHP;
            json["CurrentMP"] = unit.CurrentMP;
            json["RemainingLives"] = unit.RemainingLives;
            json["IsAlive"] = unit.IsAlive;
            json["HasMoved"] = unit.HasMoved;
            json["HasActed"] = unit.HasActed;

            //json["BaseStats"] = unit.Data.Stats.ToJson();
            json["Stats"] = unit.Stats.ToJson();

            JsonArray inventory = new JsonArray();

            for (int i = 0; i < unit.Inventory.InventorySize; i++)
            {
                var item = unit.Inventory[i];

                JsonObject slot = new JsonObject
                {
                    ["Slot"] = i + 1,
                    ["ID"] = item == null ? "NONE" : item.ID,
                    ["RemainingUses"] = unit.Inventory.RemainingUses(i)
                };

                inventory.Add(slot);
            }

            json["Inventory"] = inventory;

            return json;
        }

        private JsonObject GetMinimalUnitJson(Unit unit, int team)
        {
            return new JsonObject
            {
                ["ID"] = unit.Data.ID,
                ["Team"] = team,
                ["CurrentHP"] = unit.CurrentHP,
                ["CurrentMP"] = unit.CurrentMP,
                ["Position"] = new JsonObject
                {
                    ["X"] = unit.Position.X,
                    ["Y"] = unit.Position.Y,
                    ["Z"] = unit.Position.Z
                }
            };
        }

        private bool HasLivingUnits(int team)
        {
            foreach (Unit unit in GetTeam(team))
            {
                if (unit.IsAlive)
                    return true;
            }

            return false;
        }

        private bool AdvanceTurn()
        {
            if (battleOver)
                return false;

            turnMessages.Clear();

            int endingTeam = currentTeam;

            foreach (Unit unit in GetTeam(endingTeam))
            {
                if (unit.IsAlive)
                    unit.NotifyTurnPassed(turnMessages);
            }

            if (!HasLivingUnits(1) || !HasLivingUnits(2))
            {
                battleOver = true;
                return true;
            }

            currentTeam = currentTeam == 1 ? 2 : 1;
            turnNumber++;

            foreach (Unit unit in GetTeam(currentTeam))
            {
                unit.NewTurn();

                if (unit.IsAlive)
                    unit.NotifyTurnStarted(turnMessages);
            }

            if (!HasLivingUnits(1) || !HasLivingUnits(2))
                battleOver = true;

            return true;
        }

        public bool CheckAutomaticEndTurn()
        {
            if (battleOver)
                return false;

            if (!HasLivingUnits(1) || !HasLivingUnits(2))
            {
                battleOver = true;
                return true;
            }

            foreach (Unit unit in GetTeam(currentTeam))
            {
                if (unit.IsAlive && !unit.HasActed)
                    return false;
            }

            return AdvanceTurn();
        }

        public string ConsumeTurnMessages()
        {
            string output = string.Join(Environment.NewLine, turnMessages);
            turnMessages.Clear();
            return output;
        }

        //===============================================
        // COMMANDS
        //===============================================
        /// <summary>
        /// Executes a MOVE command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(MoveCommand command)
        {
            if (CanMove(command.unitID, command.position))
            {
                Unit? unit = GetUnit(command.unitID);
                if (unit == null)
                    return new BattleCommandResult(false, "Unit does not exist");
                BattlePosition oldPos = unit.Position;
                unit.Position = command.position;
                unit.HasMoved = true;
                return new BattleCommandResult(true, "Moved unit " + command.unitID + " from " + oldPos + " to " + command.position);
            }
            return new BattleCommandResult(false, "Could not complete movement");
        }

        /// <summary>
        /// Executes a ATTACK command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(AttackCommand command)
        {
            Unit? attacker = GetUnit(command.attackerID, out int attackerTeam);

            if (attacker == null)
                return new BattleCommandResult(false, "Attacker '" + command.attackerID + "' does not exist");

            Unit? target = GetUnit(command.targetID, out int targetTeam);

            if (target == null)
                return new BattleCommandResult(false, "Target '" + command.targetID + "' does not exist");

            if (attackerTeam != currentTeam)
                return new BattleCommandResult(false, "It is not this unit's turn");

            if (attackerTeam == targetTeam)
                return new BattleCommandResult(false, "Cannot attack an allied unit");

            if (!attacker.IsAlive)
                return new BattleCommandResult(false, "Attacker is defeated");

            if (!target.IsAlive)
                return new BattleCommandResult(false, "Target is already defeated");

            if (attacker.HasActed)
                return new BattleCommandResult(false, "Unit has already acted this turn");

            int distance = Math.Abs(attacker.Position.X - target.Position.X)
                         + Math.Abs(attacker.Position.Y - target.Position.Y)
                         + Math.Abs(attacker.Position.Z - target.Position.Z);

            int attackRange = Math.Max(1, attacker.Stats.range);

            if (distance > attackRange)
                return new BattleCommandResult(false, "Target is out of range");

            int attackPower = attacker.Stats.strength;
            DamageInstance damage = new DamageInstance(attackPower, EDamageType.PHYSICAL);
            int damageDealt = target.TakeDamage(damage);

            attacker.HasActed = true;

            if (!target.IsAlive)
                return new BattleCommandResult(true, attacker.Data.ID + " defeated " + target.Data.ID + " with " + damageDealt + " damage");

            return new BattleCommandResult(true, attacker.Data.ID + " attacked " + target.Data.ID + " for " + damageDealt + " damage. Target HP: " + target.CurrentHP + "/" + target.Stats.hp);
        }

        /// <summary>
        /// Executes a USE command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(UseCommand command)
        {
            Unit? user = GetUnit(command.unitID, out int userTeam);

            if (user == null)
                return new BattleCommandResult(false, "Unit '" + command.unitID + "' does not exist");

            if (userTeam != currentTeam)
                return new BattleCommandResult(false, "It is not this unit's turn");

            if (!user.IsAlive)
                return new BattleCommandResult(false, "Unit is defeated");

            if (user.HasActed)
                return new BattleCommandResult(false, "Unit has already acted this turn");

            if (command.slot < 0 || command.slot >= user.Inventory.InventorySize)
                return new BattleCommandResult(false, "Inventory slot is out of bounds");

            DItem? item = user.Inventory[command.slot];

            if (item == null)
                return new BattleCommandResult(false, "Inventory slot is empty");

            if (item is not DItemConsumable consumable)
                return new BattleCommandResult(false, "Only consumable items can be used with USE");

            if (user.Inventory.RemainingUses(command.slot) == 0)
                return new BattleCommandResult(false, "Item has no remaining uses");

            Unit? target = GetUnit(command.targetID);

            if (target == null)
                return new BattleCommandResult(false, "Target '" + command.targetID + "' does not exist");

            if (!target.IsAlive)
                return new BattleCommandResult(false, "Target is defeated");

            DEffect? definition = AssetManager.Instance.GetEffect(consumable.Effect);

            if (definition == null)
                return new BattleCommandResult(false, "Effect '" + consumable.Effect + "' does not exist");

            List<string> messages = new List<string>();
            messages.Add(user.Data.ID + " used " + item.Name + " on " + target.Data.ID);

            Effect.Apply(definition, user, target, messages);

            user.Inventory.Use(command.slot);
            user.HasActed = true;

            return new BattleCommandResult(true, string.Join(Environment.NewLine, messages));
        }

        /// <summary>
        /// Executes a WAIT command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(WaitCommand command)
        {
            Unit? unit = GetUnit(command.unitID, out int team);

            if (unit == null)
                return new BattleCommandResult(false, "Unit '" + command.unitID + "' does not exist");

            if (team != currentTeam)
                return new BattleCommandResult(false, "It is not this unit's turn");

            if (!unit.IsAlive)
                return new BattleCommandResult(false, "Unit is defeated");

            if (unit.HasActed)
                return new BattleCommandResult(false, "Unit has already acted");

            unit.HasActed = true;

            return new BattleCommandResult(true, "Unit " + command.unitID + " waits");
        }

        /// <summary>
        /// Executes a ENDTURN command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(EndTurnCommand command)
        {
            if (battleOver)
                return new BattleCommandResult(false, "Battle has ended");

            if (!HasLivingUnits(1) || !HasLivingUnits(2))
            {
                battleOver = true;

                if (WinnerTeam == 0)
                    return new BattleCommandResult(true, "Battle ended in a draw");

                return new BattleCommandResult(true, "Battle ended. Team " + WinnerTeam + " wins");
            }

            int previousTeam = currentTeam;

            AdvanceTurn();

            string message;

            if (battleOver)
            {
                if (WinnerTeam == 0)
                    message = "Battle ended in a draw";
                else
                    message = "Battle ended. Team " + WinnerTeam + " wins";
            }
            else
            {
                message = "Team " + previousTeam + " ended its turn. Team " + currentTeam + "'s turn begins";
            }

            string effectMessages = ConsumeTurnMessages();

            if (!string.IsNullOrEmpty(effectMessages))
                message += Environment.NewLine + effectMessages;

            return new BattleCommandResult(true, message);
        }

        //===============================================
        // DATA COMMANDS
        //===============================================

        /// <summary>
        /// Executes a TEAM command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(TeamCommand command)
        {
            if (command.team != 1 && command.team != 2)
                return new BattleCommandResult(false, "Team must be 1 or 2");

            JsonArray units = new JsonArray();

            foreach (Unit unit in GetTeam(command.team))
                units.Add(GetUnitJson(unit, command.team));

            JsonObject json = new JsonObject
            {
                ["Team"] = command.team,
                ["Units"] = units
            };

            string output = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

            return new BattleCommandResult(true, output);
        }

        /// <summary>
        /// Executes a UNIT command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(UnitCommand command)
        {
            Unit? unit = GetUnit(command.unitID, out int team);

            if (unit == null)
                return new BattleCommandResult(false, "Unit '" + command.unitID + "' does not exist");

            JsonObject json = GetUnitJson(unit, team);

            string output = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

            return new BattleCommandResult(true, output);
        }

        /// <summary>
        /// Executes a STATUS command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(StatusCommand command)
        {
            JsonObject json;

            if (command.isTeam)
            {
                JsonArray units = new JsonArray();

                foreach (Unit unit in GetTeam(command.team))
                    units.Add(GetMinimalUnitJson(unit, command.team));

                json = new JsonObject
                {
                    ["Team"] = command.team,
                    ["Units"] = units
                };
            }
            else
            {
                Unit? unit = GetUnit(command.unitID, out int team);

                if (unit == null)
                    return new BattleCommandResult(false, "Unit '" + command.unitID + "' does not exist");

                json = GetMinimalUnitJson(unit, team);
            }

            string output = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

            return new BattleCommandResult(true, output);
        }

        /// <summary>
        /// Executes a MOVEOPTIONS command in the battle
        /// </summary>
        /// <param name="command">Command data</param>
        /// <returns>Result of the command execution</returns>
        public BattleCommandResult Command(MoveOptionsCommand command)
        {
            Unit? unit = GetUnit(command.unitID, out int team);

            if (unit == null)
                return new BattleCommandResult(false, "Unit '" + command.unitID + "' does not exist");

            if (team != currentTeam)
                return new BattleCommandResult(false, "It is not this unit's turn");

            if (!unit.IsAlive || unit.HasMoved || unit.HasActed)
                return new BattleCommandResult(false, "Unit cannot move this turn");

            DMap map = AssetManager.Instance.GetMap();
            JsonArray cells = new JsonArray();

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    if (x == unit.Position.X && z == unit.Position.Z)
                        continue;

                    BattlePosition position = new BattlePosition(x, unit.Position.Y, z);
                    int cost = FindMovementCost(command.unitID, position);

                    if (cost < 0 || cost > unit.Stats.movement)
                        continue;

                    cells.Add(new JsonObject
                    {
                        ["X"] = x,
                        ["Y"] = unit.Position.Y,
                        ["Z"] = z,
                        ["Cost"] = cost
                    });
                }
            }

            JsonObject json = new JsonObject
            {
                ["UnitID"] = command.unitID,
                ["Movement"] = unit.Stats.movement,
                ["Cells"] = cells
            };

            string output = json.ToJsonString(new JsonSerializerOptions { WriteIndented = true });

            return new BattleCommandResult(true, output);
        }
    }
}
