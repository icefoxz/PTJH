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
        /// 包裹, 游戏物品的容器
        /// </summary>
        Package,
        /// <summary>
        /// 故事道具
        /// </summary>
        StoryProps,
        /// <summary>
        /// 历练道具
        /// </summary>
        AdvItems,
    }

    public interface IGameItem
    {
        int Id { get; }
        string Name { get; }
        string About { get; }
        ItemType Type { get; }
        int Price { get; }
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