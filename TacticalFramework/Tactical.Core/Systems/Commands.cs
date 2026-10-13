using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Tactical.Core.Domain.Battle;

namespace Tactical.Core.Systems
{
    internal class Command
    {
        public string command;
        public Command(string command)
        {
            this.command = command;
        }
    }

    internal class ErrorCommand : Command
    {
        public string errorMessage;
        public ErrorCommand(string error) : base("ERROR")
        {
            this.errorMessage = error;
        }
    }

    // Action commands
    internal class MoveCommand : Command
    {
        public string unitID;
        public BattlePosition position;
        public MoveCommand(string unitID, BattlePosition position) : base("MOVE")
        {
            this.unitID = unitID;
            this.position = position;
        }
    }

    // Data commands
    internal class TeamCommand : Command
    {
        public int team;

        public TeamCommand(int team) : base("TEAM")
        {
            this.team = team;
        }
    }

    internal class UnitCommand : Command
    {
        public string unitID;

        public UnitCommand(string unitID) : base("UNIT")
        {
            this.unitID = unitID;
        }
    }
}
