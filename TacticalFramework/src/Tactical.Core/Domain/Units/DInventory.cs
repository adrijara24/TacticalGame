using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace Tactical.Core.Domain.Units
{
    public class DInventory : IPersistent
    {
        List<DItem?> items;  // We work with fixed inventories, so no resizing or need to add ADD or REMOVE operations. If a 

        int inventorySize;

        public DItem this[int idx]
        {
            get
            {
                if (idx < 0 || idx >= inventorySize)
                    throw new ArgumentOutOfRangeException("Index is out of bounds.");
                return items[idx];
            }

            set
            {
                if (idx < 0 || idx >= inventorySize)
                    throw new ArgumentOutOfRangeException("Index is out of bounds.");
                items[idx] = value;
            }
        }

        public DInventory(int inventorySize)
        {
            this.inventorySize = inventorySize;
            this.items = new List<DItem?>();

            for (int i = 0; i < inventorySize; i++)
                this.items.Add(null);
        }
        public void FromJson(JsonElement json)
        {
            throw new NotImplementedException();
        }

        public JsonElement ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
