using System;
using System.Collections.Generic;
using BattleM;
using Server.Controllers.Adventures;
using Server.Controllers.Characters;
using UnityEditor;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public class Dizi
    {
        public Guid Guid { get; private set; }
        public string Name { get; private set; }
        public int Strength { get; private set; }
        public int Agility { get; private set; }
        public int Hp { get; private set; }
        public int Mp { get; private set; }
        public int Level { get; private set; }
        public int Grade { get; private set; }

        private StaminaModel Stamina { get; set; }
        private DiziSkills Skills { get; set; }
        private DiziBag Bag { get; set; }
        public Capable Capable { get; private set; }

        public Dizi()
        {
            
        }
        public Dizi(string name, GradeValue<int> strength, GradeValue<int> agility, 
            GradeValue<int> hp, GradeValue<int> mp, int level, int grade,
            int stamina, int bag,
            int combatSlot, int forceSlot, int dodgeSlot)
        {
            Guid = Guid.NewGuid();
            Name = name;
            Strength = strength.Value;
            Agility = agility.Value;
            Hp = hp.Value;
            Mp = mp.Value;
            Level = level;
            Grade = grade;
            Capable = new Capable(grade, dodgeSlot, combatSlot, bag, strength, agility, hp, mp);
            Stamina = new StaminaModel(stamina);
            Skills = new DiziSkills(combatSlot, forceSlot, dodgeSlot);
            Bag = new DiziBag(bag);
        }

        public void UpdateStamina(int value,long lastUpdate)
        {
            Stamina.Update(value, lastUpdate);
            var arg = Json.Serialize(new long[] { Stamina.Con.Value, Stamina.Con.Max, lastUpdate });
            Game.MessagingManager.Send(EventString.Model_DiziInfo_StaminaUpdate, arg);

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
        //弟子技能栏
        private class DiziSkills
        {
            public IDodgeSkill[] DodgeSkills { get; private set; }
            public ICombatSkill[] CombatSkills { get; private set; }
            public IForceSkill[] ForceSkills { get; private set; }

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
    public class Capable
    {
        /// <summary>
        /// 品级
        /// </summary>
        public int Grade { get; private set; }
        public GradeValue<int> Strength { get; private set; }
        public GradeValue<int> Agility { get; private set; }
        public GradeValue<int> Hp { get; private set; }
        public GradeValue<int> Mp { get; private set; }
        /// <summary>
        /// 轻功格
        /// </summary>
        public int DodgeSlot { get; private set; }
        /// <summary>
        /// 武功格
        /// </summary>
        public int CombatSlot { get; private set; }
        /// <summary>
        /// 背包格
        /// </summary>
        public int Bag { get; private set; }

        public Capable(int grade, int dodgeSlot, int combatSlot, int bag, GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp)
        {
            Grade = grade;
            DodgeSlot = dodgeSlot;
            CombatSlot = combatSlot;
            Bag = bag;
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
        }
    }

}