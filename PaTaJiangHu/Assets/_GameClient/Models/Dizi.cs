﻿using System;
using BattleM;
using Server.Configs.Adventures;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEditor;
using UnityEditor.VersionControl;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public class Dizi : ITerm
    {
        public string Guid { get; private set; }
        public string Name { get; private set; }
        public int Strength { get; private set; }
        public int Agility { get; private set; }
        public int Hp { get; private set; }
        public int Mp { get; private set; }
        public int Level { get; private set; }
        public int Grade { get; private set; }
        public ICombatSkill CombatSkill { get; set; }
        public IForceSkill ForceSkill { get; set; }
        public IDodgeSkill DodgeSkill { get; set; }
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }
        /// <summary>
        /// 武器战力
        /// </summary>
        public int WeaponPower => Weapon?.Damage ?? 0;
        /// <summary>
        /// 防具战力
        /// </summary>
        public int ArmorPower => Armor?.Def ?? 0;


        public IDiziStamina Stamina => _stamina;
        private DiziStamina _stamina;
        private ConValue _food;
        private ConValue _emotion;
        private ConValue _silver;
        private ConValue _injury;
        private ConValue _inner;

        public Skills Skill { get; set; }
        public Capable Capable { get; private set; }

        public IConditionValue Food => _food;
        public IConditionValue Emotion => _emotion;
        public IConditionValue Silver => _silver;
        public IConditionValue Injury => _injury;
        public IConditionValue Inner => _inner;

        public AutoAdventure Adventure { get; set; }

        internal Dizi(string guid, string name, 
            GradeValue<int> strength, 
            GradeValue<int> agility,
            GradeValue<int> hp, 
            GradeValue<int> mp, 
            int level, int grade,
            int stamina, int bag,
            int combatSlot, int forceSlot, int dodgeSlot,
            ICombatSkill combatSkill, IForceSkill forceSkill, IDodgeSkill dodgeSkill)
        {
            Guid = guid;
            Name = name;
            Strength = strength.Value;
            Agility = agility.Value;
            Hp = hp.Value;
            Mp = mp.Value;
            Level = level;
            Grade = grade;
            Capable = new Capable(grade, dodgeSlot, combatSlot, bag, strength, agility, hp, mp);
            _stamina = new DiziStamina(Game.Controllers.Get<StaminaController>(), stamina);
            CombatSkill = combatSkill;
            ForceSkill = forceSkill;
            DodgeSkill = dodgeSkill;
            var cSlot = new ICombatSkill[combatSlot];
            cSlot[0] = combatSkill;
            var fSlot = new IForceSkill[forceSlot];
            fSlot[0] = forceSkill;
            var dSlot = new IDodgeSkill[dodgeSlot];
            dSlot[0] = dodgeSkill;
            Skill = new Skills(cSlot, fSlot, dSlot);
            _food = new ConValue(100);
            _emotion = new ConValue(100);
            _silver = new ConValue(100);
            _injury = new ConValue(100);
            _inner = new ConValue(100);
        }

        #region ITerm

        IConditionValue ITerm.Stamina => Stamina.Con;
        #endregion

        internal void UpdateStamina(long ticks)
        {
            _stamina.Update(ticks);
            Game.MessagingManager.SendParams(EventString.Dizi_Params_StaminaUpdate, Stamina.Con.Value, Stamina.Con.Max);
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
        public class Skills
        {
            public IDodgeSkill[] DodgeSkills { get; private set; }
            public ICombatSkill[] CombatSkills { get; private set; }
            public IForceSkill[] ForceSkills { get; private set; }

            public Skills()
            {
                
            }
            public Skills(ICombatSkill[] combatSlot, IForceSkill[] forceSkill, IDodgeSkill[] dodgeSlot)
            {
                DodgeSkills = dodgeSlot;
                CombatSkills = combatSlot;
                ForceSkills = forceSkill;
            }

            public Skills(int combatSlot, int forceSkill, int dodgeSlot)
            {
                DodgeSkills = new IDodgeSkill[dodgeSlot];
                CombatSkills = new ICombatSkill[combatSlot];
                ForceSkills = new IForceSkill[forceSkill];
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

        internal void Wield(IWeapon weapon)
        {
            Weapon = weapon;
            XDebug.Log(weapon == null ? "弟子卸下装备" : $"弟子装备{weapon.Name}!");
        }

        internal void Wear(IArmor armor)
        {
            Armor = armor;
            XDebug.Log(armor == null ? "弟子卸下装备" : $"弟子装备{armor.Name}!");
        }

        internal void StartAdventure(long startTime,int returnSecs,int messageSecs)
        {
            Adventure = new AutoAdventure(startTime, returnSecs, messageSecs, this);
            Game.MessagingManager.Send(EventString.Dizi_Adv_Start, Guid);
        }

        internal void StartAdventureStory(DiziAdventureStory story)
        {
            if (Adventure.State == AutoAdventure.States.None)
                throw new NotImplementedException();
            Adventure.RegStory(story);
        }
        internal void QuitAdventure(long now,int lastMile)
        {
            Adventure.Quit(now, lastMile);
            Game.MessagingManager.Send(EventString.Dizi_Adv_Recall, Guid);
        }

        public void SetCon(IAdjustment.Types type, int value)
        {
            var con = type switch
            {
                IAdjustment.Types.Stamina => throw new NotImplementedException("体力状态不允许这里设!请用StaminaController"),
                IAdjustment.Types.Silver => _silver,
                IAdjustment.Types.Food => _food,
                IAdjustment.Types.Condition => _emotion,
                IAdjustment.Types.Injury => _injury,
                IAdjustment.Types.Inner => _inner,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            con.Add(value);
            Game.MessagingManager.SendParams(EventString.Dizi_ConditionUpdate, type, value);
        }
    }

    /// <summary>
    /// 资质能力
    /// </summary>
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

        public Capable()
        {
            
        }
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

        public Capable(Capable c)
        {
            Grade = c.Grade;
            DodgeSlot = c.DodgeSlot;
            CombatSlot = c.CombatSlot;
            Bag = c.Bag;
            Strength = c.Strength;
            Agility = c.Agility;
            Hp = c.Hp;
            Mp = c.Mp;
        }
    }
}