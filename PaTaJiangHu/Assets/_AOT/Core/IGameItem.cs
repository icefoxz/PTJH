using UnityEngine;

namespace AOT.Core
{
    /// <summary>
    /// 游戏物品大类(功能性, 不细化)
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// 装备
        /// </summary>
        Equipment,
        /// <summary>
        /// 书籍
        /// </summary>
        Book,
        /// <summary>
        /// 功能道具
        /// </summary>
        FunctionItem,
    }

    public interface IGameItem
    {
        int Id { get; }
        Sprite Icon { get; }
        string Name { get; }
        string About { get; }
        ItemType Type { get; }
    }
}