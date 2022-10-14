using System.Collections.Generic;

namespace HotFix_Project.Models.Charators
{
    internal class Dizi
    {
        public string Name { get; private set; }
        public int Strength { get; private set; }
        public int Agility { get; private set; }
        public int Hp { get; private set; }
        public int Tp { get; private set; }
        public int Mp { get; private set; }
        public int Level { get; private set; }
        public int Stamina { get; private set; }
        public Capable Capable { get; private set; }
        public Dictionary<int, int> Condition { get; private set; }
    }

    internal class Capable
    {
        /// <summary>
        /// 品级
        /// </summary>
        public int Grade { get; private set; }
        /// <summary>
        /// 轻功格
        /// </summary>
        public int DodgeSlot { get; private set; }
        /// <summary>
        /// 武功格
        /// </summary>
        public int MartialSlot { get; private set; }
        /// <summary>
        /// 背包格
        /// </summary>
        public int InventorySlot { get; private set; }
    }
}