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

        public Unit(DUnit unit, BattlePosition position)
        {
            data = unit;
            currentHP = unit.Stats.hp;
            currentMP = unit.Stats.mana;
            remainingLives = data.Lives;
            inventory = new Inventory(data.Inventory.Length, data.Inventory);
            this.position = position;
        }

        public Stats Stats
        {
            get
            {
                Stats total = data.Stats;
                if (weaponSlot >= 0 && weaponSlot <= inventory.InventorySize)
                    total += inventory[weaponSlot] != null ? ((DWeapon)inventory[weaponSlot]).Stats : new Stats();
                total += AssetManager.Instance.GetClass(data.ClassID).Stats;
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
        public bool IsAlive => currentHP >= 0;
        public DUnit Data => data;

        public void NewTurn()
        {
            hasActed = false;
            hasMoved = false;
        }

        
    }
}
