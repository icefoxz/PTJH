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
        /// 轻功格
        /// </summary>
        public int DodgeSlot { get; private set; }
        /// <summary>
        /// 武功格
        /// </summary>
        public int CombatSlot { get; private set; }
        /// <summary>
        /// 背包格
        /// </summary>
        public int Bag { get; private set; }

        public Capable()
        {
            
        }
        public Capable(int grade, int dodgeSlot, int combatSlot, int bag, GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp)
        {
            Grade = grade;
            DodgeSlot = dodgeSlot;
            CombatSlot = combatSlot;
            Bag = bag;
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
        }

        public Capable(Capable c)
        {
            Grade = c.Grade;
            DodgeSlot = c.DodgeSlot;
            CombatSlot = c.CombatSlot;
            Bag = c.Bag;
            Strength = c.Strength;
            Agility = c.Agility;
            Hp = c.Hp;
            Mp = c.Mp;
        }
    }
}