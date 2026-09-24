using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Tactical.Core.Domain.Items
{
    class DItemPassive : DItem
    {
        public DItemPassive(string itemID) : base(itemID)
        {
        }

        public new void FromJson(JsonElement json)
        {
            throw new NotImplementedException();
        }

        public new JsonElement ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
