﻿using System.Collections.Generic;
using Server.Controllers.Adventures;

namespace _GameClient.Models
{
    public interface IDiziInfo
    {
        string Name { get; }
        int Strength { get; }
        int Agility { get; }
        int Hp { get; }
        int MaxHp { get; }
        int Mp { get; }
        int MaxMp { get; }
        int Level { get; }
        /// <summary>
        /// 品级
        /// </summary>
        int Grade { get; }
        /// <summary>
        /// 轻功格
        /// </summary>
        int DodgeSlot { get; }
        /// <summary>
        /// 武功格
        /// </summary>
        int MartialSlot { get; }
        /// <summary>
        /// 背包格
        /// </summary>
        int InventorySlot { get; }
        /// <summary>
        /// 当前体力
        /// </summary>
        int Stamina { get; set; }
        /// <summary>
        /// 最大体力
        /// </summary>
        int StaminaMax { get; set; }
        /// <summary>
        /// 上次体力更新
        /// </summary>
        long StaminaUpdate { get; set; }
    }

    /// <summary>
    /// 门派
    /// </summary>
    public class Faction
    {
        public int Silver { get; }
        public int YuanBao { get; }
        public int ActionLing { get; }
        public int Level { get; }
        public IReadOnlyList<Dizi> DiziList { get; }
    }

    public interface IInventory
    {
        IReadOnlyList<IGameItem> Items { get; }
        int Silver { get; }
    }
}