
using System;
using System.Collections.Generic;
using System.Numerics;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Battle;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Domain.Units;

namespace Tactical.Core.Persistence
{
    public static class CampaignValidator
    {
        public static List<string> ValidateCampaign(CampaignAssets assets)
        {
            List<string> errors = new List<string>();
            HashSet<string> checkedEffects = new HashSet<string>(StringComparer.Ordinal);

            foreach (DUnit unit in assets.GetAll<DUnit>())
                ValidateUnit(unit, assets, errors, checkedEffects);

            foreach (DAbility ability in assets.GetAll<DAbility>())
                ValidateAbility(ability, assets, errors, checkedEffects);

            foreach (DEffect effect in assets.GetAll<DEffect>())
                ValidateEffect(effect.ID, "Effect '" + effect.ID + "'", assets, errors, checkedEffects);

            foreach (DItem item in assets.GetAll<DItem>())
                ValidateItem(item, assets, errors, checkedEffects);

            foreach (DTile tile in assets.GetAll<DTile>())
                ValidateTile(tile, assets, errors, checkedEffects);

            foreach (DMap map in assets.GetAll<DMap>())
                ValidateMap(map, assets, errors, checkedEffects);

            foreach (DBattle battle in assets.GetAll<DBattle>())
                ValidateBattleDefinition(battle, assets, errors, checkedEffects);

            return errors;
        }

        public static List<string> ValidateBattle(DBattle battle, CampaignAssets assets)
        {
            List<string> errors = new List<string>();
            HashSet<string> checkedEffects = new HashSet<string>(StringComparer.Ordinal);

            ValidateBattleDefinition(battle, assets, errors, checkedEffects);

            return errors;
        }

        private static bool ValidateReference<T>(string id, string source, string assetType, CampaignAssets assets, List<string> errors) where T : IAsset
        {
            if (!string.IsNullOrWhiteSpace(id) && assets.Contains<T>(id))
                return true;

            AddError(errors, source + " references missing " + assetType + " '" + id + "'.");
            return false;
        }

        private static void ValidateUnit(DUnit unit, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            string source = "Unit '" + unit.ID + "'";

            ValidateReference<DClass>(unit.ClassID, source, "class", assets, errors);

            foreach (string itemID in unit.Inventory)
            {
                if (string.IsNullOrWhiteSpace(itemID) || itemID == "NONE")
                    continue;

                if (ValidateReference<DItem>(itemID, source, "item", assets, errors))
                    ValidateItem(assets.ByTag<DItem>(itemID), assets, errors, checkedEffects);
            }

            foreach (string abilityID in unit.Abilities)
            {
                if (string.IsNullOrWhiteSpace(abilityID) || abilityID == "NONE")
                    continue;

                if (ValidateReference<DAbility>(abilityID, source, "ability", assets, errors))
                    ValidateAbility(assets.ByTag<DAbility>(abilityID), assets, errors, checkedEffects);
            }
        }

        private static void ValidateAbility(DAbility ability, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            string source = "Ability '" + ability.ID + "'";

            foreach (DAbilityAction action in ability.actions)
                foreach (string effectID in action.addEffects)
                    ValidateEffect(effectID, source, assets, errors, checkedEffects);
        }

        private static void ValidateEffect(string effectID, string source, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            if (!ValidateReference<DEffect>(effectID, source, "effect", assets, errors))
                return;

            if (!checkedEffects.Add(effectID))
                return;

            DEffect effect = assets.ByTag<DEffect>(effectID);

            foreach (EEffectTrigger trigger in Enum.GetValues<EEffectTrigger>())
                foreach (DEffectAction action in effect[trigger])
                    foreach (string addedEffectID in action.addEffects)
                        ValidateEffect(addedEffectID, "Effect '" + effect.ID + "'", assets, errors, checkedEffects);
        }

        private static void ValidateItem(DItem item, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            if (item is DItemConsumable consumable)
                ValidateEffect(consumable.Effect, "Consumable '" + consumable.ID + "'", assets, errors, checkedEffects);
        }

        private static void ValidateTile(DTile tile, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            if (!string.IsNullOrWhiteSpace(tile.Effect) && tile.Effect != "NONE")
                ValidateEffect(tile.Effect, "Tile '" + tile.ID + "'", assets, errors, checkedEffects);
        }

        private static void ValidateMap(DMap map, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            foreach (string tileID in map.TileList())
            {
                if (string.IsNullOrWhiteSpace(tileID))
                    continue;

                if (!ValidateReference<DTile>(tileID, "Map '" + map.ID + "'", "tile", assets, errors))
                    continue;

                ValidateTile(assets.ByTag<DTile>(tileID), assets, errors, checkedEffects);
            }
        }

        private static void ValidateBattleDefinition(DBattle battle, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects)
        {
            DMap? map = null;

            if (ValidateReference<DMap>(battle.mapID, "Battle '" + battle.ID + "'", "map", assets, errors))
            {
                map = assets.ByTag<DMap>(battle.mapID);
                ValidateMap(map, assets, errors, checkedEffects);
            }

            HashSet<(int X, int Z)> occupied = new HashSet<(int X, int Z)>();

            ValidateTeam(battle.team1, "Team 1", battle, map, assets, errors, checkedEffects, occupied);
            ValidateTeam(battle.team2, "Team 2", battle, map, assets, errors, checkedEffects, occupied);
        }

        private static void ValidateTeam(List<KeyValuePair<string, Vector3>> team, string teamName, DBattle battle, DMap? map, CampaignAssets assets, List<string> errors, HashSet<string> checkedEffects, HashSet<(int X, int Z)> occupied)
        {
            foreach (KeyValuePair<string, Vector3> entry in team)
            {
                string source = "Battle '" + battle.ID + "' " + teamName + " unit '" + entry.Key + "'";

                if (ValidateReference<DUnit>(entry.Key, "Battle '" + battle.ID + "' " + teamName, "unit", assets, errors))
                    ValidateUnit(assets.ByTag<DUnit>(entry.Key), assets, errors, checkedEffects);

                if (!BattlePosition.TryFromVector3(entry.Value, out BattlePosition position))
                {
                    AddError(errors, source + " has an invalid start position: " + entry.Value);
                    continue;
                }

                if (map != null && !map.InBounds(position.ToVector3()))
                    AddError(errors, source + " starts outside map '" + map.ID + "' at " + position + ".");

                if (!occupied.Add((position.X, position.Z)))
                    AddError(errors, "Multiple units in battle '" + battle.ID + "' occupy the starting position " + position + ".");
            }
        }

        private static void AddError(List<string> errors, string message)
        {
            if (!errors.Contains(message))
                errors.Add(message);
        }
    }
}
