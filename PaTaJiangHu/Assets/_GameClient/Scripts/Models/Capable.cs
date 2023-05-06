using System;
using System.Diagnostics;
using Server.Configs.Factions;
using Server.Configs.Skills;

namespace _GameClient.Models
{
    /// <summary>
    /// 资质能力
    /// </summary>
    public class Capable
    {
        /// <summary>
        /// 品级
        /// </summary>
        public int Grade { get; private set; }
        public GradeValue<int> Strength { get; private set; }
        public GradeValue<int> Agility { get; private set; }
        public GradeValue<int> Hp { get; private set; }
        public GradeValue<int> Mp { get; private set; }

        /// <summary>
        /// 武功格
        /// </summary>
        public int CombatSlot { get; private set; }
        /// <summary>
        /// 内功格
        /// </summary>
        public int ForceSlot { get; private set; }
        /// <summary>
        /// 轻功格
        /// </summary>
        public int DodgeSlot { get; private set; }
        /// <summary>
        /// 背包格
        /// </summary>
        public int Bag { get; private set; }

        public int Food { get; }
        public int Wine { get; }
        public int Herb { get; }
        public int Pill { get; }

        public Capable()
        {
        }

        public Capable(int grade, int combatSlot,int forceSlot, int dodgeSlot, int bag, GradeValue<int> strength,
            GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp, int food, int wine, int herb,
            int pill)
        {
            Grade = grade;
            DodgeSlot = dodgeSlot;
            CombatSlot = combatSlot;
            ForceSlot = forceSlot;
            Bag = bag;
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
            Food = food;
            Wine = wine;
            Herb = herb;
            Pill = pill;
        }

        public Capable(Capable c)
        {
            Food = c.Food;
            Wine = c.Wine;
            Herb = c.Herb;
            Pill = c.Pill;
            Grade = c.Grade;
            DodgeSlot = c.DodgeSlot;
            CombatSlot = c.CombatSlot;
            Bag = c.Bag;
            Strength = c.Strength;
            Agility = c.Agility;
            Hp = c.Hp;
            Mp = c.Mp;
        }

        public int GetConsume(ConsumeResources resource) =>
            resource switch
            {
                ConsumeResources.Food => Food,
                ConsumeResources.Wine => Wine,
                ConsumeResources.Herb => Herb,
                ConsumeResources.Pill => Pill,
                _ => throw new ArgumentOutOfRangeException(nameof(resource), resource, null)
            };
    }
}