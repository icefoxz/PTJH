using System;
using BattleM;
using Core;
using Server.Configs.Adventures;
using Server.Configs.Characters;
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
    public class Dizi : ModelBase, ITerm
    {
        protected override string LogPrefix => Name;
        public string Guid { get; }
        public string Name { get; }
        public int Strength => LevelCfg.GetLeveledValue(LevelConfigSo.Props.Strength, Capable.Strength.Value, Level);
        public int Agility => LevelCfg.GetLeveledValue(LevelConfigSo.Props.Agility, Capable.Agility.Value, Level);
        public int Hp => LevelCfg.GetLeveledValue(LevelConfigSo.Props.Hp, Capable.Hp.Value, Level);
        public int Mp => LevelCfg.GetLeveledValue(LevelConfigSo.Props.Mp, Capable.Mp.Value, Level);
        public int Level { get; private set; }

        public IConditionValue Exp => _exp;

        public int Power { get; }
        public IStacking<IGameItem>[] Items => Adventure?.BagItems?? Array.Empty<IStacking<IGameItem>>();
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

        private ConValue _exp;

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

        private LevelConfigSo LevelCfg => Game.Config.DiziCfg.LevelConfigSo;

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
            Log($"体力更新 = {_stamina.Con}");
            SendEvent(EventString.Dizi_Params_StaminaUpdate, Stamina.Con.Value, Stamina.Con.Max);
        }

        internal void SetLevel(int level)
        {
            Level = level;
            Log($"等级设置 = {Level}");
        }

        internal void SetExp(int exp, int max = 0)
        {
            if (max > 0) _exp.SetMax(max);
            _exp.Set(exp);
            Log($"经验设置 = {_exp}");
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
            Log(weapon == null ? $"卸下{Weapon.Name}" : $"装备{weapon.Name}!");
            Weapon = weapon;
        }
        internal void Wear(IArmor armor)
        {
            Log(armor == null ? $"卸下{Armor.Name}" : $"装备{armor.Name}!");
            Armor = armor;
        }
        internal void StartAdventure(long startTime,int returnSecs,int messageSecs)
        {
            Adventure = new AutoAdventure(startTime, returnSecs, messageSecs, this);
            Log("开始历练.");
            SendEvent(EventString.Dizi_Adv_Start, Guid);
        }
        internal void StartAdventureStory(DiziAdvLog story)
        {
            if (Adventure.State == AutoAdventure.States.None)
                throw new NotImplementedException();
            Adventure.RegStory(story);
        }
        internal void QuitAdventure(long now,int lastMile)
        {
            Adventure.Quit(now, lastMile);
            Log($"停止历练, 里数: {lastMile}");
            SendEvent(EventString.Dizi_Adv_Recall, Guid);
        }
        internal void SetCon(IAdjustment.Types type, int value)
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
            Log($"状态[{type}]设置: {con}");
            SendEvent(EventString.Dizi_ConditionUpdate, type, value);
        }
        //internal void SetProps(int str, int agi, int hp, int mp)
        //{
        //    Hp = hp;
        //    Mp = mp;
        //    Strength = str;
        //    Agility = agi;
        //    Log($"属性更新: 力【{Strength}】,敏【{Agility}】,血【{Hp}】,内【{Mp}】");
        //    SendEvent(EventString.Dizi_Props_Update, Strength, Agility, Hp, Mp);
        //}
    }
}