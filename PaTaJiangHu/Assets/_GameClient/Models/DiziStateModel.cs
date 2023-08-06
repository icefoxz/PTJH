using System.Collections;
using System.Collections.Generic;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子状态模型, 用于管理弟子状态相关信息, 如: 历练, 生产, 修炼, 休息等
    /// </summary>
    public abstract class DiziStateModel : ModelBase, IReadOnlyDictionary<string,Dizi>
    {
        protected readonly Dictionary<string, Dizi> _data = new Dictionary<string, Dizi>();

        public void AddDizi(Dizi dizi) => _data.Add(dizi.Guid, dizi);
        public void RemoveDizi(Dizi dizi) => _data.Remove(dizi.Guid);
        public bool Contains(Dizi dizi) => _data.ContainsKey(dizi.Guid);
        #region IDictionary
        public IEnumerator<KeyValuePair<string, Dizi>> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _data.GetEnumerator();
        public int Count => _data.Count;
        public bool ContainsKey(string key) => _data.ContainsKey(key);
        public bool TryGetValue(string key, out Dizi value) => _data.TryGetValue(key, out value);
        public Dizi this[string key] => _data[key];
        public IEnumerable<string> Keys => _data.Keys;
        public IEnumerable<Dizi> Values => _data.Values;
        #endregion
    }
}