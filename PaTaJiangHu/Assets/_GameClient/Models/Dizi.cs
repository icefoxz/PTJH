using System;
using System.Collections;
using System.Collections.Generic;
using BattleM;
using Core;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Analytics;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public partial class Dizi : ModelBase, ITerm
    {
        protected override string LogPrefix => Name;
        public string Guid { get; }
        public string Name { get; }
        public Gender Gender { get; }

        public int Strength => GetLeveledValue(DiziProps.Strength) + 
                               GetPropStateAddon(DiziProps.Strength) +
                               GetWeaponDamage();
        public int Agility => GetLeveledValue(DiziProps.Agility) + 
                              GetPropStateAddon(DiziProps.Agility);
        public int Hp => GetLeveledValue(DiziProps.Hp) + 
                         GetPropStateAddon(DiziProps.Hp);
        public int Mp => GetLeveledValue(DiziProps.Mp) + 
                         GetPropStateAddon(DiziProps.Mp);

        public int Level { get; private set; }
        public IConditionValue Exp => _exp;

        public int Power => ConProCfg.GetPower(Strength, Agility, WeaponPower, ArmorPower,
            CombatSkill?.Grade ?? 0, CombatSkill?.Level ?? 0,
            ForceSkill?.Grade ?? 0, ForceSkill?.Level ?? 0,
            DodgeSkill?.Grade ?? 0, DodgeSkill?.Level ?? 0);
        public int Grade { get; }
        public ICombatSkill CombatSkill { get; private set; }
        public IForceSkill ForceSkill { get; private set; }
        public IDodgeSkill DodgeSkill { get; private set; }
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }

        /// <summary>
        /// 武器战力
        /// </summary>
        public int WeaponPower => GetWeaponDamage();
        /// <summary>
        /// 防具战力
        /// </summary>
        public int ArmorPower => GetArmorDef();
        public StateModel State { get; private set; } = new StateModel();

        private ConValue _exp;

        public IDiziStamina Stamina => _stamina;
        private DiziStamina _stamina;
        private ConValue _food;
        private ConValue _emotion;
        private ConValue _silver;
        private ConValue _injury;
        private ConValue _inner;
        private AdvItemModel[] _advItems = new AdvItemModel[3];

        public Skills Skill { get; set; }
        public Capable Capable { get; private set; }

        public AdvItemModel[] AdvItems => _advItems;

        public IConditionValue Food => _food;
        public IConditionValue Emotion => _emotion;
        public IConditionValue Silver => _silver;
        public IConditionValue Injury => _injury;
        public IConditionValue Inner => _inner;


        private LevelConfigSo LevelCfg => Game.Config.DiziCfg.LevelConfigSo;
        private PropStateConfigSo PropState => Game.Config.DiziCfg.PropState;
        private PropStateConfigSo.ConfigField PropStateCfg => Game.Config.DiziCfg.PropState.Config;
        private ConditionPropertySo ConProCfg => Game.Config.AdvCfg.ConditionProperty;
        internal Dizi(string guid, string name,
            Gender gender,
            int level, 
            int stamina, 
            Capable capable,
            ICombatSkill combatSkill, IForceSkill forceSkill, IDodgeSkill dodgeSkill)
        {
            Guid = guid;
            Name = name;
            Gender = gender;
            Level = level;
            Grade = capable.Grade;
            Capable = capable;
            _stamina = new DiziStamina(Game.Controllers.Get<StaminaController>(), stamina);
            CombatSkill = combatSkill;
            ForceSkill = forceSkill;
            DodgeSkill = dodgeSkill;
            var cSlot = new ICombatSkill[capable.CombatSlot];
            cSlot[0] = combatSkill;
            var fSlot = new IForceSkill[1];
            fSlot[0] = forceSkill;
            var dSlot = new IDodgeSkill[capable.DodgeSlot];
            dSlot[0] = dodgeSkill;
            Skill = new Skills(cSlot, fSlot, dSlot);
            var maxExp = LevelCfg.GetMaxExp(level);
            _exp = new ConValue(maxExp, maxExp, 0);
            _food = new ConValue(PropStateCfg.FoodDefault.Max, PropStateCfg.FoodDefault.Min);
            _emotion = new ConValue(PropStateCfg.EmotionDefault.Max, PropStateCfg.EmotionDefault.Min);
            _silver = new ConValue(PropStateCfg.SilverDefault.Max, PropStateCfg.SilverDefault.Min);
            _injury = new ConValue(PropStateCfg.InjuryDefault.Max, PropStateCfg.InjuryDefault.Min);
            _inner = new ConValue(PropStateCfg.InnerDefault.Max, PropStateCfg.InnerDefault.Min);
            StaminaService();
        }

        private void StaminaService()
        {
            Game.CoService.RunCo(StaminaPolling(), null, Name);

            IEnumerator StaminaPolling()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    StaminaUpdate(Stamina.ZeroTicks);
                }
            }
        }

        /// <summary>
        /// 计算状态后的数值
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public int GetPropStateAddon(DiziProps prop)
        {
            var leveledValue = GetLeveledValue(prop);
            var value = PropState.GetStateAdjustmentValue(prop,
                Food.ValueMaxRatio,
                Emotion.ValueMaxRatio,
                Injury.ValueMaxRatio,
                Inner.ValueMaxRatio,
                leveledValue);
            return value;
        }
        /// <summary>
        /// 升等后的数值
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GetLeveledValue(DiziProps prop)
        {
            var propValue = prop switch
            {
                DiziProps.Strength => Capable.Strength.Value,
                DiziProps.Agility => Capable.Agility.Value,
                DiziProps.Hp => Capable.Hp.Value,
                DiziProps.Mp => Capable.Mp.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };
            var leveledValue = LevelCfg.GetLeveledValue(prop, propValue, Level);
            return leveledValue;
        }

        private int GetWeaponDamage() => Weapon?.Damage ?? 0;
        private int GetArmorDef()=> Armor?.Def ?? 0;

        #region ITerm

        IConditionValue ITerm.Stamina => Stamina.Con;
        #endregion

        internal void StaminaUpdate(long ticks)
        {
            var lastValue = _stamina.Con.Value;
            _stamina.Update(ticks);
            var updatedValue = _stamina.Con.Value;
            if (lastValue == updatedValue) return;
            Log($"体力更新 = {_stamina.Con}");
            SendEvent(EventString.Dizi_Params_StaminaUpdate, Guid);
        }

        internal void LevelSet(int level)
        {
            Level = level;
            Log($"等级设置 = {Level}");
        }

        internal void ExpSet(int exp, int max = 0)
        {
            if (max > 0) _exp.SetMax(max);
            _exp.Set(exp);
            Log($"经验设置 = {_exp}");
        }

        public class StateModel
        {
            public string ShortTitle { get; private set; } = "闲";
            public string Description { get; private set; }
            public long LastUpdate { get; private set; }

            public void Set(string shortTitle, string description, long lastUpdate)
            {
                ShortTitle = shortTitle;
                Description = description;
                LastUpdate = lastUpdate;
            }
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

        internal void SetWeapon(IWeapon weapon)
        {
            Log(weapon == null ? $"卸下{Weapon.Name}" : $"装备{weapon.Name}!");
            Weapon = weapon;
        }
        internal void SetArmor(IArmor armor)
        {
            Log(armor == null ? $"卸下{Armor.Name}" : $"装备{armor.Name}!");
            Armor = armor;
        }
        internal void ConAdd(IAdjustment.Types type, int value)
        {
            var con = type switch
            {
                IAdjustment.Types.Stamina => throw new NotImplementedException("体力状态不允许这里设!请用StaminaController"),
                IAdjustment.Types.Exp => throw new NotImplementedException("经验不允许!请用DiziController"),
                IAdjustment.Types.Silver => _silver,
                IAdjustment.Types.Food => _food,
                IAdjustment.Types.Emotion => _emotion,
                IAdjustment.Types.Injury => _injury,
                IAdjustment.Types.Inner => _inner,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            con.Add(value);
            Log($"状态[{type}]设置: {con}");
            SendEvent(EventString.Dizi_ConditionUpdate, Guid, type, value);
        }

        internal void SetAdvItem(int slot, IGameItem item)
        {
            _advItems[slot] = new AdvItemModel(item);
            Game.MessagingManager.Send(EventString.Dizi_Adv_SlotUpdate, Guid);
            Log("装备" + $"历练道具[{slot}]:" + item?.Name);
        }        
        internal IGameItem RemoveAdvItem(int slot)
        {
            var item = _advItems[slot];
            _advItems[slot] = null;
            Game.MessagingManager.Send(EventString.Dizi_Adv_SlotUpdate, Guid);
            Log("移除" + $"历练道具[{slot}]:" + item?.Item?.Name);
            return item.Item;
        }
    }

    /// <summary>
    /// 历练道具模型
    /// </summary>
    public class AdvItemModel : ModelBase
    {
        public enum Kinds
        {
            Medicine,
            StoryProp,
            Horse
        }
        protected override string LogPrefix => "历练道具";
        public IGameItem Item { get; private set; }
        public Kinds Kind { get; private set; }

        internal AdvItemModel(IGameItem item)
        {
            Kind = item.Type switch
            {
                ItemType.Medicine => Kinds.Medicine,
                ItemType.StoryProps => Kinds.StoryProp,
                ItemType.AdvProps => Kinds.Horse,
                _ => throw new ArgumentOutOfRangeException($"物品{item.Type}不支持! ")
            };
            Item = item;
        }
    }


    //弟子模型,处理历练事件
    public partial class Dizi
    {
        public IEnumerable<IStacking<IGameItem>> Items => Adventure?.GetItems() ?? Array.Empty<IStacking<IGameItem>>();
        public AutoAdventure Adventure { get; set; }
        internal void AdventureStart(IAutoAdvMap map, long startTime, int messageSecs)
        {
            Adventure = new AutoAdventure(map, startTime, messageSecs, this);
            Adventure.UpdateStoryService.AddListener(() => SetStateShort("历", "历练中...", startTime));
            Log("开始历练.");
            SendEvent(EventString.Dizi_Adv_Start, Guid);
        }

        // 设定弟子状态短文本
        private void SetStateShort(string title, string description, long time)
        {
            State.Set(title, description, time);
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid, title);
        }

        internal void AdventureStoryLogging(DiziAdvLog story)
        {
            if (Adventure.State == AutoAdventure.States.End)
                throw new NotImplementedException();
            Adventure.RegStory(story);
        }
        internal void AdventureRecall(long now, int lastMile)
        {
            SetStateShort("回", "回程中...", now);
            Adventure.UpdateStoryService.RemoveAllListeners();
            Adventure.Recall(now, lastMile, () => SetStateShort("待", "宗门外等待", 0));
            Log($"停止历练, 里数: {lastMile}");
            SendEvent(EventString.Dizi_Adv_Recall, Guid);
        }
        internal void AdventureFinalize()
        {
            Adventure = null;
            Log("历练结束!");
            SendEvent(EventString.Dizi_Adv_Finalize, Guid);
        }
    }

    //弟子模型, 处理闲置事件
    public partial class Dizi
    {
        public IdleState Idle { get; private set; }

        internal void StartIdle(long startTime)
        {
            Idle = new IdleState(this, startTime);
            SetStateShort("闲", "闲置中...", startTime);
            SendEvent(EventString.Dizi_Idle_Start, Guid);
        }

        internal void StopIdle()
        {
            Idle.StopIdleState();
            SendEvent(EventString.Dizi_Idle_Stop, Guid);
        }

        internal void RegIdleStory(DiziAdvLog log)
        {
            Idle.RegStory(log);
            
        }
    }
}