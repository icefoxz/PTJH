using System.Collections;
using System.Collections.Generic;
using AOT.Core;

namespace GameClient.Models
{
    public class ItemList<T> : IReadOnlyList<T> 
    {
        private List<T> _items = new List<T>();
        public ItemList(List<T> items) => _items = items;
        public ItemList() { }
        public void Add(T item) => _items.Add(item);
        public void AddRange(IEnumerable<T> items) => _items.AddRange(items);
        public bool Remove(T item) => _items.Remove(item);
        public IEnumerator<T> GetEnumerator()=> _items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public int Count => _items.Count;
        public T this[int index] => _items[index];
    }

    public class StackableItemList
    {
        private Dictionary<int, int> _items = new Dictionary<int, int>();
        public void Add(IGameItem item, int quantity)
        {
            _items.TryAdd(item.Id, 0);
            _items[item.Id] += quantity;
        }
        public bool Remove(IGameItem item, int quantity)
        {
            if (!_items.ContainsKey(item.Id)) return false;
            _items[item.Id] -= quantity;
            if (_items[item.Id] <= 0) 
                _items.Remove(item.Id);
            return true;
        }
        public int GetQuantity(IGameItem item) => _items.TryGetValue(item.Id, out var quantity) ? quantity : 0;
        public IReadOnlyDictionary<int, int> GetAllItems() => _items;
        // 其他管理可叠加物品的方法...
    }

}