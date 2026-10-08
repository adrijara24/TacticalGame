using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.Json.Nodes;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Units
{
    public enum EAbilityType
    {
        ACTIVE = 1, PASSIVE = 2
    }

    public enum EAbilityTarget
    {
        SELF = 1, ALLY = 2, ENEMY = 4, ANY = 8
    }
    public enum EAbilityTrigger
    {
        ONUSE = 1, ONATTACK = 2, ONHIT = 4, ONDAMAGE = 8, ONDAMAGERECEIVED = 16, ONKILL = 32, ONCRITICAL = 64, ONTURNSTART = 128, ONTURNEND = 256, ONACTIVE = 512 // More can be added
    }

    public class DAbilityAction : IPersistent
    {
        public EAbilityTrigger trigger;
        public DamageDefinition damageDefinition;

        public Stats stats;

        public List<string> addEffects;
        public List<string> remEffects;

        public DAbilityAction()
        {
            damageDefinition = new DamageDefinition();
            addEffects = new List<string>();
            remEffects = new List<string>();
        }

        public DAbilityAction(EAbilityTrigger trigger)
        {
            this.trigger = trigger;
            damageDefinition = new DamageDefinition();
            addEffects = new List<string>();
            remEffects = new List<string>();
        }

        public void FromJson(JsonObject json)
        {
            trigger = (EAbilityTrigger)json["Trigger"]!.GetValue<int>();

            damageDefinition.FromJson(json["Damage"]!.AsObject());

            stats.FromJson(json["Stats"]!.AsObject());

            addEffects.Clear();
            foreach (JsonNode? effect in json["AddEffects"]!.AsArray())
                addEffects.Add(effect!.GetValue<string>());

            remEffects.Clear();
            foreach (JsonNode? effect in json["RemEffects"]!.AsArray())
                remEffects.Add(effect!.GetValue<string>());
        }

        public JsonObject ToJson()
        {
            JsonObject json = new JsonObject();

            json.Add("Trigger", (int)trigger);
            json.Add("Damage", damageDefinition.ToJson());
            json.Add("Stats", stats.ToJson());

            JsonArray addEffectsJson = new JsonArray();
            foreach (string effect in addEffects)
                addEffectsJson.Add(effect);

            JsonArray remEffectsJson = new JsonArray();
            foreach (string effect in remEffects)
                remEffectsJson.Add(effect);

            json.Add("AddEffects", addEffectsJson);
            json.Add("RemEffects", remEffectsJson);

            return json;
        }
    }

    public class DAbility : IAsset
    {
        // Delete public access when UI is done.
        public string abilityID;
        public string name;
        public string description;

        public EAbilityType type;
        public int manaCost;
        public int minRange, maxRange;

        public EAbilityTarget target;

        public List<Vector3> area; // Area defined by users.

        public List<DAbilityAction> actions;

        public DAbility()
        {
            abilityID = "";
            name = "";
            description = "";
            type = EAbilityType.ACTIVE;
            target = EAbilityTarget.ANY;
            area = new List<Vector3>();
            actions = new List<DAbilityAction>();
        }

        public string ID => abilityID;

        public string Name => name;
        public string Description => description;

        public EAbilityType AbilityType => type;

        public int ManaCost => manaCost;

        public int MinRange => minRange;
        public int MaxRange => maxRange;

        public EAbilityTarget Target => target;
        public Vector3[] Area => area.ToArray();

        public List<DAbilityAction> this[EAbilityTrigger trigger]
        {
            get
            {
                List<DAbilityAction> ret = new List<DAbilityAction>();
                foreach (DAbilityAction action in actions)
                    if (action.trigger == trigger)
                        ret.Add(action);
                return ret;
            }
        }

        public void FromJson(JsonObject json)
        {
            abilityID = json["ID"]!.GetValue<string>();
            name = json["Name"]!.GetValue<string>();
            description = json["Description"]!.GetValue<string>();

            type = (EAbilityType)json["Type"]!.GetValue<int>();
            manaCost = json["ManaCost"]!.GetValue<int>();

            JsonObject rangeJson = json["Range"]!.AsObject();
            minRange = rangeJson["Min"]!.GetValue<int>();
            maxRange = rangeJson["Max"]!.GetValue<int>();

            target = (EAbilityTarget)json["Target"]!.GetValue<int>();

            area.Clear();
            foreach (JsonNode? node in json["Area"]!.AsArray())
            {
                JsonObject point = node!.AsObject();

                area.Add(new Vector3(
                    point["X"]!.GetValue<float>(),
                    point["Y"]!.GetValue<float>(),
                    point["Z"]!.GetValue<float>()
                ));
            }

            actions.Clear();
            foreach (JsonNode? node in json["Actions"]!.AsArray())
            {
                DAbilityAction action = new DAbilityAction();
                action.FromJson(node!.AsObject());
                actions.Add(action);
            }
        }

        public JsonObject ToJson()
        {
            JsonObject json = new JsonObject();

            json.Add("ID", abilityID);
            json.Add("Name", name);
            json.Add("Description", description);
            json.Add("Type", (int)type);
            json.Add("ManaCost", manaCost);

            JsonObject rangeJson = new JsonObject();
            rangeJson.Add("Min", minRange);
            rangeJson.Add("Max", maxRange);
            json.Add("Range", rangeJson);

            json.Add("Target", (int)target);

            JsonArray areaJson = new JsonArray();
            foreach (Vector3 point in area)
            {
                JsonObject pointJson = new JsonObject();
                pointJson.Add("X", point.X);
                pointJson.Add("Y", point.Y);
                pointJson.Add("Z", point.Z);
                areaJson.Add(pointJson);
            }
            json.Add("Area", areaJson);

            JsonArray actionsJson = new JsonArray();
            foreach (DAbilityAction action in actions)
                actionsJson.Add(action.ToJson());

            json.Add("Actions", actionsJson);

            return json;
        }
    }
}
