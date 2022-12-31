using System;
using System.Collections.Generic;
using System.Linq;
using Server.Configs._script.Skills;

namespace BattleM
{
    public enum CombatEvents
    {
        /// <summary>
        /// 等待策略，不做任何事
        /// </summary>
        Wait,
        /// <summary>
        /// 主动策略，一般为发动攻击
        /// </summary>
        Offend,
        /// <summary>
        /// 恢复血量策略
        /// </summary>
        Recover,
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
        bool IsSurrenderCondition { get; }
        int Distance(ICombatInfo target);
    }
    /// <summary>
    /// 战斗单位
    /// </summary>
    public interface ICombatUnit : ICombatInfo
    {
        int Strength { get; }
        int Agility { get; }
        int WeaponDamage { get; }
        ICombatStatus Status { get; }
        IBreathBar BreathBar { get; }
        IForceSkill Force { get; }
        ICombatForm[] CombatForms { get; }
        IDodgeSkill Dodge { get; }
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
            IForceSkill forceSkill, ICombatSkill combatSkill,
            IDodgeSkill dodgeSkill, IEquip equip) => new(name, strength, agility, status, forceSkill,
            combatSkill.Combats.ToArray(), dodgeSkill, equip);

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
        public IForceSkill Force { get; }
        public ICombatForm[] CombatForms { get; }
        public IDodgeSkill Dodge { get; }
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
                    return Round.RoundIndex >= Round.MinEscapeRounds &&
                           (value > status.Hp.ValueFixRatio);
                }
            }
        }

        private CombatUnitManager Mgr { get; set; }
        #region ICombatTarget

        public ICombatInfo Target { get; private set; }
        public int Position { get; private set; }
        public bool IsBusy => BreathBar.Busies.Any();
        public int WeaponDamage => Equipment.Weapon?.Damage ?? 0;
        public Weapon.Injuries WeaponInjuryType => Equipment.Weapon?.Injury ?? Weapon.Injuries.Blunt;
        public int Armor => Equipment.Armor?.Def ?? 0;
        public bool IsExhausted => IsDeath || Status.IsExhausted;
        public CombatEvents Plan => _breathBar.AutoPlan;
        #endregion

        private CombatUnit(string name, int strength, int agility, 
            ICombatStatus status, IForceSkill forceSkill,
            ICombatForm[] combatForms, IDodgeSkill dodge, IEquip equip)
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
        /// <summary>
        /// 仅根据Position找出位置的距离(差值)，不考虑是否目标已死亡
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public int Distance(ICombatInfo target) => Math.Abs(Position - target.Position);

        /// <summary>
        /// 初始化战斗
        /// </summary>
        /// <param name="mgr"></param>
        /// <param name="round"></param>
        /// <param name="target"></param>
        /// <param name="standingPoint"></param>
        /// <param name="strategy"></param>
        public void Init(CombatUnitManager mgr,CombatRound round, ICombatInfo target,int standingPoint,Strategies strategy)
        {
            Mgr = mgr;
            Round = round;
            _breathBar = new BreathBar();
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
            if (charge <= 0) return;
            _breathBar.Charge(charge);
            if (_breathBar.TotalCharged > 0) //仅增加蓄力值，并不被硬直影响
                Round.RecRecharge(this, _breathBar.TotalCharged);
        }

        /// <summary>
        /// 准备下一招
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AutoCombatPlan()
        {
            if (!Mgr.IsAlive(Target))
            {
                Mgr.SetTargetFor(this);
                Round.OnTargetSwitch(this, Target);
            }

            var (isDodgeAvailable, dodge) = GetDodgeForm();
            var (isCombatAvailable, combat) = GetCombatForm();
            var strategy =
                new CombatUnitSummary(this, Target, combat, dodge, Force, isCombatAvailable, isDodgeAvailable);
            _breathBar.SetAutoPlan(Mgr.Judge, strategy);

            //获取轻功招式
            (bool isDodgeAvailable, IDodgeSkill dodge) GetDodgeForm()
            {
                if (Dodge != null && Dodge.DodgeMp <= Status.Mp.Value)
                    return (true, Dodge);
                return (Status.Mp.Value > DefaultDodge.DodgeMp, DefaultDodge);
            }

            //获取战斗招式
            (bool isCombatAvailable, ICombatForm combat) GetCombatForm()
            {
                if (CombatForms != null && CombatForms.Any())
                {
                    var combat = CombatForms
                        .Where(IsCombatFormAvailable) //先排除不够内力的招式
                        .OrderBy(_ => Random.Next(100)) //随机一式
                        .FirstOrDefault();
                    return combat != null ? (true, combat) : (false, CombatForms.First()); //如果没有内力只能使用第一式
                }

                return (false, DefaultCombat);
                bool IsCombatFormAvailable(ICombatForm form) => Status.Mp.Value > form.CombatMp;
            }
        }
        /// <summary>
        /// 目标在自己的攻击范围内
        /// </summary>
        /// <returns></returns>
        public bool IsTargetInRange() => IsCombatRange(Target);
        /// <summary>
        /// 目标是否在可攻击范围内s
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public bool IsCombatRange(ICombatInfo unit) => Equipment.Armed.InCombatRange(unit.Distance(this));

        /// <summary>
        /// 开始战斗
        /// </summary>
        /// <param name="breathes"></param>
        /// <returns></returns>
        public void Action(int breathes)
        {
            if (_breathBar.AutoPlan is CombatEvents.Wait) return;
            //等待不消耗气息
            _breathBar.BreathConsume(breathes);
            var perform = _breathBar.Perform;
            switch (perform.Activity)
            {
                case IPerform.Activities.Attack:
                    Round.OnAttack(this, perform, Target);
                    break;
                case IPerform.Activities.Recover:
                    Round.OnRecovery(this, perform.Recover, perform.ForceSkill);
                    break;
                case IPerform.Activities.Auto: //主动接入不允许自动活动
                    switch (_breathBar.AutoPlan)
                    {
                        case CombatEvents.Offend:
                            Round.OnAttack(this, perform, Target);
                            break;
                        case CombatEvents.Recover:
                            Round.OnRecovery(this, perform.ForceSkill, _breathBar.Perform.ForceSkill);
                            break;
                        case CombatEvents.Surrender:
                            Round.OnEscape(this, perform.DodgeSkill);
                            break;
                        case CombatEvents.Wait:
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetExert(IExert exert) => _breathBar.SetExert(exert);
        public void SetBusy(int busy) => _breathBar.AddBusy(busy);
        public void ConsumeForm(ICombatForm form) => AddMp(-form.CombatMp);
        public void ConsumeForm(IDodgeSkill dodge) => AddMp(-dodge.DodgeMp);
        public void ConsumeForm(IParryForm form) => AddMp(-form.ParryMp);
        public void AddMp(int mp) => Status.Mp.Add(mp);
        public void AddHp(int hp) => Status.Hp.Add(hp);

        /// <summary>
        /// 消耗内甲，如果内力不够不消耗
        /// </summary>
        public bool ArmorDepletion()
        {
            if (Status.Mp.Value < Force.ArmorCost) return false;
            AddMp(-Force.ArmorCost);
            return true;
        }
        public void SufferDamage(int finalDamage, int damageMp, Weapon.Injuries kind)
        {
            var rec = ConsumeRecord.Instance();
            rec.Set(this, () =>
            {
                AddHp(-finalDamage);
                AddMp(-damageMp);
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
            public int DamageMp => 0;
            public int Breath => 5;
            public int Parry => 1;
            public int ParryMp => 1;
            public int OffBusy => 0;
            public IRecovery Recover { get; }
            public ICombo Combo { get; }

            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => Array.Empty<IBuffInstance>();
            public IEnumerable<ICombatBuff> GetAllBuffs()=> Array.Empty<ICombatBuff>();
            public IPerform.Activities Activity { get; } = IPerform.Activities.Attack;
            public int TarBusy => 0;
            public Way.Armed Armed => Way.Armed.Unarmed;
            public override string ToString() => Name;
        }
        private class BasicDodge : IDodgeSkill
        {
            public string Name => string.Empty;
            public int DodgeMp => 0;
            public int Breath => 3;
            public int Dodge => 1;
            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => Array.Empty<IBuffInstance>();
            public override string ToString() => Name;
            public SkillGrades Grade { get; } = SkillGrades.E;
            public int Level => 1;
        }
        private class BasicForce : IForceSkill
        {
            public string Name => "深呼吸";
            public int ForceRate => 0;
            public int MpCharge => 10;
            public int Recover => 10;
            public int ArmorCost => 0;
            public int Armor => 0;
            public int Breath => 5;
            public int Level => 1;
            public IBuffInstance[] GetBuffs(ICombatUnit unit,ICombatBuff.Appends append) => Array.Empty<IBuffInstance>();
            public SkillGrades Grade { get; } = SkillGrades.E;
        }
        public int CompareTo(CombatUnit other) => BreathBar.CompareTo(other.BreathBar);
        public override string ToString() =>$"{CombatId}.{Name}[{StandingPoint}]{Status}.力({Strength})敏{Agility}";
    }
}