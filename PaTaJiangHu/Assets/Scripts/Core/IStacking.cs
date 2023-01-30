using Server.Configs.Adventures;

namespace Core
{
    /// <summary>
    /// 游戏物品大类(功能性, 不细化)
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// 丹药
        /// </summary>
        Medicine,
        /// <summary>
        /// 装备
        /// </summary>
        Equipment,
        /// <summary>
        /// 书籍
        /// </summary>
        Book,
        /// <summary>
        /// 故事道具
        /// </summary>
        StoryProps,
        /// <summary>
        /// 历练道具
        /// </summary>
        AdvItems,
    }

    /// <summary>
    /// 装备类型
    /// </summary>
    public enum EquipKinds
    {
        Weapon,
        Armor
    }

    public interface IEquipment : IGameItem
    {
        EquipKinds EquipKind { get; }
    }

    public interface IGameItem
    {
        public int Id { get; }
        public string Name { get; }
        public string About { get; }
        public ItemType Type { get; }
        public int Price { get; }
    }
    /// <summary>
    /// 堆叠
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IStacking<out T> 
    {
        T Item { get; }
        int Amount { get; }
    }
}