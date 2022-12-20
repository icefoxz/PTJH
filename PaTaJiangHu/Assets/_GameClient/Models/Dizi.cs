using System;
using System.Collections.Generic;
using BattleM;
using Server.Controllers.Adventures;

namespace _Game.Models
{
    /// <summary>
    /// 弟子
    /// </summary>
    public class Dizi
    {
        private DiziInventory _inventory;
        private ConValue _stamina;
        private Slots _slots;
        public string Name { get; }
        public int Strength { get; }
        public int Agility { get; }
        public int Power { get; }

        public int Hp { get; }
        public int Mp { get; }
        public int Level { get; }
        public int Grade { get; }

        public IConditionValue Stamina => _stamina;
        public IInventory Inventory => _inventory;

        public Dizi(IDiziInfo d)
        {
            Name = d.Name;
            Strength = d.Strength;
            Agility = d.Agility;
            Hp = d.Hp;
            Mp = d.Mp;
            Level = d.Level;
            Grade = d.Grade;
            _slots = new Slots(d.DodgeSlot, d.MartialSlot, d.InventorySlot);
            _stamina = new ConValue(d.Stamina);
            _inventory = new DiziInventory(new List<IGameItem>());
        }

        private class Slots
        {
            public int DodgeSlot { get; }
            public int MartialSlot { get; }
            public int InventorySlot { get; }

            public Slots(int dodgeSlot, int martialSlot, int inventorySlot)
            {
                DodgeSlot = dodgeSlot;
                MartialSlot = martialSlot;
                InventorySlot = inventorySlot;
            }
        }

        private class DiziInventory : IInventory
        {
            private List<IGameItem> _items;
            private int _silver;
            public IReadOnlyList<IGameItem> Items => _items;
            public int Silver => _silver;
            public DiziInventory(List<IGameItem> items)
            {
                _items = items;
            }
            public void TradeSilver(int silver,bool throwIfLessThanZero = false)
            {
                _silver += silver;
                if (throwIfLessThanZero && _silver < 0)
                    throw new InvalidOperationException($"{nameof(TradeSilver)}: silver = {_silver}");
            }
            public void ClearSilver() => _silver = 0;
            public void AddItem(IGameItem item) => _items.Add(item);
            public void ClearItems() => _items.Clear();
            public bool RemoveItem(IGameItem item) => _items.Remove(item);
            public void ClearAll()
            {
                ClearItems();
                ClearSilver();
            }
        }
    }
}