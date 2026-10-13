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
                case ErrorCommand:
                    return new BattleCommandResult(false, (command as ErrorCommand).errorMessage);
                case MoveCommand:
                    return battle.CommandMove(command as MoveCommand);
                default:
                    break;
            }

            return new BattleCommandResult(false, "Unknown error");
        }
    }
}
