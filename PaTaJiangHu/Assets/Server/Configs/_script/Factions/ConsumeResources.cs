using UnityEngine;

namespace Server.Configs.Factions
{
    /// <summary>
    /// 游戏消耗资源
    /// </summary>
    public enum ConsumeResources
    {
        [InspectorName("食物")]Food,
        [InspectorName("酒水")]Wine,
        [InspectorName("药草")]Herb,
        [InspectorName("丹药")]Pill,
    }
}