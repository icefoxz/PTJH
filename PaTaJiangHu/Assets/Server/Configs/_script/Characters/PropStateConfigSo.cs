using System;
using System.Linq;
using MyBox;
using UnityEngine;
using Utls;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "propStateConfig", menuName = "配置/弟子/属性状态配置")]
    internal class PropStateConfigSo : ScriptableObject
    {
        [SerializeField] private PropCfg 力量;
        [SerializeField] private PropCfg 敏捷;
        [SerializeField] private PropCfg 血量;
        [SerializeField] private PropCfg 内力;

        private PropCfg StrengthCfgs => 力量;
        private PropCfg AgilityCfgs => 敏捷;
        private PropCfg HpCfgs => 血量;
        private PropCfg MpCfgs => 内力;

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
            var cfg = propCfg.Cfgs.First(c => c.IsInRange(rate));
            return cfg.GetAdjustmentValue(max);
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
            public bool IsInRange(int rate) => Range.InMinMaxRange(rate);
            public int GetAdjustmentValue(int value) => (int)(value * Percentage * 0.01d);
        }
        private static int ToIntPercentage(double ratio) => (int)(ratio * 100);
    }
}