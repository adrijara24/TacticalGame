using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tactical.Core.Domain.Items;
using Tactical.Core.Domain.Units;

namespace Tactical.Core.Persistence
{
    public class CampaignSerializer
    {

        private static void SaveJson(string route, JsonObject jsonObject)
        {
            string? directory = Path.GetDirectoryName(route);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            JsonSerializerOptions options = new()
            {
                WriteIndented = true
            };

            File.WriteAllText(route, jsonObject.ToJsonString(options));

        }

        public static void SaveCampaign(CampaignAssets assets)
        {
            string campaignName = "TestCampaign";
            string campaignRoute = Path.Combine(campaignName);

            JsonObject campaignIndex = new();

            foreach (IAsset asset in assets.GetAllAssets())
            {
                string folder;
                switch(asset)
                {
                    case DUnit:
                        folder = "Units";
                        break;
                    case DClass:
                        folder = "Classes";
                        break;
                    case DWeapon:
                        folder = "Items";
                        break;
                    default:
                        folder = "Misc";
                        break;
                }

                string relativeRoute = Path.Combine(folder, asset.ID + ".json");
                string route = Path.Combine(campaignRoute, relativeRoute);

                SaveJson(route, asset.ToJson());

                if (campaignIndex[folder] is not JsonObject category)
                {
                    category = new JsonObject();
                    campaignIndex[folder] = category;
                }

                category[asset.ID] = relativeRoute.Replace('\\', '/');
            }

            SaveJson(Path.Combine(campaignRoute, "index.json"), campaignIndex);
        }
    }
}
