using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using Data;
using MyBox;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "martialSo", menuName = "战斗测试/武功")]
    [Serializable]
    public class MartialFieldSo : ScriptableObject, IMartial, IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private CombatForm[] 招式;
        [SerializeField] private ExertField[] 介入;

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        public IList<ICombatForm> Combats => 招式;
        public IList<IExert> Exerts => 介入;

        [Serializable] private class CombatForm : ICombatForm
        {
            [SerializeField] private string name;
            [SerializeField] private int 息;
            [SerializeField] private int 进攻内;
            [SerializeField] private int 削内力;
            [SerializeField] private int 招架值;
            [SerializeField] private int 招架内消;
            [SerializeField] private int 对方硬直;
            [SerializeField] private int 己方硬直;
            [SerializeField] private int[] 连击伤害表;
            [SerializeField] private CombatBuffSoBase[] _buffs;

            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => _buffs.Where(b => b.Append == append)
                .Select(b => b.InstanceBuff(unit)).ToArray();
            public int Breath => 息;
            public string Name => name;
            public int TarBusy => 对方硬直;
            public int Parry => 招架值;
            public int ParryMp => 招架内消;
            public int OffBusy => 己方硬直;

            public int CombatMp => 进攻内;
            public int DamageMp => 削内力;
            public ICombo Combo => 连击伤害表?.Length > 0 ? new ComboField(连击伤害表) : null;

            private class ComboField : ICombo
            {
                public int[] Rates { get; }

                public ComboField(int[] rates)
                {
                    Rates = rates;
                }
            }
        }
        [Serializable] private class RecoveryField : IRecovery
        {
            [SerializeField] private string _name;
            [SerializeField] private int 恢复值;
            [SerializeField] private CombatBuffSoBase[] _buffs;
            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => _buffs.Where(b => b.Append == append)
                .Select(b => b.InstanceBuff(unit)).ToArray();
            public string Name => _name;
            public int Recover => 恢复值;
        }
        [Serializable] private class ExertField : IExert
        {
            public enum Activities
            {
                [InspectorName("进攻")]Attack,
                [InspectorName("恢复")]Recover
            }
            [SerializeField] private Activities 活动;
            [ConditionalField(nameof(活动), false, Activities.Attack)][SerializeField] private CombatForm 招式;
            [ConditionalField(nameof(活动), false, Activities.Recover)][SerializeField] private RecoveryField 恢复;
            private ICombatForm Combat => 招式;
            private IRecovery Recovery => 恢复;

            public IPerform.Activities Activity => 活动 switch
            {
                Activities.Attack => IPerform.Activities.Attack,
                Activities.Recover => IPerform.Activities.Recover,
                _ => throw new ArgumentOutOfRangeException()
            };

            public IPerform GetPerform(IBreathBar bar)
            {
                IPerform perform = Activity switch
                {
                    IPerform.Activities.Attack => new Perform(bar.Perform.ForceSkill, bar.Perform.DodgeSkill, Combat,
                        bar.Perform.Recover, IPerform.Activities.Attack, bar.IsReposition),
                    IPerform.Activities.Recover => new Perform(bar.Perform.ForceSkill, bar.Perform.DodgeSkill,
                        bar.Perform.CombatForm,
                        Recovery, IPerform.Activities.Attack, bar.IsReposition),
                    IPerform.Activities.Auto => throw new ArgumentOutOfRangeException(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                return perform;
            }

            private record Perform(IForce ForceSkill, IDodge DodgeSkill, ICombatForm CombatForm, IRecovery Recover, IPerform.Activities Activity, bool IsReposition) : IPerform
            {
                public IForce ForceSkill { get; } = ForceSkill;
                public IDodge DodgeSkill { get; } = DodgeSkill;
                public IPerform.Activities Activity { get; } = Activity;
                public ICombatForm CombatForm { get; } = CombatForm;
                public IRecovery Recover { get; } = Recover;
                public bool IsReposition { get; } = IsReposition;
            }
        }
    }
}