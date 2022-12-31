using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using Data;
using MyBox;
using Server.Configs._script.Skills;
using UnityEngine;

namespace Server.Configs._script.Battles
{
    interface IUnlockable
    {
        int UnlockLevel { get; }
    }
    [CreateAssetMenu(fileName = "combatSo", menuName = "战斗测试/武功")]
    internal class CombatFieldSo : ScriptableObject, IDataElement,ILeveling<ICombatSkill>
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }
        [ConditionalField(true, nameof(ReferenceSo))][ReadOnly][SerializeField] private ScriptableObject _so;
        #endregion
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private CombatForm[] 招式;
        [SerializeField] private ExertField[] 介入;

        public int Id => id;
        public string Name => _name;
        public Way.Armed Armed => 类型;
        private IList<CombatForm> Combats => 招式;
        private IList<ExertField> Exerts => 介入;

        private SkillGrades Grade => _grade;

        public ICombatSkill GetFromLevel(int level)
        {
            var unlockedCombats = Combats.Where(c => c.UnlockLevel <= level).ToArray();
            var unlockedExerts = Exerts.Where(e => e.UnlockLevel <= level).ToArray();
            return new CombatSkill(Name, Armed, unlockedCombats, unlockedExerts, Grade, level);
        }

        public ICombatSkill GetMaxLevel() =>
            new CombatSkill(Name, Armed, Combats.ToArray(), Exerts.ToArray(), Grade, MaxLevel());

        private int MaxLevel()
        {
            var comMax = Combats.Max(c => c.UnlockLevel);
            var exMax = Exerts.Max(e => e.UnlockLevel);
            return Math.Max(comMax, exMax);
        }

        public CombatBuffSoBase[] GetBuffSos(ICombatForm form) => Combats.Single(c => c == form).GetBuffSos().ToArray();

        [Serializable]
        private class CombatForm : ICombatForm, IUnlockable
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
            [SerializeField] private int 解锁等级;

            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => _buffs
                .Where(b => b.Append == append)
                .Select(b => b.InstanceBuff(unit)).ToArray();

            public IEnumerable<CombatBuffSoBase> GetBuffSos() => _buffs.ToArray();

            public int Breath => 息;
            public string Name => name;
            public int TarBusy => 对方硬直;
            public int Parry => 招架值;
            public int ParryMp => 招架内消;
            public int OffBusy => 己方硬直;

            public int CombatMp => 进攻内;
            public int DamageMp => 削内力;
            public ICombo Combo => 连击伤害表?.Length > 0 ? new ComboField(连击伤害表) : null;

            public int UnlockLevel => 解锁等级;

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
        [Serializable] private class ExertField : IExert,IUnlockable
        {
            public enum Activities
            {
                [InspectorName("进攻")]Attack,
                [InspectorName("恢复")]Recover
            }
            [SerializeField] private Activities 活动;
            [ConditionalField(nameof(活动), false, Activities.Attack)][SerializeField] private CombatForm 招式;
            [ConditionalField(nameof(活动), false, Activities.Recover)][SerializeField] private RecoveryField 恢复;
            [SerializeField] private int 解锁等级;

            public int UnlockLevel => 解锁等级;
            private ICombatForm Combat => 招式;
            private IRecovery Recovery => 恢复;
            public string Name => Activity switch
            {
                IPerform.Activities.Attack => Combat.Name,
                IPerform.Activities.Recover => Recovery.Name,
                IPerform.Activities.Auto => throw new ArgumentOutOfRangeException(),
                _ => throw new ArgumentOutOfRangeException()
            };

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

            private record Perform(IForceSkill ForceSkill, IDodgeSkill DodgeSkill, ICombatForm CombatForm, IRecovery Recover, IPerform.Activities Activity, bool IsReposition) : IPerform
            {
                public IForceSkill ForceSkill { get; } = ForceSkill;
                public IDodgeSkill DodgeSkill { get; } = DodgeSkill;
                public IPerform.Activities Activity { get; } = Activity;
                public ICombatForm CombatForm { get; } = CombatForm;
                public IRecovery Recover { get; } = Recover;
                public bool IsReposition { get; } = IsReposition;
            }
        }

        private record CombatSkill : ICombatSkill
        {
            public string Name { get; }
            public Way.Armed Armed { get; }
            public IList<ICombatForm> Combats { get; }
            public IList<IExert> Exerts { get; }
            public SkillGrades Grade { get; }
            public int Level { get; }

            public CombatSkill(string name, Way.Armed armed, CombatForm[] combats, ExertField[] exerts, SkillGrades grade, int level)
            {
                Name = name;
                Armed = armed;
                Combats = combats;
                Exerts = exerts;
                Grade = grade;
                Level = level;
            }

        }
    }
}