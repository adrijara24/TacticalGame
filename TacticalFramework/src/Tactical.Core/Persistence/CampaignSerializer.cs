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

        public static void SaveCampaign(string campaignName, CampaignAssets assets)
        {
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
                    case DItemConsumable:
                        folder = "Items";
                        break;
                    case DEffect:
                        folder = "Effects";
                        break;
                    case DAbility:
                        folder = "Abilities";
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
        public static CampaignAssets LoadCampaign(string campaignName, string campaignRoute)
        {
            string campaignDirectory = Path.Combine(campaignRoute, campaignName);
            string indexRoute = Path.Combine(campaignDirectory, "index.json");

            if (!File.Exists(indexRoute))
                throw new FileNotFoundException("Campaign index not found.", indexRoute);

            JsonObject campaignIndex = JsonNode.Parse(File.ReadAllText(indexRoute))!.AsObject();

            CampaignAssets assets = new();

            foreach (KeyValuePair<string, JsonNode?> category in campaignIndex)
            {
                string folder = category.Key;
                JsonObject files = category.Value!.AsObject();

                foreach (KeyValuePair<string, JsonNode?> entry in files)
                {
                    string relativeRoute = entry.Value!.GetValue<string>();
                    string assetRoute = Path.Combine(campaignDirectory, relativeRoute);

                    if (!File.Exists(assetRoute))
                        throw new FileNotFoundException($"Asset file for '{entry.Key}' not found.", assetRoute);

                    JsonObject assetJson = JsonNode.Parse(File.ReadAllText(assetRoute))!.AsObject();

                    IAsset asset;

                    switch (folder)
                    {
                        case "Units":
                            asset = new DUnit();
                            break;

                        case "Classes":
                            asset = new DClass();
                            break;

                        case "Effects":
                            asset = new DEffect();
                            break;

                        case "Items":
                            string type = assetJson["Type"]!.GetValue<string>();

                            asset = type switch
                            {
                                "DWeapon" => new DWeapon(),
                                "DItemConsumable" => new DItemConsumable(),
                                _ => throw new InvalidDataException($"Unknown item type '{type}' in '{assetRoute}'.")
                            };
                            break;
                        case "Abilities":
                            asset = new DAbility();
                            break;
                        default:
                            throw new InvalidDataException($"Unknown asset category '{folder}'.");
                    }

                    asset.FromJson(assetJson);
                    assets.Add(asset);
                }
            }

            return assets;
        }

    }

}
