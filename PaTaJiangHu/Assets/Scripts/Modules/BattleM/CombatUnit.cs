using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    public enum CombatPlans
    {
        /// <summary>
        /// 等待策略，不做任何事
        /// </summary>
        Wait,
        /// <summary>
        /// 进攻策略，主动发动攻击
        /// </summary>
        Attack,
        /// <summary>
        /// 恢复血量策略
        /// </summary>
        RecoverHp,
        /// <summary>
        /// 发招策略(玩家主动介入)
        /// </summary>
        Exert,
        /// <summary>
        /// 投降策略，根据战况决定是否等待，逃跑，或是直接投降
        /// </summary>
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
        IForce Force { get; }
        ICombatForm[] CombatForms { get; }
        IDodge Dodge { get; }
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
        private static readonly BasicForce DefaultForce = new();

        public static CombatUnit Instance(string name, int strength, int agility, ICombatStatus status,
            IForce forceSkill, IMartial martial,
            IDodge dodgeSkill, IEquip equip) => new(name, strength, agility, status, forceSkill,
            martial.Combats.ToArray(), dodgeSkill, equip);

        public static CombatUnit Instance(ICombatUnit o) => new(o.Name, 
            o.Strength, o.Agility, o.Status.Clone(),
            o.Force, o.CombatForms, 
            o.Dodge, o.Equipment);
        public int StandingPoint { get; private set; }
        public string Name { get; }
        public bool IsDeath { get; private set; }
        public int CombatId { get; private set; }
        public int Strength { get; }
        public int Agility { get; }
        public Strategies Strategy { get; private set; }
        public ICombatStatus Status { get; }
        public IBreathBar BreathBar => _breathBar;
        public IForce Force { get; }
        public ICombatForm[] CombatForms { get; }
        public IDodge Dodge { get; }
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
                    return _breathBar.Round.RoundIndex >= Round.MinEscapeRounds &&
                           (value > status.Hp.ValueFixRatio);
                }
            }
        }

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

        private CombatUnit(string name, int strength, int agility, 
            ICombatStatus status, IForce forceSkill,
            ICombatForm[] combatForms, IDodge dodge, IEquip equip)
        {
            Name = name;
            Strength = strength;
            Agility = agility;
            Status = status;
            Force = forceSkill ?? DefaultForce;
            CombatForms = combatForms;
            Dodge = dodge;
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
            _breathBar.Charge(charge);
            if (_breathBar.TotalCharged > 0)//如果蓄力与硬直抵消不了就不增加内力
                MpCharge(_breathBar.TotalCharged);
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
            var target = Mgr.TryGetAliveCombatUnit(Target);
            if (target == null)
            {
                Mgr.SetTargetFor(this);
                Round.OnTargetSwitch(this, Target);
            }
            var(isDodgeAvailable, dodge) = GetDodgeForm();
            var (isCombatAvailable, combat) = GetCombatForm();
            var strategy =
                new CombatUnitSummary(this, target, combat, dodge, Force, isCombatAvailable, isDodgeAvailable);
            _breathBar.SetPlan(Mgr.Judge, strategy);
        }

        private bool IsCombatFormAvailable(ICombatForm form)
        {
            return Status.Mp.Value > form.CombatMp;
        }

        public bool IsTargetRange() => IsCombatRange(Target);
        public bool IsCombatRange(ICombatInfo unit) => Equipment.Armed.InCombatRange(unit.Distance(this));

        //获取轻功招式
        private (bool isDodgeAvailable,IDodge dodge) GetDodgeForm()
        {
            if (Dodge != null && Dodge.DodgeMp <= Status.Mp.Value)
                return (true, Dodge);
            return (Status.Mp.Value > DefaultDodge.DodgeMp, DefaultDodge);
        }
        //获取战斗招式
        private (bool isCombatAvailable, ICombatForm combat) GetCombatForm()
        {
            if (CombatForms != null && CombatForms.Any())
            {
                var combat = CombatForms
                    .Where(IsCombatFormAvailable) //先排除不够内力的招式
                    .OrderBy(_ => Random.Next(100)) //随机一式
                    .FirstOrDefault();
                return combat != null ? (true, combat) : (false ,CombatForms.First()); //如果没有内力只能使用第一式
            }
            return (false, DefaultCombat);
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
                case CombatPlans.Attack:
                    {
                        _breathBar.BreathConsume(breathes);
                        Round.OnAttack(this, _breathBar, Target);
                    }
                    return;
                case CombatPlans.RecoverHp:
                    {
                        _breathBar.BreathConsume(breathes);
                        Recovery(Status.Hp, Force, _breathBar.Force);
                    }
                    return;
                case CombatPlans.Wait:
                    return;
                case CombatPlans.Surrender:
                    {
                        _breathBar.BreathConsume(breathes);
                        Round.OnEscape(this, _breathBar.Dodge);
                    }
                    return;
                case CombatPlans.Exert:
                default:
                    throw new ArgumentOutOfRangeException();
            }
            throw new ArgumentOutOfRangeException(nameof(_breathBar.Plan), $"{_breathBar.Plan}");
        }

        #endregion
        private void MpCharge(int charge)
        {
            var rec = ConsumeRecord.Instance();
            rec.Set(this, () => Status.Mp.Add(charge));
            Round.RecRecharge(new RechargeRecord(charge, rec));
        }

        #region Recover

        private void Recovery(IGameCondition con, IForce force, IForce forceForm)
        {
            var rec = RecoveryRecord.Instance(force);
            rec.Set(this, () =>
            {
                var formula = RecoverFormula.Instance(forceForm.Recover, force.ForceRate);
                con.Add(formula.Finalize);
                Status.Mp.Add(-formula.Mp);
            });
            Round.RecForceRecovery(rec);
        }

        #endregion

        public void SetBusy(int busy) => _breathBar.AddBusy(busy);

        public void ConsumeForm(ICombatForm form)
        {
            Status.Mp.Add(-form.CombatMp);
        }
        public void ConsumeForm(IDodge dodge)
        {
            Status.Mp.Add(-dodge.DodgeMp);
        }
        public void ConsumeForm(IParryForm form)
        {
            Status.Mp.Add(-form.ParryMp);
        }

        /// <summary>
        /// 消耗内甲，如果内力不够不消耗
        /// </summary>
        public bool ArmorDepletion()
        {
            if (Status.Mp.Value < Force.ArmorDepletion) return false;
            Status.Mp.Add(-Force.ArmorDepletion);
            return true;
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
                        Status.Mp.AddMax(-inj);
                        Status.Hp.AddMax(-injury + inj);
                        break;
                    case Weapon.Injuries.Sharp:
                        Status.Hp.AddMax(-injury);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
                }
            });
        }

        public void DeathAction() => IsDeath = true;

        private class BasicCombat : ICombatForm
        {
            public string Name => "王八拳";
            public int CombatMp => 0;
            public int Breath => 5;
            public int Parry => 1;
            public int ParryMp => 1;
            public int OffBusy => 0;
            public ICombo Combo { get; }
            public int TarBusy => 0;
            public Way.Armed Armed => Way.Armed.Unarmed;
            public override string ToString() => Name;
        }
        private class BasicDodge : IDodge
        {
            public string Name => string.Empty;
            public int DodgeMp => 0;
            public int Breath => 3;
            public int Dodge => 1;
            public override string ToString() => Name;
        }
        private class BasicForce : IForce
        {
            public string Name => "深呼吸";
            public int ForceRate => 0;
            public int MpConvert => 10;
            public int Recover => 10;
            public int ArmorDepletion => 0;
            public int Armor => 0;
            public int Breath => 5;
        }
        public int CompareTo(CombatUnit other) => BreathBar.CompareTo(other.BreathBar);
        public override string ToString() => Name;
    }
}