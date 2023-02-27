using System;
using System.Linq;
using BattleM;
using MyBox;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utls;

namespace Server.Configs.Adventures
{
    /// <summary>
    /// 条款基类
    /// </summary>
    internal abstract class ClauseFieldBase : IIClause
    {
        public abstract bool IsInTerm(ITerm term);
    }
    [Serializable]
    internal class NoTermField
    {
        [SerializeField] private bool 描述事件;
        [ConditionalField(nameof(描述事件))][SerializeField] private string _title;
        [SerializeField] private AdvEventSoBase _eventSo;
        private bool Rename => 描述事件;
        public IAdvEvent Event => _eventSo;
        public string Title => Rename ? _title : string.Empty;
    }
    [Serializable]
    internal class TermField
    {
        public enum Modes
        {
            [InspectorName("第一符合条件")] First,
            [InspectorName("随机符合条件")] Random
        }
        private enum Resetter
        {
            [InspectorName("切换会重置数据，支持ctrl+z恢复")] Default,
            [InspectorName("重置")] Reset,
        }
        [ConditionalField(nameof(描述事件))][SerializeField] private string _title;
        [SerializeField] private bool 描述事件;
        [SerializeField] private AdvEventSoBase 事件;
        [SerializeField] private StatusClauseField 状态条件;
        //[ConditionalField(true, nameof(ResetDefault))][SerializeField] private Resetter 重置器;
        private bool Rename => 描述事件;
        public StatusClauseField StatusTerm => 状态条件;
        public IAdvEvent Event => 事件;
        public string Title => Rename ? _title : string.Empty;
        public bool InTerm(ITerm term) => StatusTerm.IsInTerm(term);

        //public bool ResetDefault()
        //{
        //    if (重置器 == Resetter.Reset)
        //    {
        //        重置器 = Resetter.Default;
        //        _title = string.Empty;
        //        //状态条件.ResetDefault();
        //        事件 = null;
        //    }
        //    return true;
        //}

    }
    [Serializable] internal class StatusClauseField : ClauseFieldBase
    {
        private enum Modes
        {
            [InspectorName("所有条件")] And,
            [InspectorName("任何条件")] Or
        }
        private const string StaminaText = "体力";
        private const string SilverText = "俸禄";
        private const string FoodText = "食物";
        private const string EmotionText = "精神";
        private const string InjuryText = "外伤";
        private const string InnerText = "内伤";

        private static string GetText(Conditions con) => con switch
        {
            Conditions.Stamina => StaminaText,
            Conditions.Silver => SilverText,
            Conditions.Food => FoodText,
            Conditions.Emotion => EmotionText,
            Conditions.Injury => InjuryText,
            Conditions.Inner => InnerText,
            _ => throw new ArgumentOutOfRangeException(nameof(con), con, null)
        };
        private enum Conditions
        {
            [InspectorName(StaminaText)]Stamina,
            [InspectorName(SilverText)]Silver,
            [InspectorName(FoodText)]Food,
            [InspectorName(EmotionText)]Emotion,
            [InspectorName(InjuryText)]Injury,
            [InspectorName(InnerText)]Inner
        }
        
        [SerializeField] private Modes 判断模式;
        [SerializeField] private ConValueSettingField[] 状态;

        private Modes Mode => 判断模式;
        private ConValueSettingField[] Cons => 状态;

        public override bool IsInTerm(ITerm term)
        {
            var stas = Cons.Where(c => c.Con == Conditions.Stamina).ToArray();
            var sils = Cons.Where(c => c.Con == Conditions.Silver).ToArray();
            var foos = Cons.Where(c => c.Con == Conditions.Food).ToArray();
            var emos = Cons.Where(c => c.Con == Conditions.Emotion).ToArray();
            var injs = Cons.Where(c => c.Con == Conditions.Injury).ToArray();
            var inns = Cons.Where(c => c.Con == Conditions.Inner).ToArray();
            var isStaInTerm = InConInTerm(term.Stamina, stas);
            var isSilverInTerm = InConInTerm(term.Silver, sils);
            var isFoodInTerm = InConInTerm(term.Food, foos);
            var isEmotionInTerm = InConInTerm(term.Emotion, emos);
            var isInjuryInTerm = InConInTerm(term.Injury,injs);
            var isInnerInTerm = InConInTerm(term.Inner, inns);
            return Mode switch
            {
                Modes.And => isStaInTerm && 
                             isSilverInTerm && 
                             isFoodInTerm && 
                             isEmotionInTerm && 
                             isInjuryInTerm &&
                             isInnerInTerm,
                Modes.Or => isStaInTerm || 
                            isSilverInTerm || 
                            isFoodInTerm || 
                            isEmotionInTerm || 
                            isInjuryInTerm ||
                            isInnerInTerm,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private bool InConInTerm(IConditionValue con, ConValueSettingField[] clauses) =>
            clauses.All(c => c.InTerm(con.Value, con.Max));

        //public void ResetDefault() => 状态 = Array.Empty<ConValueSettingField>();

        [Serializable] private class ConValueSettingField
        {
            private bool RenameElement()
            {
                switch (Con)
                {
                    case Conditions.Stamina:
                    case Conditions.Silver:
                    case Conditions.Food:
                    case Conditions.Emotion:
                    case Conditions.Injury:
                    case Conditions.Inner:
                        _name = $"{GetText(Con)}:{TermText()}";
                        break;
                    default:
                        _name = string.Empty;
                        return true;
                }

                return true;
            }

            [ConditionalField(true, nameof(RenameElement))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private Conditions 状态;
            [SerializeField] private bool 范围外;
            [SerializeField] private bool 设百分比;

            [ConditionalField(nameof(设百分比))]
            [SerializeField]
            [MinMaxRange(0, 100)]
            private RangedInt 百分比;

            [ConditionalField(nameof(设百分比), true)]
            [SerializeField]
            private RangedInt 范围;

            public Conditions Con => 状态;

            private RangedInt PercentRange => 百分比;
            private RangedInt ValueRange => 范围;
            private bool Inverse => 范围外;
            private bool IsPercent => 设百分比;

            public bool InTerm(int value, int max)
            {
                var verifyValue = IsPercent ? value.PercentInt(max) : value;
                var range = GetRange();
                var isInRange = range.InMinMaxRange(verifyValue);
                return Inverse ? !isInRange : isInRange;
            }

            private RangedInt GetRange()
            {
                var range = IsPercent ? PercentRange : ValueRange;
                return range;
            }

            public string TermText()
            {
                var percentText = IsPercent ? "%" : string.Empty;
                var inverseText = Inverse ? "!" : string.Empty;
                var range = GetRange();
                var rangeText = range.Min + "~" + range.Max;
                return inverseText + rangeText + percentText;
            }

            public void ResetDefault()
            {
                范围外 = default;
                范围 = default;
                设百分比 = default;
                百分比 = default;
            }

            //[Serializable]
            //public class SettingResetter
            //{
            //}

            //[CustomPropertyDrawer(typeof(SettingResetter))]
            //private class ResetterDrawer : PropertyDrawer
            //{
            //    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            //    {
            //        if (GUI.Button(position, "重置"))
            //        {
            //            var obj = property.propertyPath;
            //            var type = obj.GetType();
            //            var reset = type.GetMethod(nameof(ResetDefault));
            //            reset?.Invoke(obj, null);
            //        }
            //    }
            //}
        }
    }

    [Serializable] internal class PropertyClauseField
    {

    }
}