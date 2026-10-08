using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Tactical.Core.Persistence;

namespace Tactical.Core.Domain.Units
{
    public enum EEffectDuration
    {
        SINGLE = 1, PERMANENT = 2, TURNS = 4
    }
    public enum EEffectStacking
    {
        IGNORE = 1, REFRESH = 2, STACK = 4
    }

    public enum EEffectTrigger
    {
        // Priority: ONAPPPLY, ONTURNSTART, ONTURNEND, ONEXPIRE, ONEND
        // And by order of addition
        ONAPPLY = 1, ONTURNSTART = 2, ONTURNEND = 4, ONEXPIRE = 8, ONEND = 16   // OnExpire is when it runs out naturally (Single,Turns). OnEND is any time it is removed
    }

    public class DEffectAction
    {
        public EEffectTrigger trigger;
        public DamageDefinition? damageDefinition;
        public Stats stats;

        public List<string> addEffects;
        public List<string> removeEffects;
        
        public DEffectAction()
        {
            this.addEffects = new List<string>();
            this.removeEffects = new List<string>();
        }

        public DEffectAction(EEffectTrigger trigger)
        {
            this.trigger = trigger;
            this.addEffects = new List<string>();
            this.removeEffects = new List<string>();
        }
    }

    public class DEffect : IAsset
    {
        string effectID;
        private string name;
        private string description;

        EEffectDuration duration;
        int turns;

        EEffectStacking stacking;
        int maxStacks;

        List<DEffectAction> actions;

        public DEffect()
        {
            this.effectID = "";
            this.name = "";
            this.description = "";
            this.actions = new List<DEffectAction>();
        }
        public DEffect(string effectID)
        {
            this.effectID = effectID;
            this.name = "";
            this.description = "";
            this.actions = new List<DEffectAction>();
        }

        public DEffect(string effectID, EEffectDuration duration, int maxTurns, EEffectStacking stacking, int maxStacks, DEffectAction[] actions)
        {
            this.effectID = effectID;
            this.name = "";
            this.description = "";
            this.duration = duration;
            this.turns = maxTurns;
            this.stacking = stacking;
            this.maxStacks = maxStacks;
            this.actions = new List<DEffectAction>(actions);
        }

        public string ID => effectID;

        public string Name => name;
        public string Description => description;
        public EEffectDuration Duration => duration;
        public int Turns => turns;
        public EEffectStacking Stacking => stacking;
        public int MaxStacks => maxStacks;

        public List<DEffectAction> this[EEffectTrigger trigger]
        {
            get
            {
                List<DEffectAction> ret = new List<DEffectAction>();
                foreach (DEffectAction action in actions)
                    if (action.trigger == trigger)
                        ret.Add(action);
                return ret;
            }
        }

        public JsonObject ToJson()
        {
            JsonObject json = new JsonObject();
            json.Add("ID", ID);
            json.Add("Name", Name);
            json.Add("Description", Description);
            JsonObject durationData = new JsonObject();
            durationData.Add("EffectDuration", (int)duration);
            durationData.Add("Turns", turns);
            
            JsonObject stackingData = new JsonObject();
            stackingData.Add("EffectStacking", (int)stacking);
            stackingData.Add("MaxStacks", maxStacks);

            JsonArray actionsJson = new JsonArray();
            foreach (DEffectAction action in actions)
            {
                JsonObject acJson = new JsonObject();
                acJson.Add("EffectTrigger", (int)action.trigger);
                if (action.damageDefinition != null)
                    acJson.Add("Damage", action.damageDefinition.ToJson());
                acJson.Add("Stats", action.stats.ToJson());
                JsonArray addEffectsJson = new JsonArray();
                foreach (string s in action.addEffects)
                    addEffectsJson.Add(s);
                JsonArray remEffectsJson = new JsonArray();
                foreach (string s in action.removeEffects)
                    remEffectsJson.Add(s);
                acJson.Add("AddEffects", addEffectsJson);
                acJson.Add("RemEffects", remEffectsJson);
                actionsJson.Add(acJson);
            }
            json.Add("Duration", durationData);
            json.Add("Stacking", stackingData);
            json.Add("Actions", actionsJson);

            return json;
        }

        public void FromJson(JsonObject json)
        {
            effectID = json["ID"]!.GetValue<string>();
            name = json["Name"]!.GetValue<string>();
            description = json["Description"]!.GetValue<string>();

            JsonObject durationData = json["Duration"]!.AsObject();
            duration = (EEffectDuration)durationData["EffectDuration"]!.GetValue<int>();
            turns = durationData["Turns"]!.GetValue<int>();

            JsonObject stackingData = json["Stacking"]!.AsObject();
            stacking = (EEffectStacking)stackingData["EffectStacking"]!.GetValue<int>();
            maxStacks = stackingData["MaxStacks"]!.GetValue<int>();

            actions.Clear();

            foreach (JsonNode? node in json["Actions"]!.AsArray())
            {
                JsonObject actionJson = node!.AsObject();

                DEffectAction action = new DEffectAction((EEffectTrigger)actionJson["EffectTrigger"]!.GetValue<int>());

                if (actionJson["Damage"] != null)
                {
                    action.damageDefinition = new DamageDefinition();
                    action.damageDefinition.FromJson(actionJson["Damage"]!.AsObject());
                }
                action.stats.FromJson(actionJson["Stats"]!.AsObject());

                foreach (JsonNode? effect in actionJson["AddEffects"]!.AsArray())
                    action.addEffects.Add(effect!.GetValue<string>());

                foreach (JsonNode? effect in actionJson["RemEffects"]!.AsArray())
                    action.removeEffects.Add(effect!.GetValue<string>());

                actions.Add(action);
            }
        }
    }
}
