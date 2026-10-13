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
            string[] split = command.Split(' ');
            if (split.Length < 1)
                return new ErrorCommand("No command");
            switch (split[0].ToUpper())
            {
                case "MOVE":
                    if (split.Length != 5)
                        return new ErrorCommand("USAGE: MOVE unitID X Y Z");
                    else
                        return new MoveCommand(split[1], new BattlePosition(Int32.Parse(split[2]), Int32.Parse(split[3]), Int32.Parse(split[4])));
                    break;
                default:
                    return new ErrorCommand("Command " + split[0] + " not supported");
                    break;
            }
        }
    }
}
