using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
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

    class DEffectAction
    {
        public EEffectTrigger trigger;
        public int damage;
        public int healing;
        public Stats stats;

        public List<DEffect> addEffects;
        public List<String> removeEffects;
        
        public DEffectAction()
        {
            this.addEffects = new List<DEffect>();
            this.removeEffects = new List<String>();
        }

        public DEffectAction(EEffectTrigger trigger)
        {
            this.trigger = trigger;
            this.addEffects = new List<DEffect>();
            this.removeEffects = new List<String>();
        }
    }

    class DEffect : IPersistent
    {
        String effectID;
        EEffectDuration duration;
        int turns;

        EEffectStacking stacking;
        int maxStacks;

        List<DEffectAction> actions;

        public DEffect()
        {
            effectID = "";
            this.actions = new List<DEffectAction>();
        }

        public String ID => effectID;

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

        public JsonElement ToJson()
        {
            throw new NotImplementedException();
        }

        public void FromJson(JsonElement json)
        {
            throw new NotImplementedException();
        }
    }
}
