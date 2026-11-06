using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;
using Tactical.Core.Domain;
using System.Numerics;
using Tactical.Core.Systems;
using Tactical.Test;

internal class Program
{
    private static void Main(string[] args)
    {
        //DamageFormulaValidatorTests.RunTests();
        Console.WriteLine("Battle console. Type commands");

        CampaignAssets campaign = CampaignSerializer.LoadCampaign("CommandTest", "");
        DBattle battle = campaign.ByTag<DBattle>("MovementTestBattle");
        try
        {
            AssetManager.Instance.LoadBattle(battle, campaign);
        }
        catch (System.IO.InvalidDataException exception)
        {
            Console.WriteLine(exception.Message);
            return;
        }

        BattleProcessor battleProcessor = new BattleProcessor(battle);

        while (true)
        {
            Console.Write("> ");
            string? input = Console.ReadLine();
            if (input == null)
                break;
            else
            {
                BattleCommandResult res = battleProcessor.Execute(input);
                if (!res.Status)
                    Console.WriteLine(res.Error);
                else if (!string.IsNullOrEmpty(res.Output))
                    Console.WriteLine(res.Output);
            }
        }
    }
}