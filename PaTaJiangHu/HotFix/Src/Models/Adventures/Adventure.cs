using System;
using System.Collections;
using System.Collections.Generic;

namespace HotFix_Project.Models.Adventures
{
    /// <summary>
    /// 冒险实例，<see cref="AdEvent"/>(冒险事件)的管理器。
    /// </summary>
    internal class Adventure
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AdEvent StartNode { get; set; }
        public AdEvent Current { get; private set; }
        public IReadOnlyList<AdEvent> History => _history;
        private List<AdEvent> _history;

        public Adventure(int id, string name, AdEvent startNode)
        {
            Id = id;
            Name = name;
            StartNode = startNode;
            Current = startNode;
        }
        public void RegEvent(AdEvent adEvent)
        {
            if (Current.Result == 0)
                throw new InvalidOperationException($"当前事件[{Current.Id}]{Current.Name}还未结束，不允许注册新事件");
            _history.Add(adEvent);
            Current = adEvent;
        }
    }

    /// <summary>
    /// 冒险事件
    /// </summary>
    internal class AdEvent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 结果Id。0=未结束
        /// </summary>
        public int Result { get; set; }
    }
}
