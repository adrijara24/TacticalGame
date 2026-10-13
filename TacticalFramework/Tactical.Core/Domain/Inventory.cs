using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using Tactical.Core.Systems;

namespace Tactical.Core.Domain
{
    public class Inventory
    {
        List<KeyValuePair<DItem?, int>> items;

        int inventorySize;

        public DItem? this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= inventorySize)
                    throw new ArgumentOutOfRangeException("Index is out of bounds.");
                return items[idx].Key;
            }

            set
            {
                if (idx < 0 || idx >= inventorySize)
                    throw new ArgumentOutOfRangeException("Index is out of bounds.");
                items[idx] = new KeyValuePair<DItem?, int>(value, (value is not null and DItemConsumable) ? ((DItemConsumable)value).Uses : -1);
            }
        }

        public int RemainingUses(int idx)
        {
            return items[idx].Value;
        }

        /// <summary>
        /// Spends an use from an item in the inventory. Ignored for items with no uses. If item is used beyond its remaining uses, it's changed to null
        /// </summary>
        /// <param name="idx">Index</param>
        /// <returns>True if the item still exists</returns>
        public bool Use(int idx)
        {
            if (items[idx].Value == -1)
                return true;

            if (items[idx].Value - 1 <= 0)
            {
                items[idx] = new KeyValuePair<DItem?, int>(null, -1);
                return false;
            }
            else
                items[idx] = new KeyValuePair<DItem?, int>(items[idx].Key, items[idx].Value - 1);

            return true;
        }

        public int InventorySize => inventorySize;

        public Inventory(int inventorySize, string[]? items = null)
        {
            this.inventorySize = inventorySize;
            this.items = new List<KeyValuePair<DItem?, int>>();

            if (items == null)
            {
                for (int i = 0; i < inventorySize; i++)
                    this.items.Add(new KeyValuePair<DItem?, int>(null, -1));
            }
            else
            {
                foreach (string s in items)
                {
                    DItem? item =  s == "NONE" ? null : AssetManager.Instance.GetItem(s);
                    this.items.Add(new KeyValuePair<DItem?, int>(item, (item is not null and DItemConsumable) ? ((DItemConsumable)item).Uses : -1));
                }
            }
        }
    }
}
