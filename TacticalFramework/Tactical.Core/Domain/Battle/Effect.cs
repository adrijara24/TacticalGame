using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain.Units;
using Tactical.Core.Systems;

namespace Tactical.Core.Domain.Battle
{
    internal class Effect
    {
        private const int MaxEffectDepth = 16;

        private DEffect definition;
        private Unit source;
        private Unit target;

        private int stacks;
        private int turnsRemaining;
        private bool active;

        private List<DEffectAction> activeStatActions = new List<DEffectAction>();

        public DEffect Definition => definition;
        public string ID => definition.ID;
        public Unit Source => source;
        public Unit Target => target;
        public int Stacks => stacks;
        public int TurnsRemaining => turnsRemaining;
        public bool IsActive => active;

        private Effect(DEffect definition, Unit source, Unit target)
        {
            this.definition = definition;
            this.source = source;
            this.target = target;
            stacks = 1;
            turnsRemaining = definition.Duration == EEffectDuration.TURNS ? Math.Max(0, definition.Turns) : -1;
            active = true;
        }

        public static Effect? Apply(DEffect? definition, Unit source, Unit target, List<string>? messages = null, int depth = 0)
        {
            if (depth > MaxEffectDepth)
            {
                AddMessage(messages, "Effect chain stopped because it was too deep");
                return null;
            }

            if (definition == null)
            {
                AddMessage(messages, "Effect definition does not exist");
                return null;
            }

            if (definition.Duration == EEffectDuration.SINGLE)
            {
                Effect single = new Effect(definition, source, target);
                single.Activate(EEffectTrigger.ONAPPLY, messages, depth + 1);
                single.Activate(EEffectTrigger.ONEXPIRE, messages, depth + 1);
                single.active = false;
                return single;
            }

            Effect? existing = target.FindEffect(definition.ID);

            if (existing != null)
            {
                switch (definition.Stacking)
                {
                    case EEffectStacking.IGNORE:
                        AddMessage(messages, "Effect " + definition.ID + " was ignored because it is already active");
                        return existing;

                    case EEffectStacking.REFRESH:
                        existing.source = source;
                        existing.ResetDuration();
                        AddMessage(messages, "Refreshed effect " + definition.ID);
                        existing.Activate(EEffectTrigger.ONAPPLY, messages, depth + 1);
                        existing.ExpireIfNeeded(messages, depth + 1);
                        return existing;

                    case EEffectStacking.STACK:
                        if (existing.stacks >= Math.Max(1, definition.MaxStacks))
                        {
                            AddMessage(messages, "Effect " + definition.ID + " is already at maximum stacks");
                            return existing;
                        }

                        existing.stacks++;
                        existing.source = source;
                        existing.ResetDuration();
                        AddMessage(messages, "Effect " + definition.ID + " now has " + existing.stacks + " stacks");
                        existing.Activate(EEffectTrigger.ONAPPLY, messages, depth + 1);
                        existing.ExpireIfNeeded(messages, depth + 1);
                        return existing;
                }
            }

            Effect effect = new Effect(definition, source, target);
            target.AddEffect(effect);

            AddMessage(messages, "Applied effect " + definition.ID + " to " + target.Data.ID);

            effect.Activate(EEffectTrigger.ONAPPLY, messages, depth + 1);
            effect.ExpireIfNeeded(messages, depth + 1);

            return effect;
        }

        public Stats GetStatsBonus()
        {
            Stats bonus = new Stats();

            if (!active)
                return bonus;

            foreach (DEffectAction action in activeStatActions)
            {
                for (int i = 0; i < stacks; i++)
                    bonus += action.stats;
            }

            return bonus;
        }

        public void Activate(EEffectTrigger trigger, List<string>? messages = null, int depth = 0)
        {
            if (depth > MaxEffectDepth)
            {
                AddMessage(messages, "Effect chain stopped because it was too deep");
                return;
            }

            if (!target.IsAlive)
                return;

            if (!active && trigger != EEffectTrigger.ONEND)
                return;

            foreach (DEffectAction action in definition[trigger])
            {
                if (!target.IsAlive)
                    break;

                if (!activeStatActions.Contains(action))
                    activeStatActions.Add(action);

                ExecuteAction(action, messages, depth + 1);
            }
        }

        public void TurnStarted(List<string>? messages = null)
        {
            if (active)
                Activate(EEffectTrigger.ONTURNSTART, messages);
        }

        public void TurnPassed(List<string>? messages = null)
        {
            if (!active)
                return;

            Activate(EEffectTrigger.ONTURNEND, messages);

            if (!active || definition.Duration != EEffectDuration.TURNS)
                return;

            turnsRemaining--;

            AddMessage(messages, "Effect " + ID + " has " + turnsRemaining + " turns remaining");

            ExpireIfNeeded(messages, 0);
        }

        public bool Remove(List<string>? messages = null, int depth = 0)
        {
            if (!active)
                return false;

            target.RemoveEffect(this);
            active = false;

            AddMessage(messages, "Removed effect " + ID + " from " + target.Data.ID);

            Activate(EEffectTrigger.ONEND, messages, depth + 1);

            return true;
        }

        private void ResetDuration()
        {
            turnsRemaining = definition.Duration == EEffectDuration.TURNS ? Math.Max(0, definition.Turns) : -1;
        }

        private void ExpireIfNeeded(List<string>? messages, int depth)
        {
            if (!active || definition.Duration != EEffectDuration.TURNS || turnsRemaining > 0)
                return;

            Activate(EEffectTrigger.ONEXPIRE, messages, depth + 1);

            if (active)
                Remove(messages, depth + 1);
        }

        private void ExecuteAction(DEffectAction action, List<string>? messages, int depth)
        {
            if (depth > MaxEffectDepth)
            {
                AddMessage(messages, "Effect chain stopped because it was too deep");
                return;
            }

            if (!target.IsAlive)
                return;

            if (action.damageDefinition != null)
            {
                DamageInstance damage = action.damageDefinition.CreateDamageInstance();
                int oldHP = target.CurrentHP;

                target.TakeDamage(damage);

                if (damage.DamageType == EDamageType.HEALING)
                    AddMessage(messages, target.Data.ID + " healed for " + (target.CurrentHP - oldHP) + " HP");
                else
                    AddMessage(messages, target.Data.ID + " took " + (oldHP - target.CurrentHP) + " damage");

                if (!target.IsAlive)
                    return;
            }

            foreach (string effectID in action.removeEffects)
            {
                Effect? effect = target.FindEffect(effectID);

                if (effect != null)
                    effect.Remove(messages, depth + 1);
                else
                    AddMessage(messages, "Effect " + effectID + " was not active on " + target.Data.ID);
            }

            foreach (string effectID in action.addEffects)
            {
                if (!target.IsAlive)
                    return;

                DEffect? addedDefinition = AssetManager.Instance.GetEffect(effectID);

                if (addedDefinition == null)
                {
                    AddMessage(messages, "Effect definition '" + effectID + "' does not exist");
                    continue;
                }

                Apply(addedDefinition, source, target, messages, depth + 1);
            }
        }

        private static void AddMessage(List<string>? messages, string message)
        {
            messages?.Add(message);
        }
    }
}
