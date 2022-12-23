using System;
using System.Collections.Generic;
using BattleM;
using Server.Controllers.Adventures;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public class Dizi
    {
        private DiziInventory _inventory;
        private StaminaModel _stamina;
        private Slots _slots;
        public string Name { get; }
        public int Strength { get; }
        public int Agility { get; }
        public int Power { get; }

        public int Hp { get; }
        public int Mp { get; }
        public int Level { get; }
        public int Grade { get; }

        private StaminaModel Stamina => _stamina;
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
            _stamina = new StaminaModel(d.Stamina, d.StaminaMax, d.StaminaUpdate);
            _inventory = new DiziInventory(new List<IGameItem>());
        }

        public void UpdateStamina(int value,long lastUpdate)
        {
            _stamina.Update(value, lastUpdate);
            var arg = Json.Serialize(new long[] { _stamina.Con.Value, _stamina.Con.Max, lastUpdate });
            Game.MessagingManager.Invoke(EventString.Model_DiziInfo_StaminaUpdate, arg);

        }

        private class StaminaModel
        {
            private ConValue _con;
            private long _lastUpdate;

            public IGameCondition Con => _con;

            public long LastUpdate => _lastUpdate;

            public StaminaModel(int value,int max, long lastUpdate)
            {
                _con = new ConValue(max, max, value);
                _lastUpdate = lastUpdate;
            }

            public void Update(int current, long lastUpdate)
            {
                Con.Set(current);
                _lastUpdate = lastUpdate;
            }
        }

        private class StateModel
        {
            public enum States
            {
                Idle,
                Adventure
            }
            public string ShortTitle { get; private set; }
            public string Description { get; private set; }
            public long LastUpdate { get; private set; }
            public States Current { get; private set; }

            public void Set(States state)
            {
                Current = state;
                LastUpdate = SysTime.UnixNow;
                ShortTitle = GetShort(state);
                Description = GetDescription(state);
            }

            private string GetDescription(States state) => state switch
            {
                States.Idle => "发呆中...",
                States.Adventure => "历练中...",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
            private string GetShort(States state) => state switch
            {
                States.Idle => "闲",
                States.Adventure => "历",
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
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