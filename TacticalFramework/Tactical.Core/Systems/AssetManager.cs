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

        private AssetManager()
        {
            dClasses = new Dictionary<string, DClass>();
            dUnits = new Dictionary<string, DUnit>();
            map = null;
            tiles = new Dictionary<string, DTile>();
            items = new Dictionary<string, DItem>();
            effects = new Dictionary<string, DEffect>();
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
            this.battle = battle;
            dClasses.Clear();
            dUnits.Clear();
            map = null;
            tiles.Clear();
            items.Clear();
            effects.Clear();

            foreach (DEffect effect in assets.GetAll<DEffect>())
                effects.TryAdd(effect.ID, effect);

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
                if (dUnits.ContainsKey(unitID))
                {
                    // MISSING: Modify native unit IDs if they already exist.
                    throw new NotImplementedException();
                }
                else
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
                if (dUnits.ContainsKey(unitID))
                {
                    // MISSING: Modify native unit IDs if they already exist.
                    throw new NotImplementedException();
                }
                else
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
    }
}
