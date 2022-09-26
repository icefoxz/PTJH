using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    public enum CombatPlans
    {
        Attack,
        RecoverHp,
        RecoverTp,
        Exert,
        Wait,
        Surrender
    }

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
        bool IsExhausted { get; }
        int Distance(ICombatInfo target);
        bool IsCombatRange(ICombatInfo unit);
        bool IsTargetRange();
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
        ICombatForm[] CombatForms { get; }
        IParryForm[] ParryForms { get; }
        IDodgeForm[] DodgeForms { get; }
        void SetStandingPoint(int standingPoint);
        void SetStrategy(CombatUnit.Strategies strategy);
        void SetCombatId(int combatId);
        bool IsCombatFormAvailable(ICombatForm form);
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
        private ICombatRound Round { get; set; }
        private static readonly BasicDodge DefaultDodge = new();
        private static readonly BasicCombat DefaultCombat = new();
        private static readonly BasicParry DefaultParry = new();
        private static readonly BasicForce DefaultForce = new();

        public static CombatUnit Instance(string name, int strength, int agility, ICombatStatus status,
            IForce forceSkill, IMartial martial,
            IDodge dodgeSkill, IEquip equip) => new(name, strength, agility, status, forceSkill,
            martial.Combats.ToArray(), martial.Parries.ToArray(), dodgeSkill.Forms.ToArray(), equip);

        public static CombatUnit Instance(ICombatUnit o) => new(o.Name, 
            o.Strength, o.Agility, o.Status.Clone(),
            o.ForceSkill, o.CombatForms, o.ParryForms, 
            o.DodgeForms, o.Equipment);
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
        public ICombatForm[] CombatForms { get; }
        public IParryForm[] ParryForms { get; }
        public IDodgeForm[] DodgeForms { get; }
        public void SetStandingPoint(int standingPoint) => StandingPoint = standingPoint;
        public void SetStrategy(Strategies strategy) => Strategy = strategy;
        public void SetCombatId(int combatId) => CombatId = combatId;
        public IEquipment Equipment { get; }
        /// <summary>
        /// 是否逃跑状态，如果死战会一直返回false
        /// </summary>
        public bool IsSurrenderCondition
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
        public event Action<SwitchTargetRecord> OnSwitchTargetEvent;
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
        public CombatPlans Plan => _breathBar.Plan;

        public IDodgeForm PickDodge() => DodgeForms?.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultDodge;
        public ICombatForm PickCombat() => CombatForms?.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultCombat;
        public IParryForm PickParry() => ParryForms?.OrderBy(_ => Random.Next(100)).FirstOrDefault() ?? DefaultParry;

        private CombatUnit(string name, int strength, int agility, 
            ICombatStatus status, IForce forceSkill,
            ICombatForm[] combatForms, IParryForm[] parryForms,
            IDodgeForm[] dodgeForms, IEquip equip)
        {
            Name = name;
            Strength = strength;
            Agility = agility;
            Status = status;
            ForceSkill = forceSkill ?? DefaultForce;
            CombatForms = combatForms;
            DodgeForms = dodgeForms;
            ParryForms = parryForms;
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
            BuffMgr = mgr.BuffMgr;
            ChangeTarget(target);
        }

        /// <summary>
        /// 换目标
        /// </summary>
        /// <param name="target"></param>
        public void ChangeTarget(ICombatInfo target) => Target = target;
        public void BreathCharge(int charge)
        {
            if (charge <= 0) return;
            OnIdleRecovery(charge);
            _breathBar.Charge(charge);
        }

        #region BuffManager
        private CombatBuffManager BuffMgr { get; set; }
        public IEnumerable<ICombatBuff> Buffs => BuffMgr.GetBuffs(CombatId);


        #endregion

        #region CombatPlan
        /// <summary>
        /// 准备下一招
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AutoCombatPlan()
        {
            var target = Mgr.GetAliveCombatUnit(Target);
            if(target == null)
            {
                Mgr.SetTargetFor(this);
                SwitchTargetEvent();
                target = Mgr.GetAliveCombatUnit(Target);
            }

            var strategy = new CombatUnitSummary(this, target, GetCombatForm(), GetDodgeForm(), GetForceForm());
            var plan = strategy.GetPlan();
            switch (plan)
            {
                case CombatPlans.Attack:
                    AttackPlan(strategy.CombatForm, strategy.DodgeForm);
                    break;
                case CombatPlans.RecoverHp:
                    RecoverHpPlan(strategy.RecoverForm);
                    break;
                case CombatPlans.RecoverTp:
                    RecoverTpPlan(strategy.RecoverForm);
                    break;
                case CombatPlans.Exert:
                    ExertPlan(strategy.RecoverForm);
                    break;
                case CombatPlans.Wait:
                    WaitPlan();
                    break;
                case CombatPlans.Surrender:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            //if (IsSurrenderCondition)
            //{
            //    if (Mgr.GetAliveCombatUnit(Target).IsSurrenderCondition) //如果对方打算逃跑，可以等待或恢复
            //    {
            //        var hpLow = CheckRecoverNeed(Status.Hp);
            //        var tpLow = CheckRecoverNeed(Status.Tp);
            //        if (hpLow)
            //        {
            //            TryRecoverPlan(CombatPlans.RecoverHp);
            //            return;
            //        }

            //        if (tpLow)
            //        {
            //            TryRecoverPlan(CombatPlans.RecoverTp);
            //            return;
            //        }
            //    }//如果对方没有打算逃跑，己方准备认输模式(逃跑，认输)
            //    _breathBar.SetPlan(CombatPlans.Surrender);
            //    return;
            //}

            //if (Target == null || Target.IsExhausted) //切换对手
            //{
            //    Mgr.SetTargetFor(this);
            //    SwitchTargetEvent();
            //}

            //var combatForm = GetCombatForm();//获取攻击招式
            //var dodgeForm = GetDodgeForm();//获取身法
            //var isReachable = IsTargetRange() || dodgeForm != null;
            //var isHpRecoverNeed = CheckRecoverNeed(Status.Hp);
            //var isTpRecoverNeed = CheckRecoverNeed(Status.Tp);
            //if (isHpRecoverNeed)
            //{
            //    TryRecoverPlan(CombatPlans.RecoverHp);
            //    return;
            //}

            //if (isTpRecoverNeed)
            //{
            //    TryRecoverPlan(CombatPlans.RecoverTp);
            //    return;
            //}

            //if (combatForm != null && isReachable && !IsSurrenderCondition)
            //{
            //    if (IsTargetRange()) dodgeForm = null;
            //    AttackPlan(combatForm, dodgeForm);
            //    return;
            //}

            //WaitPlan();
            //return;

            //void TryRecoverPlan(CombatPlans recoverPlan)
            //{
            //    var forceForm = GetForceForm();
            //    if (forceForm == null)//如果状态不允许等待下一个回合
            //    {
            //        //_breathBar.SetBusy(1);//暂时状态不允许+1硬直
            //        //_breathBar.SetPlan(BattleM.BreathBar.Plans.Wait);
            //        WaitPlan();
            //        return;
            //    }

            //    switch (recoverPlan)
            //    {
            //        case CombatPlans.RecoverHp: RecoverHpPlan(forceForm); return;
            //        case CombatPlans.RecoverTp: RecoverTpPlan(forceForm); return;
            //        case CombatPlans.Attack:
            //        case CombatPlans.Exert:
            //        case CombatPlans.Wait:
            //        case CombatPlans.Surrender:
            //        default:
            //            throw new ArgumentOutOfRangeException(nameof(recoverPlan), recoverPlan, null);
            //    }
            //}

            //bool CheckRecoverNeed(IGameCondition gameCondition)
            //{
            //    //回气策略
            //    switch (Strategy)
            //    {
            //        case Strategies.Steady when Status.Mp.Value > 10 && LowerThan(0.4f, gameCondition):
            //        case Strategies.Hazard when Status.Mp.Value > 10 && LowerThan(0.2f, gameCondition):
            //        case Strategies.Defend when Status.Mp.Value > 10 && LowerThan(0.8f, gameCondition):
            //        case Strategies.RunAway when Status.Mp.Value > 10 && LowerThan(0.8f, gameCondition):
            //        case Strategies.DeathFight when Status.Mp.Value > 10 && LowerThan(0.4f, gameCondition):
            //            return true;
            //        default:
            //            return false;
            //    }
            //}

            //bool LowerThan(float ratio, IGameCondition con) => con.ValueMaxRatio < ratio;
        }

        public void AttackPlan(ICombatForm combat,IDodgeForm dodge)
        {
            _breathBar.SetPlan(CombatPlans.Attack);
            _breathBar.SetDodge(dodge);
            _breathBar.SetCombat(combat);
        }
        public void WaitPlan()
        {
            _breathBar.SetPlan(CombatPlans.Wait);
        }

        public void RecoverHpPlan(IForceForm forceForm)
        {
            _breathBar.SetRecover(forceForm);
            _breathBar.SetPlan(CombatPlans.RecoverHp);
        }

        public void RecoverTpPlan(IForceForm forceForm)
        {
            _breathBar.SetRecover(forceForm);
            _breathBar.SetPlan(CombatPlans.RecoverTp);
        }

        public void ExertPlan(IForceForm force)
        {
            //todo: 暂时没有特殊招方式
            throw new NotImplementedException();
            _breathBar.SetRecover(force);
            _breathBar.SetPlan(CombatPlans.RecoverHp);
        }

        public bool IsCombatFormAvailable(ICombatForm form)
        {
            return Status.Mp.Value > form.Mp &&
                   Status.Tp.Value > form.Tp;
        }

        public bool IsTargetRange() => IsCombatRange(Target);
        public bool IsCombatRange(ICombatInfo unit) => Equipment.Armed.InCombatRange(unit.Distance(this));

        //获取内功招式    
        public IForceForm GetForceForm()
        {
            var force = ForceSkill.Forms.LastOrDefault() ?? DefaultForce.Forms.First();
            return Status.Mp.Value >= 10 ? force : null;
        }
        //获取轻功招式
        private IDodgeForm GetDodgeForm()
        {
            if (DodgeForms != null && DodgeForms.Any())
                return DodgeForms?
                    .Where(d => d.Mp <= Status.Mp.Value) //先排除不够内力的招式
                    .OrderBy(_ => Random.Next(100)) //随机一式
                    .FirstOrDefault();
            return Status.Tp.Value > DefaultDodge.Tp ? DefaultDodge : null;
        }
        //获取战斗招式
        private ICombatForm GetCombatForm()
        {
            if (CombatForms != null && CombatForms.Any())
                return CombatForms?
                    .Where(IsCombatFormAvailable) //先排除不够内力的招式
                    .OrderBy(_ => Random.Next(100)) //随机一式
                    .FirstOrDefault();
            return Status.Tp.Value > DefaultCombat.Tp ? //如果找不到，直接用第一式
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
            var strategy = new CombatUnitSummary(this, Mgr.GetAliveCombatUnit(Target), 
                _breathBar.Combat, _breathBar.Dodge, _breathBar.Recover);
            var plan = strategy.GetPlan();
            switch (plan)
            {
                case CombatPlans.Attack:
                    {
                        _breathBar.BreathConsume(breathes);
                        OnBattle(_breathBar.Dodge, _breathBar.Combat);
                    }
                    return;
                case CombatPlans.RecoverHp:
                    {
                        _breathBar.BreathConsume(breathes);
                        Recovery(Status.Hp, ForceSkill, _breathBar.Recover);
                    }
                    return;
                case CombatPlans.RecoverTp:
                    {
                        _breathBar.BreathConsume(breathes);
                        Recovery(Status.Tp, ForceSkill, _breathBar.Recover);
                    }
                    return;
                case CombatPlans.Wait:
                    return;
                case CombatPlans.Surrender:
                    {
                        _breathBar.BreathConsume(breathes);
                        OnEscape(_breathBar.Dodge, _breathBar.Combat);
                    }
                    return;
                case CombatPlans.Exert:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new ArgumentOutOfRangeException(nameof(_breathBar.Plan), $"{_breathBar.Plan}");
        }

        #endregion
        private void OnIdleRecovery(int charge) => OnNonCombatRecover(0, 0, charge);

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
                dodge ??= PickDodge();
                var isSuccess = Round.OnTryEscape(this, dodge);
                OnDodge(dodge, true);
                if (!isSuccess)
                {
                    Mgr.CheckExhausted();
                    return;
                }
            }
            OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.Escaped));
            Mgr.RemoveAlive(this);
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

        private void Recovery(IGameCondition con, IForce force, IForceForm forceForm)
        {
            var rec = ConsumeRecord<IForceForm>.Instance(forceForm);
            rec.Set(this, () =>
            {
                //var list = new[] { Status.Hp, Status.Tp };
                //var totalGap = Status.Hp.Max - Status.Hp.Value + Status.Tp.Max - Status.Tp.Value;
                //var mp = Math.Min(totalGap, Status.Mp.Value);
                //var exert = ExertFormula.Instance(mp, forceForm.Breath, force.MpRate);
                //var final = exert.Finalize;
                //foreach (var con in list.OrderBy(c => c.ValueMaxRatio))
                //{
                //    if (final <= 0) break;
                //    var gap = con.Max - con.Value;
                //    var recover = Math.Min(gap, final);
                //    con.Add(recover);
                //    final -= recover;
                //}
                var gap = con.Max - con.Value; //差距值
                var mpRequired = RecoverFormula.MpRequire(gap, force.MpRate, forceForm.Breath); //需要内力
                var mp = Math.Min(mpRequired, Status.Mp.Value); //需要内力与剩余内力获取最低(内力不够的情况)
                var formula = RecoverFormula.Instance(mp, forceForm.Breath, force.MpRate);
                con.Add(formula.Finalize);
                Status.Mp.Add(-mp);
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
            Status.Tp.Add(-form.Tp);
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

        public void SwitchTargetEvent() =>
            OnSwitchTargetEvent?.Invoke(SwitchTargetRecord.Instance(this, Target.CombatId));
        public void ExhaustedAction()
        {
            OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.Exhausted));
        }
        public void DeathAction()
        {
            IsDeath = true;
            OnFightEvent?.Invoke(EventRecord.Instance(this, FightFragment.Types.Death));
        }

        private class BasicCombat : ICombatForm
        {
            public string Name => "王八拳";
            public int Tp => 10;
            public int Mp => 0;
            public int Breath => 5;
            public int OffBusy => 0;
            public ICombo Combo { get; }
            public int TarBusy => 0;
            public Way.Armed Armed => Way.Armed.Unarmed;
            public override string ToString() => Name;
        }
        private class BasicDodge : IDodgeForm
        {
            public string Name => string.Empty;
            public int Tp => 5;
            public int Mp => 0;
            public int Breath => 3;
            public int Dodge => 1;
            public override string ToString() => Name;
        }
        private class BasicParry : IParryForm
        {
            public string Name => string.Empty;
            public int Tp => 5;
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