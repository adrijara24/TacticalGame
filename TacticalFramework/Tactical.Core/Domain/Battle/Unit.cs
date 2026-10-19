using System;
using System.Collections.Generic;
using System.Text;
using Tactical.Core.Domain.Units;
using Tactical.Core.Domain.Items;
using Tactical.Core.Systems;

namespace Tactical.Core.Domain.Battle
{
    internal class Unit
    {
        DUnit data;

        int currentHP, currentMP;
        int remainingLives;

        Inventory inventory;
        int weaponSlot;

        BattlePosition position;

        private bool hasActed;
        private bool hasMoved;

        private List<Effect> activeEffects = new List<Effect>();

        public Unit(DUnit unit, BattlePosition position)
        {
            data = unit;

            inventory = new Inventory(data.Inventory.Length, data.Inventory);
            this.position = position;
            currentHP = Stats.hp;
            currentMP = Stats.mana;
            remainingLives = data.Lives;
        }

        public Stats Stats
        {
            get
            {
                Stats total = data.Stats;
                if (weaponSlot >= 0 && weaponSlot < inventory.InventorySize)
                    total += inventory[weaponSlot] != null ? ((DWeapon)inventory[weaponSlot]).Stats : new Stats();
                total += AssetManager.Instance.GetClass(data.ClassID).Stats;

                foreach (Effect effect in activeEffects)
                    total += effect.GetStatsBonus();
                return total;
            }
        }

        public BattlePosition Position { get { return position; } set { position = value; } }


        public bool HasMoved { get { return hasMoved; } set { hasMoved = value; } }
        public bool HasActed { get { return hasActed; } set { hasActed = value; } }

        public int CurrentHP => currentHP;
        public int CurrentMP => currentMP;
        public int RemainingLives => remainingLives;
        public Inventory Inventory => inventory;
        public bool IsAlive => currentHP > 0;
        public DUnit Data => data;

        public void NewTurn()
        {
            hasActed = false;
            hasMoved = false;
        }

        public int TakeDamage(DamageInstance damage)
        {
            int oldHP = currentHP;
            currentHP = damage.Apply(currentHP, Stats.hp, Stats.defense, Stats.magicDefense);
            return oldHP - currentHP;
        }

        public List<Effect> GetActiveEffects()
        {
            return activeEffects;
        }

        public Effect? FindEffect(string id)
        {
            foreach (Effect effect in activeEffects)
            {
                if (effect.IsActive && effect.ID == id)
                    return effect;
            }

            return null;
        }

        public void AddEffect(Effect effect)
        {
            if (effect.IsActive && FindEffect(effect.ID) == null)
                activeEffects.Add(effect);
        }

        public void RemoveEffect(Effect effect)
        {
            activeEffects.Remove(effect);
        }

        public void NotifyTurnStarted(List<string> messages)
        {
            List<Effect> effects = new List<Effect>(activeEffects);

            foreach (Effect effect in effects)
            {
                if (!activeEffects.Contains(effect))
                    continue;

                if (effect.IsActive)
                    effect.TurnStarted(messages);
            }
        }

        public void NotifyTurnPassed(List<string> messages)
        {
            List<Effect> effects = new List<Effect>(activeEffects);

            foreach (Effect effect in effects)
            {
                if (!activeEffects.Contains(effect))
                    continue;

                if (effect.IsActive)
                    effect.TurnPassed(messages);
            }
        }

    }
}
