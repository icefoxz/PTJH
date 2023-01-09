using System;
using BattleM;
using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utls;

namespace Server.Configs.Adventures
{
    /// <summary>
    /// 条款基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class ClauseFieldBase<T> : IIClause<T>
    {
        public abstract bool IsInTerm(ITerm term);
    }
    [Serializable]
    internal class NoTermField
    {
        [SerializeField] private string _title;
        [SerializeField] private AdvEventSoBase _eventSo;
        public IAdvEvent Event => _eventSo;
        public string Title => _title;
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
        [SerializeField] private string _title;
        [SerializeField] private AdvEventSoBase 事件;
        [SerializeField] private StatusClauseField 状态条件;
        [ConditionalField(true, nameof(ResetDefault))][SerializeField] private Resetter 重置器;

        public StatusClauseField StatusTerm => 状态条件;
        public IAdvEvent Event => 事件;
        public string Title => _title;
        public bool InTerm(ITerm term) => StatusTerm.IsInTerm(term);

        public bool ResetDefault()
        {
            if (重置器 == Resetter.Reset)
            {
                重置器 = Resetter.Default;
                _title = string.Empty;
                状态条件.ResetDefault();
                事件 = null;
            }
            return true;
        }

    }
    [Serializable]
    internal class StatusClauseField : ClauseFieldBase<ICombatStatus>
    {
        [FormerlySerializedAs("_hp")][SerializeField] private ConValueSettingField Stamina;

        private ConValueSettingField StaminaClause => Stamina;


        public override bool IsInTerm(ITerm term)
        {
            var sta = term.Stamina;
            var isHpInTerm = StaminaClause.InTerm(sta.Value, sta.Max);
            return isHpInTerm;
        }

        public void ResetDefault() => Stamina.ResetDefault();

        [Serializable]
        private class ConValueSettingField
        {
            [SerializeField] private bool 范围外;
            [SerializeField] private bool 设百分比;

            [ConditionalField(nameof(设百分比))]
            [SerializeField]
            [MinMaxRange(0, 100)]
            private RangedInt 百分比;

            [ConditionalField(nameof(设百分比), true)]
            [SerializeField]
            private RangedInt 范围;

            private RangedInt PercentRange => 百分比;
            private RangedInt ValueRange => 范围;
            private bool Inverse => 范围外;
            private bool IsPercent => 设百分比;

            public bool InTerm(int value, int max)
            {
                var verifyValue = IsPercent ? (int)(100f * value / max) : value;
                var range = IsPercent ? PercentRange : ValueRange;
                var isInRange = range.InMinMaxRange(verifyValue);
                return Inverse ? !isInRange : isInRange;
            }

            public void ResetDefault()
            {
                范围外 = default;
                范围 = default;
                设百分比 = default;
                百分比 = default;
            }

            [Serializable]
            public class SettingResetter
            {
            }

            [CustomPropertyDrawer(typeof(SettingResetter))]
            private class ResetterDrawer : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    if (GUI.Button(position, "重置"))
                    {
                        var obj = property.propertyPath;
                        var type = obj.GetType();
                        var reset = type.GetMethod(nameof(ResetDefault));
                        reset?.Invoke(obj, null);
                    }
                }
            }
        }
    }

}