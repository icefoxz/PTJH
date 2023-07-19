using System;
using System.Collections;
using AOT.Core;
using AOT.Core.Dizi;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;
using GameClient.SoScripts.Characters;
using GameClient.System;
using UnityEngine;
using UnityEngine.Analytics;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子模型
    /// </summary>
    public partial class Dizi : ModelBase, ITerm
    {
        // 属性
        protected override string LogPrefix => Name;
        public string Guid { get; }
        public string Name { get; }
        public Gender Gender { get; }
        public int Level { get; private set; }
        public int Grade { get; }
        public IConditionValue Exp => _exp;
        public Capable Capable { get; private set; }
        public AdvItemModel[] AdvItems => _advItems;
        public IConditionValue Silver => _silver;
        public IConditionValue Food => _food;
        public IConditionValue Emotion => _emotion;
        public IConditionValue Injury => _injury;
        public IConditionValue Inner => _inner;

        public ICombatGifted Gifted { get; }

        // 计算属性
        public int Strength => StrengthProp.TotalValue;
        public int Agility => AgilityProp.TotalValue;
        public int Hp => HpProp.TotalValue;
        public int Mp => MpProp.TotalValue;
        public int Power => BattleSimulator.GetPower(strength: Strength, agility: Agility, hp: Hp, mp: Mp);
        public DiziPropValue StrengthProp { get; }
        public DiziPropValue AgilityProp { get; }
        public DiziPropValue HpProp { get; }
        public DiziPropValue MpProp { get; }
        public IDiziStamina Stamina => StaminaManager.Stamina;
        public DiziStaminaManager StaminaManager { get; }
        // 私有字段
        private ConValue _exp;
        private ConValue _food;
        private ConValue _emotion;
        private ConValue _silver;
        private ConValue _injury;
        private ConValue _inner;
        private AdvItemModel[] _advItems = new AdvItemModel[3];

        // 配置
        private LevelConfigSo LevelCfg => Game.Config.DiziCfg.LevelConfigSo;
        private PropStateConfigSo PropState => Game.Config.DiziCfg.PropState;
        private PropStateConfigSo.ConfigField PropStateCfg => PropState.Config;
        private BattleSimulatorConfigSo BattleSimulator => Game.Config.AdvCfg.BattleSimulation;

        // 构造函数
        public Dizi(string guid, string name,
            Gender gender,
            int level,
            int stamina,
            Capable capable, 
            ICombatGifted gifted,
            ICombatArmedAptitude armedAptitude)
        {
            // 初始化代码
            Guid = guid;
            Name = name;
            Gender = gender;
            Level = level;
            Grade = capable.Grade;
            Capable = capable;
            var maxExp = LevelCfg.GetMaxExp(level);
            _exp = new ConValue(maxExp, maxExp, 0);
            _food = new ConValue(PropStateCfg.FoodDefault.Max, PropStateCfg.FoodDefault.Min);
            _emotion = new ConValue(PropStateCfg.EmotionDefault.Max, PropStateCfg.EmotionDefault.Min);
            _silver = new ConValue(PropStateCfg.SilverDefault.Max, PropStateCfg.SilverDefault.Min);
            _injury = new ConValue(PropStateCfg.InjuryDefault.Max, PropStateCfg.InjuryDefault.Min);
            _inner = new ConValue(PropStateCfg.InnerDefault.Max, PropStateCfg.InnerDefault.Min);
            _equipment = new DiziEquipment(this);
            StrengthProp = new DiziPropValue(Capable.Strength.Value, () => GetLevelBonus(DiziProps.Strength),
                () => GetPropStateAddon(DiziProps.Strength), () => _equipment.GetPropAddon(DiziProps.Strength), null);
            AgilityProp = new DiziPropValue(Capable.Agility.Value, () => GetLevelBonus(DiziProps.Agility),
                () => GetPropStateAddon(DiziProps.Agility), () => _equipment.GetPropAddon(DiziProps.Agility), null);
            HpProp = new DiziPropValue(Capable.Hp.Value, () => GetLevelBonus(DiziProps.Hp),
                () => GetPropStateAddon(DiziProps.Hp), () => _equipment.GetPropAddon(DiziProps.Hp), null);
            MpProp = new DiziPropValue(Capable.Mp.Value, () => GetLevelBonus(DiziProps.Mp),
                () => GetPropStateAddon(DiziProps.Mp), () => _equipment.GetPropAddon(DiziProps.Mp), null);
            StaminaManager = new DiziStaminaManager(this, stamina);
            State = new DiziStateHandler(this, OnMessageAction, OnAdjustAction, OnRewardAction);
            Gifted = gifted;
            _armedAptitude = new DiziArmedAptitude(armedAptitude);
        }

        protected void EventUpdate(string eventString) => SendEvent(eventString, Guid);

        /// <summary>
        /// 计算状态后的数值
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public int GetPropStateAddon(DiziProps prop)
        {
            var leveledValue = GetLevelBonus(prop);
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
        public int GetLevelBonus(DiziProps prop)
        {
            var propValue = prop switch
            {
                DiziProps.Strength => Capable.Strength.Value,
                DiziProps.Agility => Capable.Agility.Value,
                DiziProps.Hp => Capable.Hp.Value,
                DiziProps.Mp => Capable.Mp.Value,
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };
            var leveledValue = propValue + LevelCfg.GetLeveledBonus(prop, propValue, Level);
            return leveledValue;
        }

        #region ITerm

        IConditionValue ITerm.Stamina => Stamina.Con;
        #endregion

        internal void StaminaUpdate(long ticks)
        {
            if (!StaminaManager.StaminaUpdate(ticks)) return;
            Log($"体力更新 = {StaminaManager.Stamina.Con}");
            EventUpdate(EventString.Dizi_Params_StaminaUpdate);
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
}