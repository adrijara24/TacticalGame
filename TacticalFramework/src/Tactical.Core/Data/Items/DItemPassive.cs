using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

namespace Tactical.Core.Domain.Items
{
    [Obsolete("Out of scope for now", true)]
    class DItemPassive : DItem
    {
        public DItemPassive(string itemID) : base(itemID)
        {
        }

        public override void FromJson(JsonObject json)
        {
            throw new NotImplementedException();
        }

        public override JsonObject ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
