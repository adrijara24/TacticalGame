using System;
using System.Collections.Generic;
using System.Text;

namespace Tactical.Core.Domain
{

    public enum EDamageType
    {
        PHYSICAL = 1, MAGICAL = 2, PURE = 4, HEALING = 8
    }

    struct DamageInstance
    {        
        readonly int value;
        readonly EDamageType damageType;

        public DamageInstance(int value, EDamageType damageType)
        {
            this.value = value;
            this.damageType = damageType;
        }

        public int Value => value;
        public EDamageType DamageType => damageType;

        /// <summary>
        /// Calculates the new HP value of the target receiving this instance
        /// </summary>
        /// <param name="HP">Current HP</param>
        /// <param name="maxHP">Max HP of the target</param>
        /// <param name="def">Defense of the target</param>
        /// <param name="res">Resistance of the target</param>
        /// <returns>HP value after processing this instance</returns>
        public int Apply(int HP, int maxHP, int def, int res)
        {
            int newHP = HP;
            switch(damageType)
            {
                case EDamageType.PHYSICAL:
                    newHP -= Math.Max(0, value - def);
                    break;
                case EDamageType.MAGICAL:
                    newHP -= Math.Max(0, value - res);
                    break;
                case EDamageType.PURE:
                    newHP -= Math.Max(0, value);
                    break;
                case EDamageType.HEALING:
                    newHP += value;
                    break;
            }

            return Math.Min(maxHP, Math.Max(0, newHP));

        }
    }
}
