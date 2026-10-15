using System;
using System.Collections.Generic;
using System.Text;

using Tactical.Core.Domain.Battle;

namespace Tactical.Core.Systems
{
    internal class BattleCommandParser
    {
        public static Command ParseCommand(string command)
        {
            string[] split = command.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length == 0)
                return new ErrorCommand("No command");
            switch (split[0].ToUpper())
            {
                case "MOVE":
                    if (split.Length != 5)
                        return new ErrorCommand("USAGE: MOVE unitID X Y Z");

                    return new MoveCommand(split[1], new BattlePosition(Int32.Parse(split[2]), Int32.Parse(split[3]), Int32.Parse(split[4])));

                case "ATTACK":
                    if (split.Length != 3)
                        return new ErrorCommand("USAGE: ATTACK attackerID targetID");
                
                    return new AttackCommand(split[1], split[2]);

                case "WAIT":
                    if (split.Length != 2)
                        return new ErrorCommand("USAGE: WAIT unitID");

                    return new WaitCommand(split[1]);

                case "ENDTURN":
                    if (split.Length != 1)
                        return new ErrorCommand("USAGE: ENDTURN");

                    return new EndTurnCommand();
                // DATA commands
                case "TEAM":
                {
                    if (split.Length != 2)
                        return new ErrorCommand("USAGE: TEAM 1|2");

                    if (!Int32.TryParse(split[1], out int team) || (team != 1 && team != 2))
                        return new ErrorCommand("Team must be 1 or 2");

                    return new TeamCommand(team);
                }           

                case "UNIT":
                    if (split.Length != 2)
                        return new ErrorCommand("USAGE: UNIT unitID");
                    return new UnitCommand(split[1]);

                default:
                    return new ErrorCommand("Command " + split[0] + " not supported");

                case "MOVEOPTIONS":
                    if (split.Length != 2)
                        return new ErrorCommand("USAGE: MOVEOPTIONS unitID");

                    return new MoveOptionsCommand(split[1]);

                case "STATUS":
                {
                    if (split.Length == 3 && split[1].ToUpperInvariant() == "UNIT")
                        return new StatusCommand(split[2]);

                    if (split.Length == 3 && split[1].ToUpperInvariant() == "TEAM")
                    {
                        if (Int32.TryParse(split[2], out int team) && (team == 1 || team == 2))
                            return new StatusCommand(team);
                    }

                    return new ErrorCommand("USAGE: STATUS UNIT unitID | STATUS TEAM 1|2");
                }
            }
        }
    }
}
