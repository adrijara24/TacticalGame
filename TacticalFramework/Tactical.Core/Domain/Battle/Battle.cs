using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Systems;

namespace Tactical.Core.Domain.Battle
{
    internal class Battle
    {
        DBattle Definition { get; }

        Dictionary<string, Unit> team1, team2;

        int currentTeam;
        int turnNumber;

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



        //===============================================
        // COMMANDS
        //===============================================
        public BattleCommandResult CommandMove(MoveCommand command)
        {
            if (CanMove(command.unitID, command.position))
            {
                Unit? unit = GetUnit(command.unitID);
                if (unit == null)
                    return new BattleCommandResult(false, "Unit does not exist");
                unit.Position = command.position;
                unit.HasMoved = true;
                return new BattleCommandResult(true, "");
            }
            return new BattleCommandResult(false, "Could not complete movement");
        }
    }
}
