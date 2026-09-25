using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Tactical.Core.Domain.Items
{
    class DItemPassive : DItem
    {
        public DItemPassive(string itemID) : base(itemID)
        {
        }

        public new void FromJson(JsonObject json)
        {
            throw new NotImplementedException();
        }

        public new JsonObject ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
