using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using static Unity.VisualScripting.Member;

namespace BattleM
{
    /// <summary>
    /// 战斗目标,无论任何单位都是战斗目标，但主单位有更多信息<see cref="ICombatUnit"/>
    /// </summary>
    public interface ICombatInfo
    {
        int CombatId { get; }
        IEquipment Equipment { get; }
        int Position { get; }
        int StandingPoint { get; }
        string Name { get; }
        bool IsDeath { get; }
        int Distance(ICombatInfo target);
    }
    /// <summary>
    /// 战斗单位
    /// </summary>
    public interface ICombatUnit : ICombatInfo
    {
        int Strength { get; }
        int Agility { get; }
        ICombatStatus Status { get; }
        IBreathBar BreathBar { get; }
        IForce ForceSkill { get; }
        IMartial CombatSkill { get; }
        IDodge DodgeSkill { get; }
        void SetStandingPoint(int standingPoint);
        void SetStrategy(CombatUnit.Strategies strategy);
        void SetCombatId(int combatId);
    }

    public class CombatUnit : ICombatUnit, IComparable<CombatUnit>
    {
        public enum Strategies
        {
            /// <summary>
            /// 稳扎稳打
            /// </summary>
            Steady,
            /// <summary>
            /// 以伤换伤
            /// </summary>
            Hazard,
            /// <summary>
            /// 死守保全
            /// </summary>
            Defend,
            /// <summary>
            /// 逃跑求生
            /// </summary>
            RunAway,
            /// <summary>
            /// 死战到底
            /// </summary>
            DeathFight
        }
        private static Random Random { get; } = new(DateTime.Now.Millisecond);

        private BreathBar _breathBar;
        private CombatRound Round { get; set; }
        private static readonly BasicDodge DefaultDodge = new();
        private static readonly BasicCombat DefaultCombat = new();
        private static readonly BasicParry DefaultParry = new();
        private static readonly BasicForce DefaultForce= new();

        public static CombatUnit Instance(string name, int strength, int agility, ICombatStatus status, IForce forceSkill, IMartial combatSkill,
            IDodge dodgeSkill, IEquip equip) => new(name, strength, agility, status, forceSkill, combatSkill, dodgeSkill, equip);
        public static CombatUnit Instance(ICombatUnit o) => new(o.Name, o.Strength, o.Agility, o.Status.Clone(),
            o.ForceSkill, o.CombatSkill, o.DodgeSkill, o.Equipment);
        public int StandingPoint { get; private set; }
        public string Name { get; }
        public bool IsDeath { get; private set; }
        public int CombatId { get; private set; }
        public int Strength { get; }
        public int Agility { get; }
        public Strategies Strategy { get; private set; }
        public ICombatStatus Status { get; }
        public IBreathBar BreathBar => _breathBar;
        public IForce ForceSkill { get; }
        public IMartial CombatSkill { get; }
        public IDodge DodgeSkill { get; }
        public void SetStandingPoint(int standingPoint) => StandingPoint = standingPoint;
        public void SetStrategy(Strategies strategy) => Strategy = strategy;
        public void SetCombatId(int combatId) => CombatId = combatId;

        public IEquipment Equipment { get; }
        public bool IsEscapeCondition
        {
            get
            {
                switch (Strategy)
                {
                    case Strategies.Steady when LowerThan(0.3f, Status):
                    case Strategies.Hazard when LowerThan(0.1f, Status):
                    case Strategies.Defend when LowerThan(0.4f, Status):
                    case Strategies.RunAway when LowerThan(0.5f, Status):
                        return true;
                    case Strategies.DeathFight:
                    default: return false;
                }
                bool LowerThan(float value, ICombatStatus status)
                {
                    return _breathBar.Round.Current >= Round.MinEscapeRounds &&
                           (value > status.Hp.ValueFixRatio ||
                            value > status.Tp.ValueFixRatio);
                }
            }
        }

        #region RecordEvents
        public event Action<ConsumeRecord<IForceForm>> OnRecoverConsume;
        public event Action<ConsumeRecord<IDodgeForm>> OnDodgeConsume;
        public event Action<ConsumeRecord<ICombatForm>> OnCombatConsume;
        public event Action<ConsumeRecord> OnConsume;
        public event Action<EventRecord> OnFightEvent;
        #endregion

        private CombatManager Mgr { get; set; }
        #region ICombatTarget

        public ICombatInfo Target { get; private set; }
        public int Position { get; private set; }
        public bool IsBusy => BreathBar.Busies.Any();
        public int WeaponDamage => Equipment.Weapon?.Damage ?? 0;
        public Weapon.Injuries WeaponInjuryType => Equipment.Weapon?.Injury ?? Weapon.Injuries.Blunt;
        public int Armor => Equipment.Armor?.Def ?? 0;
        public bool IsExhausted => IsDeath || Status.IsExhausted;

        public IDodgeForm PickDodge() => DodgeSkill?.Forms.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultDodge;
        public ICombatForm PickCombat() => CombatSkill?.Combats.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultCombat;
        public IParryForm PickParry() => CombatSkill?.Parries.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultParry;

        private CombatUnit(string name, int strength, int agility, ICombatStatus status, IForce forceSkill, IMartial combatSkill, IDodge dodgeSkill,
            IEquip equip)
        {
            Name = name;
            Strength = strength;
            Agility = agility;
            Status = status;
            ForceSkill = forceSkill ?? DefaultForce;
            CombatSkill = combatSkill;
            DodgeSkill = dodgeSkill;
            Equipment = new Equipment(equip);
        }

        public void SetPosition(int position) => Position = position;

        public int Distance(ICombatInfo target) => Math.Abs(Position - target.Position);
        #endregion

        /// <summary>
        /// 初始化战斗
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="round"></param>
        /// <param name="target"></param>
        /// <param name="standingPoint"></param>
        /// <param name="strategy"></param>
        public void Init(CombatManager mgr,CombatRound round, ICombatInfo target,int standingPoint,Strategies strategy)
        {
            Mgr = mgr;
            Round = round;
            _breathBar = new BreathBar(round);
            StandingPoint = standingPoint;
            Strategy = strategy;
            ChangeTarget(target);
        }

        /// <summary>
        /// 换目标
        /// </summary>
        /// <param name="target"></param>
        public void ChangeTarget(ICombatInfo target) => Target = target;
        public void BreathCharge(int charge)
        {
            WaitingRecovery();
            if (charge <= 0) return;
            _breathBar.Charge(charge);
        }

        #region CombatPlan
        /// <summary>
        /// 准备下一招
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CombatPlan()
        {
            if (IsEscapeCondition)
            {
                if (Mgr.GetAliveCombatUnit(Target).IsEscapeCondition) //如果对方打算逃跑，可以等待或恢复
                {
                    TryRecover();
                    return;
                }
                _breathBar.SetPlan(BattleM.BreathBar.Plans.Surrender);
                return;
            }
            var combatForm = GetCombatForm();//获取攻击招式
            var dodgeForm = GetDodgeForm();//获取身法
            var isReachable = IsCombatRange() || dodgeForm != null;
            var isRecoverNeed = CheckRecover();
            if (isRecoverNeed)
            {
                TryRecover();
                return;
            }

            if (combatForm != null && isReachable && !IsEscapeCondition)
            {
                _breathBar.SetPlan(BattleM.BreathBar.Plans.Attack);
                if (!IsCombatRange()) _breathBar.SetDodge(dodgeForm);
                _breathBar.SetCombat(combatForm);
                return;
            }

            _breathBar.SetPlan(BattleM.BreathBar.Plans.Wait);
            return;

            void TryRecover()
            {
                var forceForm = CheckHealthForm();
                if (forceForm == null)//如果状态不允许等待下一个回合
                {
                    _breathBar.SetPlan(BattleM.BreathBar.Plans.Wait);
                    return;
                }
                _breathBar.SetRecover(forceForm);
                _breathBar.SetPlan(BattleM.BreathBar.Plans.Recover);
            }

            bool CheckRecover()
            {
                //回气策略
                switch (Strategy)
                {
                    case Strategies.Steady when Status.Mp.Value > 10 && IsLower(0.4f, Status):
                    case Strategies.Hazard when Status.Mp.Value > 10 && IsLower(0.2f, Status):
                    case Strategies.Defend when Status.Mp.Value > 10 && IsLower(0.8f, Status):
                    case Strategies.RunAway when Status.Mp.Value > 10 && IsLower(0.8f, Status):
                    case Strategies.DeathFight when Status.Mp.Value > 10 && IsLower(0.4f, Status):
                        return true;
                    default:
                        return false;
                }
            }
        }

        public bool IsCombatRange() => Equipment.Armed.InCombatRange(Target.Distance(this));
            
        //获取轻功招式
        private IDodgeForm GetDodgeForm()
        {
            if (DodgeSkill != null && DodgeSkill.Forms.Any())
                return DodgeSkill?.Forms
                    .Where(d => d.Mp <= Status.Mp.Value) //先排除不够内力的招式
                    .OrderBy(_ => Random.Next(100)) //随机一式
                    .FirstOrDefault();
            return Status.Tp.Value > DefaultDodge.Qi ? DefaultDodge : null;
        }
        //获取战斗招式
        private ICombatForm GetCombatForm()
        {
            if (CombatSkill != null && CombatSkill.Combats.Any())
                return CombatSkill?.Combats
                    .Where(c => c.Mp <= Status.Mp.Value) //先排除不够内力的招式
                    .OrderBy(_ => Random.Next(100)) //随机一式
                    .FirstOrDefault();
            return Status.Tp.Value > DefaultCombat.Qi ? //如果找不到，直接用第一式
                DefaultCombat : null;
        }

        #endregion

        #region Action
        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="breathes"></param>
        /// <returns></returns>
        public void Action(int breathes)
        {
            switch (_breathBar.Plan)
            {
                case BattleM.BreathBar.Plans.Attack:
                case BattleM.BreathBar.Plans.Recover:
                    _breathBar.BreathConsume(breathes, OnRecovery, OnBattle);
                    break;
                case BattleM.BreathBar.Plans.Wait:
                    WaitingRecovery();
                    break;
                case BattleM.BreathBar.Plans.Surrender:
                    _breathBar.BreathConsume(breathes, OnRecovery, OnEscape);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        #endregion
        public void WaitingRecovery()
        {
            OnNonCombatRecover(0, 0, 5);
        }

        #region Battle
        private void OnBattle(IDodgeForm dodge, ICombatForm combat)
        {
            if (dodge != null)
            {
                OnDodge(dodge, false);
            }
            if (combat != null) OnCombat(combat);
        }
        private void OnCombat(ICombatForm combat)
        {
            var rec = ConsumeRecord<ICombatForm>.Instance(combat);
            rec.Set(this, () => ConsumeForm(combat));
            OnCombatConsume?.Invoke(rec);
            Round.OnAttack(this, combat, Target);
        }

        #endregion

        #region Escape
        private void OnEscape(IDodgeForm dodge, ICombatForm combat)
        {
            if(Mgr.Judge == CombatManager.Judgment.Duel)
            {
                OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.TryEscape));
                dodge ??= PickDodge();
                OnDodge(dodge, true);
                SetBusy(1);
                var tar = Mgr.GetAliveCombatUnit(Target);
                var avoidEscape = Target.Distance(this) <= 3;

                if (tar.Target != this)
                    avoidEscape = tar.FlingToAvoidEscape(dodge);

                if (avoidEscape)
                {
                    Mgr.CheckExhausted();
                    return;
                }
            }
            OnFightEvent?.Invoke(EventRecord.Instance(this,FightFragment.Types.Escaped));
            Mgr.RemoveUnit(this);
        }
        
        private bool FlingToAvoidEscape(IDodgeForm dodge)
        {
            if (Equipment.Fling != null)
            {
                var combat = PickCombat();
                var isAvoidEscape = Round.FlingOnTargetEscape(this, Target, combat, dodge);
                Equipment.FlingConsume();
                return isAvoidEscape;
            }

            return false;
        }

        #endregion

        #region Recover

        private void OnNonCombatRecover(int hp, int tp, int mp)
        {
            var rec = ConsumeRecord.Instance();
            rec.Set(this, () =>
            {
                Status.Hp.Add(hp);
                Status.Tp.Add(tp);
                Status.Mp.Add(mp);
            });
            OnConsume?.Invoke(rec);
        }

        private void OnRecovery(IForceForm forceForm)
        {
            var rec = ConsumeRecord<IForceForm>.Instance(forceForm);
            rec.Set(this, () =>
            {
                var list = new[] { Status.Hp, Status.Tp };

                foreach (var con in list.OrderBy(c => c.ValueMaxRatio))
                {
                    var gap = con.Max - con.Value;
                    var recover = Math.Min(gap, Status.Mp.Value);
                    con.Add(recover);
                    Status.Mp.Add(-recover);
                }
            });
            OnRecoverConsume?.Invoke(rec);
        }
        #endregion

        private void OnDodge(IDodgeForm dodge, bool isEscape)
        {
            var rec = ConsumeRecord<IDodgeForm>.Instance(dodge);
            rec.Set(this, () =>
            {
                ConsumeForm(dodge);
                Round.AdjustCombatDistance(this, Target, isEscape);
            });
            OnDodgeConsume?.Invoke(rec);
        }

        public void SetBusy(int busy) => _breathBar.SetBusy(busy);

        private void ConsumeForm(IDepletionForm form)
        {
            Status.Tp.Add(-form.Qi);
            Status.Mp.Add(-form.Mp);
        }

        public void DodgeFromAttack(IDodgeForm dodge)
        {
            OnDodge(dodge, false);
        }

        public void SufferDamage(int finalDamage, Weapon.Injuries kind)
        {
            var rec = ConsumeRecord.Instance();
            rec.Set(this, () =>
            {
                Status.Hp.Add(-finalDamage);
                var injury = Random.Next(finalDamage);

                switch (kind)
                {
                    case Weapon.Injuries.Blunt:
                        var inj = Random.Next(injury);
                        Status.Tp.AddMax(-inj);
                        Status.Hp.AddMax(-injury + inj);
                        break;
                    case Weapon.Injuries.Sharp:
                        Status.Hp.AddMax(-injury);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }
            });
            OnConsume?.Invoke(rec);
        }
        public void ExhaustedAction()
        {
            OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.Exhausted));
        }
        public void DeathAction()
        {
            IsDeath = true;
            OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.Death));
        }
        private IForceForm CheckHealthForm()
        {
            var force = ForceSkill.Forms.LastOrDefault() ?? DefaultForce.Forms.First();
            return Status.Mp.Value >= 10 ? force : null;
        }
        private bool IsLower(float heathRatio, ICombatStatus status)
        {
            return status.Hp.ValueMaxRatio < heathRatio ||
                   status.Tp.ValueMaxRatio < heathRatio;
        }

        private class BasicCombat : ICombatForm
        {
            public string Name => "王八拳";
            public int Qi => 10;
            public int Mp => 0;
            public int Breath => 5;
            public int OffBusy => 0;
            public int TarBusy => 0;
            public Way.Armed Armed => Way.Armed.Unarmed;
            public override string ToString() => Name;
        }
        private class BasicDodge : IDodgeForm
        {
            public string Name => string.Empty;
            public int Qi => 5;
            public int Mp => 0;
            public int Breath => 3;
            public int Dodge => 1;
            public override string ToString() => Name;
        }
        private class BasicParry : IParryForm
        {
            public string Name => string.Empty;
            public int Qi => 5;
            public int Mp => 0;
            public int Parry => 1;
            public int OffBusy => 1;
            public override string ToString() => Name;
        }
        private class BasicForce : IForce
        {
            private static IList<IForceForm> forms = new IForceForm[] { new BasicForm() };
            public string Name => "基础呼吸";
            public int MpRate => 1;
            public int MpArmor => 0;
            public IList<IForceForm> Forms => forms;

            private class BasicForm : IForceForm
            {
                public string Name => "深呼吸";
                public int Breath => 5;
            }
        }
        public int CompareTo(CombatUnit other) => BreathBar.CompareTo(other.BreathBar);
        public override string ToString() => Name;
    }
}