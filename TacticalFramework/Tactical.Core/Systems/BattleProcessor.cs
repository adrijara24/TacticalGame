using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Battle;

namespace Tactical.Core.Systems
{
    public class BattleProcessor
    {
        private Battle battle;

        public BattleProcessor(DBattle definition)
        {
            this.battle = new Battle(definition);
        }

        public BattleCommandResult Execute(string input)
        {
            Command command = BattleCommandParser.ParseCommand(input);

            switch (command)
            {
                case ErrorCommand error:
                    return new BattleCommandResult(false, error.errorMessage);
                case MoveCommand move:
                    return battle.Command(move);
                case TeamCommand team:
                    return battle.Command(team);
                case UnitCommand unit:
                    return battle.Command(unit);
                default:
                    break;
            }

            return new BattleCommandResult(false, "Unknown error");
        }
    }
}
