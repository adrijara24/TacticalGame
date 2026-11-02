using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Tactical.Core.Domain;
using Tactical.Core.Domain.Battle;
using Tactical.Core.Domain.Terrain;
using Tactical.Core.Domain.Units;
using Tactical.Core.Persistence;

namespace Tactical.Core.Systems
{
    public class AssetManager
    {
        private static AssetManager? manager;

        private DBattle? battle;

        private DMap? map;

        private Dictionary<string, DTile> tiles;

        private Dictionary<string, DItem> items;

        private Dictionary<string, DClass> dClasses;

        private Dictionary<string, DUnit> dUnits;

        private Dictionary<string, DEffect> effects;

        private Dictionary<string, DAbility> abilities;

        private AssetManager()
        {
            dClasses = new Dictionary<string, DClass>();
            dUnits = new Dictionary<string, DUnit>();
            map = null;
            tiles = new Dictionary<string, DTile>();
            items = new Dictionary<string, DItem>();
            effects = new Dictionary<string, DEffect>();
            abilities = new Dictionary<string, DAbility>();
            battle = null;
        }

        public static AssetManager Instance
        {
            get
            {
                if (manager == null)
                    manager = new AssetManager();
                return manager;
            }
        }

        public void LoadBattle(DBattle battle, CampaignAssets assets)
        {
            List<string> validationErrors = CampaignValidator.ValidateBattle(battle, assets);

            if (validationErrors.Count > 0)
                throw new System.IO.InvalidDataException("Battle '" + battle.ID + "' contains invalid asset references:" + Environment.NewLine + "- " + string.Join(Environment.NewLine + "- ", validationErrors));
            this.battle = battle;
            dClasses.Clear();
            dUnits.Clear();
            map = null;
            tiles.Clear();
            items.Clear();
            effects.Clear();
            abilities.Clear();

            foreach (DEffect effect in assets.GetAll<DEffect>())
                effects.TryAdd(effect.ID, effect);

            foreach (DAbility ability in assets.GetAll<DAbility>())
                abilities.TryAdd(ability.ID, ability);

            map = assets.ByTag<DMap>(battle.mapID);
            {
                HashSet<string> uniqueTiles = map.TileList();
                foreach (string s in uniqueTiles)
                    tiles.TryAdd(s, assets.ByTag<DTile>(s));
            }
            
            
            for (int i = 0; i < battle.team1.Count; i++)
            {
                string unitID = battle.team1[i].Key;
                DUnit unit = assets.ByTag<DUnit>(unitID);
                dUnits.TryAdd(unitID, unit);
                foreach (string s in unit.Inventory)
                    if (s != "NONE")
                        items.TryAdd(s, assets.ByTag<DItem>(s));
                dClasses.TryAdd(unit.ClassID, assets.ByTag<DClass>(unit.ClassID));
                   
            }

            for (int i = 0; i < battle.team2.Count; i++)
            {
                string unitID = battle.team2[i].Key;
                DUnit unit = assets.ByTag<DUnit>(unitID);
                dUnits.TryAdd(unitID, unit);
                foreach (string s in unit.Inventory)
                    if (s != "NONE")
                        items.TryAdd(s, assets.ByTag<DItem>(s));
                dClasses.TryAdd(unit.ClassID, assets.ByTag<DClass>(unit.ClassID));

            }
        }

        public DClass? GetClass(string id)
        {
            DClass? val = null;
            dClasses.TryGetValue(id, out val);
            return val;
        }

        public DUnit? GetUnit(string id)
        {
            DUnit? val = null;
            dUnits.TryGetValue(id, out val);
            return val;
        }

        public DMap? GetMap()
        {
            return map;
        }

        public DTile? GetTile(string id)
        {
            DTile? val = null;
            tiles.TryGetValue(id, out val);
            return val;
        }

        public DItem? GetItem(string id)
        {
            DItem? val = null;
            items.TryGetValue(id, out val);
            return val;
        }

        public DEffect? GetEffect(string id)
        {
            DEffect? val = null;
            effects.TryGetValue(id, out val);
            return val;
        }

        public DAbility? GetAbility(string id)
        {
            DAbility? val = null;
            abilities.TryGetValue(id, out val);
            return val;
        }
    }
}
