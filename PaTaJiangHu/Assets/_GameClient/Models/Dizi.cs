using System;
using System.Collections.Generic;
using BattleM;
using Server.Controllers.Adventures;
using UnityEditor;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public class Dizi
    {
        public Guid Guid { get; }
        public string Name { get; }
        public int Strength { get; }
        public int Agility { get; }
        public int Hp { get; }
        public int Mp { get; }
        public int Level { get; }
        public int Grade { get; }

        private StaminaModel Stamina { get; }
        private DiziSkills Skills { get; }
        private DiziBag Bag { get; }

        public Dizi(string name, int strength, int agility, 
            int hp, int mp, int level, int grade,
            int stamina, int bag,
            int combatSlot, int forceSlot, int dodgeSlot)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
            Level = level;
            Grade = grade;
            Stamina = new StaminaModel(stamina);
            Skills = new DiziSkills(combatSlot, forceSlot, dodgeSlot);
            Bag = new DiziBag(bag);
        }

        public void UpdateStamina(int value,long lastUpdate)
        {
            Stamina.Update(value, lastUpdate);
            var arg = Json.Serialize(new long[] { Stamina.Con.Value, Stamina.Con.Max, lastUpdate });
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
            public StaminaModel(int value)
            {
                _con = new ConValue(value, value, value);
                _lastUpdate = SysTime.UnixNow;
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
            public int BagSlot { get; }
            public int SkillSlot { get; }
        }
        //弟子技能栏
        private class DiziSkills
        {
            public IDodgeSkill[] DodgeSkills { get; }
            public ICombatSkill[] CombatSkills { get; }
            public IForceSkill[] ForceSkills { get; }

            public DiziSkills(ICombatSkill[] combatSlot, IForceSkill[] forceSkill, IDodgeSkill[] dodgeSlot)
            {
                DodgeSkills = dodgeSlot;
                CombatSkills = combatSlot;
                ForceSkills = forceSkill;
            }
            public DiziSkills(int combatSlot, int forceSkill, int dodgeSlot)
            {
                DodgeSkills = new IDodgeSkill[dodgeSlot];
                CombatSkills = new ICombatSkill[combatSlot];
                ForceSkills = new IForceSkill[forceSkill];
            }
        }
        //弟子背包
        private class DiziBag 
        {
            private IGameItem[] _items;
            public IGameItem[] Items => _items;

            public DiziBag(int length)
            {
                _items = new IGameItem[length];
            }
            public DiziBag(IGameItem[] items)
            {
                _items = items;
            }

        }
        //弟子钱包
        private class DiziWallet
        {
            public int Silver { get; private set; }
            public int MaxSilver { get; private set; }

            public DiziWallet(int silver, int maxSilver)
            {
                Silver = silver;
                MaxSilver = maxSilver;
            }

            public void ClearSilver() => Silver = 0;
            public void TradeSilver(int silver, bool throwIfLessThanZero = false)
            {
                Silver += silver;
                if (throwIfLessThanZero && Silver < 0)
                    throw new InvalidOperationException($"{nameof(TradeSilver)}: silver = {Silver}");
            }
        }
    }
}