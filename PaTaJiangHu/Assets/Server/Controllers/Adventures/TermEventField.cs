using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using MyBox;
using UnityEditor;
using UnityEngine;
using Utls;

namespace Server.Controllers.Adventures
{
    /// <summary>
    /// 条款
    /// </summary>
    internal interface IIClause<in T>
    {
        bool IsTerm(ITerm term);
    }

    /// <summary>
    /// 条件
    /// </summary>
    internal interface ITerm
    {
        ICombatStatus Status { get; }
    }

    [CreateAssetMenu(fileName = "id_条件事件名", menuName = "副本/条件事件")]
    internal class TermEventField : AdvEventSoBase
    {
        [SerializeField] private NoTermField 无条件事件;
        [SerializeField] private TermField[] 条件;
        //[SerializeField] private int _id;
        //public override int Id => _id;
        /// <summary>
        /// NoTerm = 0 index
        /// </summary>
        public override IAdvEvent[] PossibleEvents
        {
            get
            {
                var list = new List<IAdvEvent>(TermFields.Select(t => t.Event));
                list.Insert(0, NoTermEvent.Event);
                return list.ToArray();
            }
        }

        public override AdvTypes AdvType => AdvTypes.Term;
        protected override Action<IAdvEvent> OnResultCallback { get; set; }
        private TermField[] TermFields => 条件;
        private NoTermField NoTermEvent => 无条件事件;
        public void NextEvent(IAdvEvent nextEvent) => OnResultCallback.Invoke(nextEvent);

        public (string title, IAdvEvent advEvent)[] GetInTermEventsWithTitle(ITerm term)
        {
            var inTermEvents = TermFields
                .Where(s => s.InTerm(term))
                .Select(c => (c.Title, c.Event)).ToList();
            inTermEvents.Add((NoTermEvent.Title, NoTermEvent.Event));
            return inTermEvents.ToArray();
        }

        public IAdvEvent[] GetInTermEvents(ITerm term)
        {
            var inTermEvents = TermFields
                .Where(s => s.InTerm(term))
                .Select(c => c.Event).ToList();
            inTermEvents.Add(NoTermEvent.Event);
            return inTermEvents.ToArray();
        }
        [Serializable] private class NoTermField
        {
            [SerializeField] private string _title;
            [SerializeField] private AdvEventSoBase _eventSo;

            public IAdvEvent Event => _eventSo;
            public string Title => _title;
        }
        [Serializable] private class TermField
        {
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
            public bool InTerm(ITerm term) => StatusTerm.IsTerm(term);

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
        [Serializable] private class StatusClauseField : ClauseFieldBase<ICombatStatus>
        {
            [SerializeField] private ConValueSettingField _hp;
            [SerializeField] private ConValueSettingField _tp;
            [SerializeField] private ConValueSettingField _mp;

            private ConValueSettingField HpClause => _hp;
            private ConValueSettingField TpClause => _tp;
            private ConValueSettingField MpClause => _mp;

            public override bool IsTerm(ITerm term)
            {
                var hp = term.Status.Hp;
                var mp = term.Status.Mp;
                var tp = term.Status.Tp;
                var isHpInTerm = HpClause.InTerm(hp.Value, hp.Max);
                var isTpInTerm = TpClause.InTerm(tp.Value, tp.Max);
                var isMpInTerm = MpClause.InTerm(mp.Value, mp.Max);
                return isHpInTerm && isTpInTerm && isMpInTerm;
            }

            public void ResetDefault()
            {
                _hp.ResetDefault();
                _tp.ResetDefault();
                _mp.ResetDefault();
            }

            [Serializable]
            private class ConValueSettingField
            {
                [SerializeField] private bool 范围外;
                [SerializeField] private bool 设百分比;

                [ConditionalField(nameof(设百分比))] [SerializeField] [MinMaxRange(0, 100)]
                private RangedInt 百分比;

                [ConditionalField(nameof(设百分比), true)] [SerializeField]
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

        //条款基类
        private abstract class ClauseFieldBase<T> : IIClause<T>
        {
            public abstract bool IsTerm(ITerm term);
        }
    }

}