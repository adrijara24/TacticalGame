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

            if (command is ErrorCommand error)
                return new BattleCommandResult(false, error.errorMessage);

            if (battle.BattleOver && command is not TeamCommand && command is not UnitCommand && command is not StatusCommand && command is not MoveOptionsCommand)
                return new BattleCommandResult(false, "Battle has ended");

            int previousTeam = battle.CurrentTeam;
            BattleCommandResult result;

            switch (command)
            {
                case MoveCommand move:
                    result = battle.Command(move);
                    break;
                case AttackCommand attack:
                    result = battle.Command(attack);
                    break;
                case UseCommand use:
                    result = battle.Command(use);
                    break;
                case WaitCommand wait:
                    result = battle.Command(wait);
                    break;
                case EndTurnCommand endTurn:
                    result = battle.Command(endTurn);
                    break;
                case TeamCommand team:
                    result = battle.Command(team);
                    break;
                case UnitCommand unit:
                    result = battle.Command(unit);
                    break;
                case MoveOptionsCommand options:
                    result = battle.Command(options);
                    break;
                case StatusCommand status:
                    result = battle.Command(status);
                    break;
                default:
                    return new BattleCommandResult(false, "Unknown command");
            }

            // Check automatically after command execution.
            if (!battle.CheckAutomaticEndTurn())
                return result;

            string turnMessage;

            if (battle.BattleOver)
            {
                if (battle.WinnerTeam == 0)
                    turnMessage = "Battle ended in a draw.";
                else
                    turnMessage = "Battle ended. Team " + battle.WinnerTeam + " wins.";
            }
            else
            {
                turnMessage = "All living units on team " + previousTeam + " have acted. Team " + battle.CurrentTeam + "'s turn begins.";
            }

            string effectMessages = battle.ConsumeTurnMessages();

            if (!string.IsNullOrEmpty(effectMessages))
                turnMessage += Environment.NewLine + effectMessages;

            if (result.Status)
            {
                string output = result.Output;

                if (!string.IsNullOrEmpty(output))
                    output += Environment.NewLine;

                return new BattleCommandResult(true, output + turnMessage);
            }

            return new BattleCommandResult(false, result.Error + Environment.NewLine + turnMessage);
        }
    }
}
