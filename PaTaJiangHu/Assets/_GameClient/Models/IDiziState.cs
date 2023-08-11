using System;

namespace GameClient.Models
{
    public interface IDiziState
    {
        DiziActivities Activity { get; }
        /// <summary>
        /// 状态开始时间
        /// </summary>
        long StartTime { get; }
        /// <summary>
        /// 最后一次更新
        /// </summary>
        long LastUpdate { get; }
        int LastMiles { get; }
        /// <summary>
        /// 状态标签
        /// </summary>
        string ShortTitle { get; }
        /// <summary>
        /// 事件描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 事件场景
        /// </summary>
        string CurrentOccasion { get; }
        /// <summary>
        /// 当前地图
        /// </summary>
        string CurrentMapName { get; }
        /// <summary>
        /// 时间描述
        /// </summary>
        string StateLabel { get; }
        /// <summary>
        /// 事件经过时间
        /// </summary>
        TimeSpan CurrentProgressTime { get; }
    }
}