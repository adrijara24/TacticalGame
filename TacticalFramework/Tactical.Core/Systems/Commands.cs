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

    internal class AttackCommand : Command
    {
        public string attackerID, targetID;
        public int? weaponSlot;

        public AttackCommand(string attackerID, string targetID, int? weaponSlot = null) : base("ATTACK")
        {
            this.attackerID = attackerID;
            this.targetID = targetID;
            this.weaponSlot = weaponSlot;
        }
    }

    internal class UseCommand : Command
    {
        public string unitID;
        public int slot;
        public string targetID;

        public UseCommand(string unitID, int slot, string targetID) : base("USE")
        {
            this.unitID = unitID;
            this.slot = slot;
            this.targetID = targetID;
        }
    }

    internal class WaitCommand : Command
    {
        public string unitID;

        public WaitCommand(string unitID) : base("WAIT")
        {
            this.unitID = unitID;
        }
    }

    internal class EndTurnCommand : Command
    {
        public EndTurnCommand() : base("ENDTURN")
        {
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

    internal class MoveOptionsCommand : Command
    {
        public string unitID;

        public MoveOptionsCommand(string unitID) : base("MOVEOPTIONS")
        {
            this.unitID = unitID;
        }
    }

    internal class StatusCommand : Command
    {
        public string unitID;
        public int team;
        public bool isTeam;

        public StatusCommand(string unitID) : base("STATUS")
        {
            this.unitID = unitID;
            this.team = 0;
            this.isTeam = false;
        }

        public StatusCommand(int team) : base("STATUS")
        {
            this.unitID = "";
            this.team = team;
            this.isTeam = true;
        }
    }


}
