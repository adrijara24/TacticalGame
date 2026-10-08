using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain
{
    /// <summary>
    /// Defines the raw power an attack produce. Do NOT consider resistances here unless you want to apply a double reduction.
    /// </summary>
    public class DamageDefinition : IPersistent
    {
        string formula; //Customizable formula to be resolved given user and target
        EDamageType damageType;

        public DamageDefinition()
        {
            formula = "";
            this.damageType = EDamageType.PHYSICAL;
        }
        public DamageDefinition(string formula, EDamageType damageType)
        {
            this.formula = formula;
            this.damageType = damageType;
        }

        public string Formula => formula;

        public EDamageType DamageType => damageType;

        DamageInstance CreateDamageInstance(/*User, Target*/)
        {
            return new DamageInstance(0, EDamageType.PURE);
        }

        public void FromJson(JsonObject json)
        {
            formula = json["Formula"]!.GetValue<string>();
            damageType = (EDamageType)json["DamageType"]!.GetValue<int>();
        }

        public JsonObject ToJson()
        {
            JsonObject json = new JsonObject();
            json.Add("Formula", formula);
            json.Add("DamageType", (int)damageType);

            return json;
        }
    }
}
