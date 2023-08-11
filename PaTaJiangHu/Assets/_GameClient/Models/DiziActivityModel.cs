using System.Collections.Generic;
using GameClient.Modules.Adventure;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子活动模型管理器, 用于管理弟子状态相关信息, 如: 历练, 生产, 修炼, 休息等
    /// </summary>
    public abstract class DiziActivityModel<TPoller> : ModelBase where TPoller : DelayedPoller
    {
        private readonly Dictionary<string, TPoller> _data = new Dictionary<string, TPoller>();
        protected IReadOnlyDictionary<string, TPoller> Data => _data;

        protected void Add(string key,TPoller obj) => _data.Add(key, obj);
        protected void Remove(string key) => _data.Remove(key);
        public bool Contains(string key) => _data.ContainsKey(key);
        protected TPoller GetPoller(string key) => _data.TryGetValue(key, out var handler) ? handler : default;
    }
}