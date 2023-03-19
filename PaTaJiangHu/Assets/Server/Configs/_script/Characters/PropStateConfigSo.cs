using System;
using System.Linq;
using MyBox;
using Server.Configs.SoUtls;
using UnityEngine;
using Utls;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "propStateConfig", menuName = "配置/弟子/属性状态配置")]
    internal class PropStateConfigSo : ScriptableObject
    {
        [SerializeField] private ConfigField 状态属性设定;
        [SerializeField] private PropCfg 力量;
        [SerializeField] private PropCfg 敏捷;
        [SerializeField] private PropCfg 血量;
        [SerializeField] private PropCfg 内力;
        [SerializeField] private Color 增益颜色;
        [SerializeField] private Color 减益颜色;

        public ConfigField Config => 状态属性设定;
        private PropCfg StrengthCfgs => 力量;
        private PropCfg AgilityCfgs => 敏捷;
        private PropCfg HpCfgs => 血量;
        private PropCfg MpCfgs => 内力;
        private Color BuffColor => 增益颜色;
        private Color DebuffColor => 减益颜色;
        public Color GetBuffColor(bool isDebuff = false) => isDebuff ? DebuffColor : BuffColor;
        public (string title, Color color) GetFoodCfg(double ratio) => GetPropMapping((int)(ratio * 100), Config.FoodMap);
        public (string title, Color color) GetSilverCfg(double ratio) => GetPropMapping((int)(ratio * 100), Config.SilverMap);
        public (string title, Color color) GetEmotionCfg(double ratio) => GetPropMapping((int)(ratio * 100), Config.EmotionMap);
        public (string title, Color color) GetInjuryCfg(double ratio) => GetPropMapping((int)(ratio * 100), Config.InjuryMap);
        public (string title, Color color) GetInnerCfg(double ratio) => GetPropMapping((int)(ratio * 100), Config.InnerMap);

        private (string title,Color color) GetPropMapping(int rate,ValueMapping<string>[] titleMap)
        {
            var map = titleMap.FirstOrDefault(m => m.IsInCondition(rate));
            var color = Config.StateColor.FirstOrDefault(c => c.IsInCondition(rate));
            return (map?.Value, color?.Value ?? Color.white);
        }

        [Serializable]
        internal class ConfigField
        {
            [SerializeField] private MinMaxInt 银两初始设定;
            [SerializeField] private MinMaxInt 食物初始设定;
            [SerializeField] private MinMaxInt 精神初始设定;
            [SerializeField] private MinMaxInt 外伤初始设定;
            [SerializeField] private MinMaxInt 内伤初始设定;
            [SerializeField] private ValueMapping<Color>[] 状态颜色;
            [SerializeField] private ValueMapping<string>[] 银两描述;
            [SerializeField] private ValueMapping<string>[] 食物描述;
            [SerializeField] private ValueMapping<string>[] 精神描述;
            [SerializeField] private ValueMapping<string>[] 外伤描述;
            [SerializeField] private ValueMapping<string>[] 内伤描述;
            public MinMaxInt SilverDefault => 银两初始设定;
            public MinMaxInt FoodDefault => 食物初始设定;
            public MinMaxInt EmotionDefault => 精神初始设定;
            public MinMaxInt InjuryDefault => 外伤初始设定;
            public MinMaxInt InnerDefault => 内伤初始设定;
            public ValueMapping<Color>[] StateColor => 状态颜色;
            public ValueMapping<string>[] SilverMap => 银两描述;
            public ValueMapping<string>[] FoodMap => 食物描述;
            public ValueMapping<string>[] EmotionMap => 精神描述;
            public ValueMapping<string>[] InjuryMap => 外伤描述;
            public ValueMapping<string>[] InnerMap => 内伤描述;
        }

        public int GetStateAdjustmentValue(DiziProps prop, double foodRatio, double emoRatio, double injuryRatio,
            double innerRatio, int value) =>
            prop switch
            {
                DiziProps.Strength => RangeCount(StrengthCfgs, foodRatio, emoRatio, injuryRatio, innerRatio, value),
                DiziProps.Agility => RangeCount(AgilityCfgs, foodRatio, emoRatio, injuryRatio, innerRatio, value),
                DiziProps.Hp => RangeCount(HpCfgs, foodRatio, emoRatio, injuryRatio, innerRatio, value),
                DiziProps.Mp => RangeCount(MpCfgs, foodRatio, emoRatio, injuryRatio, innerRatio, value),
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };

        private int RangeCount(PropCfg propCfg, double foodRatio, double emoRatio, double injuryRatio, double innerRatio, int max)
        {
            var rate = propCfg.GetCfgRatio(foodRatio, emoRatio, injuryRatio, innerRatio);
            var cfg = propCfg.Cfgs.FirstOrDefault(c => c.IsInRange(rate));
            return cfg?.GetAdjustmentValue(max) ?? max;
        }
        private enum Conditions
        {
            [InspectorName("食物")]Food,
            [InspectorName("精神")]Emo,
            [InspectorName("外伤")]Injury,
            [InspectorName("内伤")]Inner
        }
        [Serializable]private class PropCfg
        {
            [SerializeField] private Conditions 关联状态;
            [SerializeField] private RangeCfg[] 范围配置;

            private Conditions Condition => 关联状态;
            public RangeCfg[] Cfgs => 范围配置;

            public int GetCfgRatio(double foodRatio, double emoRatio, double injuryRatio, double innerRatio)
            {
                return Condition switch
                {
                    Conditions.Food => ToIntPercentage(foodRatio),
                    Conditions.Emo => ToIntPercentage(emoRatio),
                    Conditions.Injury => ToIntPercentage(injuryRatio),
                    Conditions.Inner => ToIntPercentage(innerRatio),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }

        }
        [Serializable]private class RangeCfg
        {
            private bool Rename()
            {
                var percent = Percentage;
                var addOnText = percent < 0 ? "扣除" : "增加";
                _name = $"{Range.Min}~{Range.Max}: {addOnText}{percent}%";
                return true;
            }

            [ConditionalField(true,nameof(Rename))][SerializeField][ReadOnly] private string _name;
            [SerializeField] private MinMaxInt 范围;
            [SerializeField] private int 百分比;

            private MinMaxInt Range => 范围;
            private int Percentage => 百分比;
            public bool IsInRange(int rate) => MinMaxExtension.IsInRange(Range, rate);
            public int GetAdjustmentValue(int value) => (int)(value * Percentage * 0.01d);
        }
        private static int ToIntPercentage(double ratio) => (int)(ratio * 100);
    }
}