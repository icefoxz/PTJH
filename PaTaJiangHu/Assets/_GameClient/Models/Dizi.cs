using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using DiziM;
using Server.Configs.Adventures;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
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
        private BattleSimulatorConfigSo BattleSimulator => Game.Config.AdvCfg.BattleSimulation;

        public int Power => BattleSimulator.GetPower(strength: Strength, agility: Agility, hp: Hp, mp: Mp,
            weaponDamage: WeaponPower, armorAddHp: ArmorPower);
        public int Grade { get; }
        //public ICombatSkill CombatSkill { get; private set; }
        //public IForceSkill ForceSkill { get; private set; }
        //public IDodgeSkill DodgeSkill { get; private set; }
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }

        /// <summary>
        /// 武器战力
        /// </summary>
        public int WeaponPower => GetWeaponDamage();
        /// <summary>
        /// 防具战力
        /// </summary>
        public int ArmorPower => GetArmorAddHp();

        private ConValue _exp;

        public IDiziStamina Stamina => _stamina;
        private DiziStamina _stamina;
        private ConValue _food;
        private ConValue _emotion;
        private ConValue _silver;
        private ConValue _injury;
        private ConValue _inner;
        private AdvItemModel[] _advItems = new AdvItemModel[3];

        //public Skills Skill { get; set; }
        public Capable Capable { get; private set; }

        public AdvItemModel[] AdvItems => _advItems;

        public IConditionValue Silver => _silver;
        public IConditionValue Food => _food;
        public IConditionValue Emotion => _emotion;
        public IConditionValue Injury => _injury;
        public IConditionValue Inner => _inner;

        /// <summary>
        /// 状态信息, 提供当前状态的描述,与时长
        /// </summary>
        public DiziStateHandler State { get; }

        private LevelConfigSo LevelCfg => Game.Config.DiziCfg.LevelConfigSo;
        private PropStateConfigSo PropState => Game.Config.DiziCfg.PropState;
        private PropStateConfigSo.ConfigField PropStateCfg => PropState.Config;

        internal Dizi(string guid, string name,
            Gender gender,
            int level, 
            int stamina, 
            Capable capable)
        {
            Guid = guid;
            Name = name;
            Gender = gender;
            Level = level;
            Grade = capable.Grade;
            Capable = capable;
            _stamina = new DiziStamina(Game.Controllers.Get<StaminaController>(), stamina);
            var maxExp = LevelCfg.GetMaxExp(level);
            _exp = new ConValue(maxExp, maxExp, 0);
            _food = new ConValue(PropStateCfg.FoodDefault.Max, PropStateCfg.FoodDefault.Min);
            _emotion = new ConValue(PropStateCfg.EmotionDefault.Max, PropStateCfg.EmotionDefault.Min);
            _silver = new ConValue(PropStateCfg.SilverDefault.Max, PropStateCfg.SilverDefault.Min);
            _injury = new ConValue(PropStateCfg.InjuryDefault.Max, PropStateCfg.InjuryDefault.Min);
            _inner = new ConValue(PropStateCfg.InnerDefault.Max, PropStateCfg.InnerDefault.Min);
            State = new DiziStateHandler(this, OnMessageAction, OnAdjustAction, OnRewardAction);
            StaminaService();
        }

        #region StateHandler
        private void OnRewardAction() => SendEvent(EventString.Dizi_Activity_Reward, Guid);
        private void OnAdjustAction(string adjust) => SendEvent(EventString.Dizi_Activity_Adjust, Guid, adjust);
        private void OnMessageAction(string message) => SendEvent(EventString.Dizi_Activity_Message, Guid, message);
        #endregion

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
        private int GetArmorAddHp()=> Armor?.AddHp ?? 0;

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

        //弟子技能栏
        //public class Skills
        //{
        //    public IDodgeSkill[] DodgeSkills { get; private set; }
        //    public ICombatSkill[] CombatSkills { get; private set; }
        //    public IForceSkill[] ForceSkills { get; private set; }

        //    public Skills()
        //    {
                
        //    }
        //    public Skills(ICombatSkill[] combatSlot, IForceSkill[] forceSkill, IDodgeSkill[] dodgeSlot)
        //    {
        //        DodgeSkills = dodgeSlot;
        //        CombatSkills = combatSlot;
        //        ForceSkills = forceSkill;
        //    }

        //    public Skills(int combatSlot, int forceSkill, int dodgeSlot)
        //    {
        //        DodgeSkills = new IDodgeSkill[dodgeSlot];
        //        CombatSkills = new ICombatSkill[combatSlot];
        //        ForceSkills = new IForceSkill[forceSkill];
        //    }
        //}

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

        public override string ToString() => Name;
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
        public AutoAdventure Adventure => State.Adventure;
        internal void AdventureStart(IAutoAdvMap map, long startTime, int messageSecs,bool isProduction)
        {
            State.StartAdventure(map, startTime, messageSecs, isProduction);
            Log("开始历练.");
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Start, Guid);
        }

        internal void AdventureStoryLogging(DiziActivityLog story)
        {
            if (Adventure.State == AutoAdventure.States.End)
                throw new NotImplementedException();
            Adventure.RegStory(story);
        }
        internal void AdventureRecall(long now, int lastMile, long reachingTime)
        {
            State.RecallFromAdventure(now, lastMile, reachingTime);
            Log($"停止历练, 里数: {lastMile}, 将{TimeSpan.FromMilliseconds(reachingTime - now).TotalSeconds}秒后到达宗门!");
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Recall, Guid);
        }
        internal void AdventureFinalize()
        {
            State.FinalizeAdventure();
            Log("历练结束!");
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Adv_Finalize, Guid);
        }

        public void AdventureTerminate(long terminateTime,int lastMile)
        {
            Log("历练中断!");
            State.Terminate(terminateTime, lastMile);
            SendEvent(EventString.Dizi_Adv_Terminate, Guid);
        }
    }

    //弟子模型, 处理闲置事件
    public partial class Dizi
    {
        //闲置状态
        //public IdleState Idle => State.Idle;

        internal void StartIdle(long startTime)
        {
            State.StartIdle(startTime);
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Idle_Start, Guid);
        }

        internal void StopIdle()
        {
            State.StopIdleState();
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Idle_Stop, Guid);
        }

        internal void RegIdleStory(DiziActivityLog log)
        {
            State.RegIdleStory(log);
        }
    }

    //处理失踪事件
    public partial class Dizi
    {
        //public LostState LostState => State.LostState;

        internal void StartLostState(long startTime, DiziActivityLog lastActivityLog)
        {
            State.StartLost(this, startTime, lastActivityLog);
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Lost_Start, Guid);
        }

        internal void RestoreFromLost()
        {
            State.RestoreFromLost();
            SendEvent(EventString.Dizi_Params_StateUpdate, Guid);
            SendEvent(EventString.Dizi_Lost_End, Guid);
        }
    }
}