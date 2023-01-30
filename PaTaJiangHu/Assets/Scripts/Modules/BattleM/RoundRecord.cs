using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    public interface IAttackRecord
    {
        int CombatId { get; }
        //FightFragment.Types Type { get; }
        IUnitInfo Unit { get; }
        IUnitInfo Target { get; }
        ICombatForm Form { get; }
        DamageFormula DamageFormula { get; }
        DodgeFormula DodgeFormula { get; }
        ParryFormula ParryFormula { get; }
        ConsumeRecord<ICombatForm> AttackConsume { get; set; }
        ConsumeRecord<IParryForm> ParryConsume { get; set; }
        ConsumeRecord<IDodgeSkill> DodgeConsume { get; set; }
    }
    //public interface IDodgeRecord
    //{
    //    IUnitRecord Unit { get; }
    //    IDodgeForm Form { get; }
    //    DodgeFormula DodgeFormula { get; }
    //}
    //public interface IParryRecord 
    //{
    //    IUnitRecord Unit { get; }
    //    IParryForm Form { get; }
    //    ParryFormula ParryFormula { get; }
    //}

    public interface IUnitInfo
    {
        int CombatId { get; }
        string Name { get; }
        int Strength { get; }
        int Agility { get; }
        int Position { get; }
        IEquip Equip { get; }
    }
    public interface IStatusRecord
    {
        int CombatId { get; }
        int Breath { get; }
        public IConditionValue Hp { get; }
        public IConditionValue Mp { get; }
    }
    public interface IConsumeRecord
    {
        string UnitName { get; }
        IStatusRecord Before { get; }
        IStatusRecord After { get; }
    }

    public record SubEventRecord: FightFragment
    {
        public enum EventTypes
        {
            None,
            Death,
        }
        public static SubEventRecord Instance(ICombatUnit t, EventTypes type) => new(t, type);
        public IUnitInfo Unit { get; }
        public IStatusRecord Status { get; }
        public EventTypes Type { get; }
        public override int CombatId => Unit.CombatId;

        protected SubEventRecord(ICombatUnit unit, EventTypes type)
        {
            Unit = new UnitRecord(unit);
            Status = new StatusRecord(unit.CombatId, unit.Status, unit.BreathBar.TotalBreath);
            Type = type;
        }

        //public override Types Type { get; }
    }

    public record SwitchTargetRecord : FightFragment
    {
        public static SwitchTargetRecord Instance(ICombatUnit unit, ICombatUnit target) =>
            new(unit, target);

        public UnitRecord CombatUnit { get; }
        public UnitRecord Target { get; }
        public override int CombatId => CombatUnit.CombatId;

        private SwitchTargetRecord(ICombatUnit combat, ICombatUnit target) 
        {
            CombatUnit = new UnitRecord(combat);
            Target = new UnitRecord(target);
        }
    }

    public record ConsumeRecord : FightFragment, IConsumeRecord
    {
        public static ConsumeRecord Instance() => new ConsumeRecord();

        public string UnitName { get; private set; }
        public IStatusRecord Before { get; private set; }
        public IStatusRecord After { get; private set; }
        //public override Types Type { get; } = Types.Consume;
        public override int CombatId => Before.CombatId;

        private void SetBefore(ICombatUnit unit) => Before = new StatusRecord(unit.CombatId,unit.Status, unit.BreathBar.TotalBreath);
        private void SetAfter(ICombatUnit unit) => After = new StatusRecord(unit.CombatId,unit.Status, unit.BreathBar.TotalBreath);

        public void Set(ICombatUnit unit, Action action)
        {
            UnitName = unit.Name;
            SetBefore(unit);
            action.Invoke();
            SetAfter(unit);
        }
    }
    public record StatusRecord : IStatusRecord
    {
        public int CombatId { get; }
        public int Breath { get; }
        public IConditionValue Hp { get; }
        public IConditionValue Mp { get; }

        public StatusRecord(int combatId, ICombatStatus status, int breath)
        {
            Breath = breath;
            CombatId = combatId;
            var s = status.Clone();
            Hp = s.Hp;
            Mp = s.Mp;
        }

        public override string ToString() => $"Hp{Hp},Mp{Mp}";
    }

    public record RechargeRecord : FightFragment
    {
        public int Charge { get; }
        public ConsumeRecord Consume { get; }
        public override int CombatId => Consume.CombatId;

        public RechargeRecord(int charge, ConsumeRecord consume)
        {
            Charge = charge;
            Consume = consume;
        }
    }
    public record ConsumeRecord<T> : ConsumeRecord
    {
        public static ConsumeRecord<T> Instance(T form)
        {
            var c = new ConsumeRecord<T>();
            return c.SetForm(form);
        }

        public T Form { get; private set; }

        public ConsumeRecord()
        {
            
        }
        public ConsumeRecord(T form)
        {
            Form = form;
        }

        public ConsumeRecord<T> SetForm(T form)
        {
            Form = form;
            return this;
        }
    }

    public record RecoveryRecord : ConsumeRecord<IRecovery>
    {
        public static RecoveryRecord Instance(IRecovery recover,IForceSkill force, RecoverFormula formula) => new(recover,force ,formula);
        public IForceSkill Force { get; }
        public RecoverFormula RecoverFormula { get; }
        public RecoveryRecord(IRecovery recover, IForceSkill force, RecoverFormula recoverFormula):base(recover)
        {
            RecoverFormula = recoverFormula;
            Force = force;
        }
    }

    /// <summary>
    /// 闪避公式
    /// </summary>
    public struct DodgeFormula
    {
        private const float BusyAlignment = 0.5f;
        private const int DistanceAlignment = 10;
        private const int MaxDodgeValue = 25;
        /// <summary>
        /// 身法
        /// </summary>
        public int Dodge;
        /// <summary>
        /// 敏捷
        /// </summary>
        public int Agility;
        /// <summary>
        /// 距离修正
        /// </summary>
        public int Distance;
        /// <summary>
        /// 是否硬直
        /// </summary>
        public bool IsBusy;
        /// <summary>
        /// 随机值
        /// </summary>
        public int RandomValue;
        public float BusyAlign => IsBusy ? BusyAlignment : 1;
        public float DistanceAlign
        {
            get
            {
                var value = (Distance - 1) * DistanceAlignment;
                return value < 0 ? 0 : value;
            }
        }

        /// <summary>
        /// 闪避值
        /// </summary>
        public int DodgeCount => Agility + Dodge;

        /// <summary>
        /// 闪避值
        /// </summary>
        public int Finalize => Math.Clamp(DodgeCount, 0, MaxDodgeValue);

        public bool IsSuccess => Finalize > RandomValue;
        private DodgeFormula(int dodge, int agility, int distance, bool isBusy, int randomValue)
        {
            Dodge = dodge;
            Agility = agility;
            Distance = distance;
            IsBusy = isBusy;
            RandomValue = randomValue;
        }

        public static DodgeFormula Instance(int dodge, int agility, int distance, bool isBusy, int randomValue) =>
            new(dodge, agility, distance, isBusy, randomValue);

        public override string ToString()=>$"总:{Finalize},敏({Agility}),轻({Dodge})={DodgeCount}" +
                                           $"硬({BusyAlign})," +
                                           $"距({Distance})={DistanceAlign}," +
                                           $"随({RandomValue})";
    }
    /// <summary>
    /// 招架公式
    /// </summary>
    public struct ParryFormula
    {
        private const float BusyAlignment = 0.5f;
        private const int MaxParryValue = 50;
        /// <summary>
        /// 招架值
        /// </summary>
        public int Parry;
        /// <summary>
        /// 敏捷
        /// </summary>
        public int Agility;
        /// <summary>
        /// 力量
        /// </summary>
        public int Strength;
        /// <summary>
        /// 距离修正
        /// </summary>
        public int Distance;
        /// <summary>
        /// 是否硬直
        /// </summary>
        public bool IsBusy;
        /// <summary>
        /// 随机值
        /// </summary>
        public int RandomValue;
        public float BusyAlign => IsBusy ? BusyAlignment : 1;
        public int DistanceAlign => Distance switch
        {
            <= 1 => 20, //近距离
            2 => 10, //中距离
            _ => 0 //远距离
        };
        public int ParryCount => Agility + Strength + Parry;
        public int Finalize => Math.Clamp(ParryCount, 0, MaxParryValue);
        public bool IsSuccess => Finalize > RandomValue;

        private ParryFormula(int parry, int agility, int strength, int distance, bool isBusy, int randomValue)
        {
            Parry = parry;
            Agility = agility;
            Strength = strength;
            Distance = distance;
            IsBusy = isBusy;
            RandomValue = randomValue;
        }

        public static ParryFormula Instance(int parry, int agility, int strength, int distance, bool isBusy,
            int randomValue) => new(parry, agility, strength, distance, isBusy, randomValue);

        public override string ToString() => $"总:{Finalize},敏({Agility}),力({Strength}),架({Parry})={ParryCount}," +
                                             $"硬({BusyAlign})," +
                                             $"距({Distance})={DistanceAlign}," +
                                             $"随({RandomValue})";
        /// <summary>
        /// 招架成功后伤害修正
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public static int Damage(int damage) => (int)(damage * 0.5f);
    }
    /// <summary>
    /// 伤害公式
    /// </summary>
    public struct DamageFormula
    {
        private const float MpRateAlign = 0.1f;
        /// <summary>
        /// 力量
        /// </summary>
        public int Strength;
        /// <summary>
        /// 武器力量
        /// </summary>
        public int WeaponDamage;
        /// <summary>
        /// 内力使用
        /// </summary>
        public int Mp;
        /// <summary>
        /// 伤害转化
        /// </summary>
        public int MpRate;
        /// <summary>
        /// 内功转化率
        /// </summary>
        public float MpRatio => MpRateAlign * MpRate;
        //
        public int Finalize => (Strength + WeaponDamage) + (int)(Mp * MpRatio);

        private DamageFormula(int strength, int weaponDamage, int mp, int mpRate)
        {
            Strength = strength;
            WeaponDamage = weaponDamage;
            MpRate = mpRate;
            Mp = mp;
        }

        public static DamageFormula Instance(int strength, int weaponDamage, int mp, int mpRate) =>
            new(strength, weaponDamage, mp, mpRate);

        public override string ToString() => $"伤:{Finalize},武器({WeaponDamage})+[内:{Mp}({MpRate})={MpRatio}]";

        public int GetDamage(int armor, int damageRate = 100)
        {
            return (int)(Damage(Finalize) * (1f - armor * 0.01f)); //最终伤害(扣除免伤)
            float Damage(int dmg) => 0.01f * dmg * damageRate;
        }
    }
    /// <summary>
    /// 护甲公式
    /// </summary>
    public struct ArmorFormula
    {
        private const float MpRateAlign = 0.01f;
        private const int MaxArmorValue = 25;

        /// <summary>
        /// 装备护甲
        /// </summary>
        public int Armor;
        /// <summary>
        /// 内力使用
        /// </summary>
        public int Depletion;
        ///// <summary>
        ///// 内力转化
        ///// </summary>
        //public int MpRate;
        public int MpArmor => Armor; //(int)(Armor * (1 + (MpRateAlign * MpRate)));
        public int Finalize => Math.Clamp(MpArmor, 0, MaxArmorValue);

        private ArmorFormula(int armor, int depletion)
        {
            Armor = armor;
            Depletion = depletion;
            //MpRate = mpRate;
        }
        public static ArmorFormula Instance(int armor, int depletion) => new(armor, depletion);
        public override string ToString() => $"防:{Finalize}\n甲({Armor})" +
                                             //$"倍({MpRate})" +
                                             $"耗:{Depletion}={MpArmor}";
    }

    public struct RecoverFormula
    {
        private const float MpRateAlign = 0.01f;
        /// <summary>
        /// 内力使用
        /// </summary>
        public int Mp;

        /// <summary>
        /// 内力转化
        /// </summary>
        public int MpRate;
        public float MpRatio => GetMpRatio(MpRate);
        public int Finalize => (int)(Mp * MpRatio);
        private RecoverFormula(int mp, int mpRate)
        {
            Mp = mp;
            MpRate = mpRate;
        }
        private static float GetMpRatio(int mpRate) => 1 + MpRateAlign * mpRate;
        public static RecoverFormula Instance(int mp, int mpRate) => new(mp,  mpRate);
        public override string ToString() => $"回:{Finalize}\n内({Mp})转({MpRate}){MpRatio:F}";
    }

    //public sealed record DodgeRecord : FightFragment, IDodgeRecord
    //{
    //    public IUnitRecord Unit { get; }
    //    public ConsumeRecord<IDodgeForm> Consume { get; }
    //    public IDodgeForm Form => Consume.Form;
    //    public DodgeFormula DodgeFormula { get; }
    //    public override Types Type { get; } = Types.Dodge;
    //    public override int CombatId => Unit.CombatId;

    //    public DodgeRecord(ICombatUnit unit, DodgeFormula dodgeFormula,ConsumeRecord<IDodgeForm> consumeRec)
    //    {
    //        Unit = new UnitRecord(unit);
    //        Consume = consumeRec;
    //        DodgeFormula = dodgeFormula;
    //    }

    //    public override string ToString()
    //    {
    //        return $"{Unit}\n{Form}\n{DodgeFormula}";
    //    }
    //}

    //public sealed record ParryRecord : FightFragment, IParryRecord
    //{
    //    public IUnitRecord Unit { get; }
    //    public IParryForm Form { get; }
    //    public ParryFormula ParryFormula { get; }
    //    public override Types Type { get; } = Types.Parry;
    //    public override int CombatId => Unit.CombatId;

    //    public ParryRecord(ICombatUnit unit, IParryForm form, ParryFormula parryFormula)
    //    {
    //        Unit = new UnitRecord(unit);
    //        Form = form;
    //        ParryFormula = parryFormula;
    //    }

    //    public override string ToString() => $"{Unit}\n{Form}\n{ParryFormula}";
    //}

    public sealed record AttackRecord : FightFragment, IAttackRecord
    {
        public IUnitInfo Unit { get; }
        public IUnitInfo Target { get; }
        public ICombatForm Form => AttackConsume.Form;
        //进攻消耗
        public ConsumeRecord<ICombatForm> AttackConsume { get; set; }
        //闪避消耗(状态更新1)
        public ConsumeRecord<IDodgeSkill> DodgeConsume { get; set; }
        //招架消耗(状态更新2)
        public ConsumeRecord<IParryForm> ParryConsume { get; set; }
        //承受(状态更新3)
        public ConsumeRecord Suffer { get; set; }
        //移位记录
        public PositionRecord AttackPlacing { get; set; }
        //public override Types Type { get; } = Types.Attack;
        public override int CombatId => Unit.CombatId;
        public DamageFormula DamageFormula { get; }
        public DodgeFormula DodgeFormula { get; }
        public ParryFormula ParryFormula { get; }

        public AttackRecord(ICombatUnit unit, ICombatUnit target,
            PositionRecord attackPlacing,
            ConsumeRecord<ICombatForm> attackConsume,
            ConsumeRecord<IDodgeSkill> dodgeConsume,
            ConsumeRecord<IParryForm> parryConsume,
            ConsumeRecord suffer,
            DamageFormula damageFormula, DodgeFormula dodgeFormula, ParryFormula parryFormula)
        {
            Unit = new UnitRecord(unit);
            Target = new UnitRecord(target);
            AttackPlacing = attackPlacing;
            AttackConsume = attackConsume;
            ParryConsume = parryConsume;
            DodgeConsume = dodgeConsume;
            DodgeFormula = dodgeFormula;
            ParryFormula = parryFormula;
            Suffer = suffer;
            DamageFormula = damageFormula;
        }

        public override string ToString()
        {
            return $"{Unit}\n{Form}\n{Target}\n{DodgeFormula}\n{ParryFormula}";
        }
    }
    public sealed record EscapeRecord : FightFragment
    {
        public override int CombatId => Escapee.CombatId;
        public UnitRecord Escapee { get; }
        public UnitRecord Attacker { get; }
        public ConsumeRecord<ICombatForm> AttackConsume { get; }
        public ConsumeRecord<IDodgeSkill> EscapeConsume { get; }
        public ConsumeRecord<IParryForm> ParryConsume { get; }
        public ConsumeRecord Suffer { get; }
        public DamageFormula DamageFormula { get; }
        public DodgeFormula DodgeFormula { get; }
        public ParryFormula ParryFormula { get; }
        public bool IsSuccess { get; }
        public EscapeRecord(ICombatUnit escapee, ICombatUnit attacker, 
            ConsumeRecord<ICombatForm> attackConsume, 
            ConsumeRecord<IDodgeSkill> escapeConsume, 
            ConsumeRecord<IParryForm> parryConsume, 
            ConsumeRecord suffer, 
            DamageFormula damageFormula, DodgeFormula dodgeFormula, ParryFormula parryFormula,
            bool isSuccess)
        {
            Escapee = new UnitRecord(escapee);
            Attacker = new UnitRecord(attacker);
            AttackConsume = attackConsume;
            EscapeConsume = escapeConsume;
            ParryConsume = parryConsume;
            Suffer = suffer;
            DamageFormula = damageFormula;
            DodgeFormula = dodgeFormula;
            ParryFormula = parryFormula;
            IsSuccess = isSuccess;
        }

        public EscapeRecord(ICombatUnit escapee, bool isSuccess)
        {
            Escapee = new UnitRecord(escapee);
            IsSuccess = isSuccess;
        }
    }

    public sealed record UnitInfo : IUnitInfo
    {
        public int CombatId { get; }
        public string Name { get; }
        public int Strength { get; }
        public int Agility { get; }
        public int Position { get; }
        public IEquip Equip { get; }
        public UnitInfo(ICombatUnit unit)
        {
            CombatId = unit.CombatId;
            Name = unit.Name;
            Strength = unit.Strength;
            Agility = unit.Agility;
            Position = unit.Position;
            Equip = new Equipment(unit.Equipment);
        }
    }
    public sealed record UnitRecord : IUnitInfo,IStatusRecord
    {
        public IUnitInfo Info { get; }
        public string Name => Info.Name;
        public int Strength => Info.Strength;
        public int Agility => Info.Agility;
        public int Position => Info.Position;
        public int CombatId => Info.CombatId;
        public IEquip Equip => Info.Equip;
        public int Breath { get; }

        public IConditionValue Hp { get; }
        public IConditionValue Mp { get; }

        public UnitRecord(ICombatUnit unit)
        {
            Info = new UnitInfo(unit);
            Breath = unit.BreathBar.TotalBreath;
            var s = unit.Status.Clone();
            Hp = s.Hp;
            Mp = s.Mp;
        }

        public override string ToString() => $"{Name}({Position}),力({Strength})敏({Agility}),血{Hp},内{Mp}";

        private record Equipment 
        {
            public IWeapon Weapon { get; }
            public IWeapon Fling { get; }
            public IArmor Armor { get; }
            public Equipment(IEquip e)
            {
                Weapon = e.Weapon;
                Fling = e.Fling;
                Armor = e.Armor;
            }

            public override string ToString() =>
                $"{Weapon.Name}:Dmg({Weapon.Damage}),Gra({Weapon.Grade}),Typ({Weapon.Injury}):{Weapon.FlingTimes}";
        }
    }
    public record PositionRecord : FightFragment
    {
        public int NewPos { get; }
        public IUnitInfo Unit { get; }
        public IUnitInfo Target { get; }
        //public override Types Type { get; } = Types.Position;
        public override int CombatId => Unit.CombatId;

        public PositionRecord(int newPos, ICombatUnit unit, ICombatUnit target)
        {
            NewPos = newPos;
            Unit = new UnitRecord(unit);
            Target = new UnitRecord(target);
        }

    }

    public abstract record FightFragment 
    {
        public abstract int CombatId { get; }
    }

    public record BreathRecord 
    {
        public enum Types
        {
            Busy,
            Charge,
            Exert,
            Attack,
            Placing,
        }
        public Types Type { get; }
        public string Name { get; }
        public int Value { get; }
        public int GetBreath() => Type == Types.Charge ? -Value : Value;

        public BreathRecord(Types type, int value, string name = null)
        {
            Type = type;
            Value = value;
            Name = name ?? string.Empty;
        }
    }

    public record BreathBarRecord : FightFragment
    {
        public override int CombatId => Unit.CombatId;
        public List<BreathRecord> Breathes { get; }
        public UnitRecord Unit { get; }
        public CombatEvents Plan { get; }
        public string Title { get; set; }

        public BreathBarRecord(ICombatUnit unit, List<BreathRecord> breathes)
        {
            Unit = new UnitRecord(unit);
            Breathes = breathes;
            Plan = unit.BreathBar.AutoPlan;
            Title = Plan switch
            {
                CombatEvents.Wait => "等待",
                CombatEvents.Offend => unit.BreathBar.Perform.CombatForm.Name,
                CombatEvents.Recover => unit.BreathBar.Perform.Recover.Name,
                CombatEvents.Surrender => "认输",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public int GetBreath() => Breathes.Sum(b => b.GetBreath());
    }

    public record CombatRoundRecord
    {
        /// <summary>
        /// 主要事件，每个回合必发生其中一项(战斗单位执行Action)。非辅助事件如：单位死亡，或是buff事件等
        /// </summary>
        public enum MajorEvents
        {
            /// <summary>
            /// 无事件，双方都没有触发任何主动事件
            /// </summary>
            None,
            /// <summary>
            /// 单方触发攻击事件，包括玩家介入的技能
            /// </summary>
            Combat,
            /// <summary>
            /// 单方触发恢复事件
            /// </summary>
            Recover,
            /// <summary>
            /// 逃跑事件
            /// </summary>
            Escape
        }
        //private readonly List<FightFragment> _records;
        public bool IsFightEnd { get; private set; }
        public int Round { get; }
        public MajorEvents Major { get; private set; }
        public List<BreathBarRecord> BreathBars { get; private set; } = new List<BreathBarRecord>();
        //public IReadOnlyList<IFightFragment> Records => _records;
        public int TargetId { get; set; } = -1;
        public int ExecutorId { get; set; } = -1;
        #region 主动事件
        /// <summary>
        /// 攻击招式
        /// </summary>
        public AttackRecord AttackRec { get; set; }
        public EscapeRecord EscapeRec { get; set; }
        /// <summary>
        /// 调息
        /// </summary>
        public RecoveryRecord RecoverRec { get; set; }
        /// <summary>
        /// 其它事件(非<see cref="AttackRec"/>和<see cref="RecoverRec"/>事件)
        /// </summary>
        public IList<SubEventRecord> OtherEventRec { get; set; } = new List<SubEventRecord>();
        #endregion

        /// <summary>
        /// 换锁定目标
        /// </summary>
        public SwitchTargetRecord SwitchTargetRec { get; set; }
        /// <summary>
        /// 单位喘息(各方面恢复)
        /// </summary>
        public IList<RechargeRecord> RechargeRec { get; } = new List<RechargeRecord>();

        public CombatRoundRecord(int round)
        {
            Round = round;
            //_records = new List<FightFragment>();
        }

        public void SetFightEnd() => IsFightEnd = true;

        public void SetExecutor(CombatUnit executor)
        {
            ExecutorId = executor.CombatId;
            TargetId = executor.Target?.CombatId ?? -1;
        }

        public void AddBreathBar(ICombatUnit unit)
        {
            var bars = GetBreathBarRecord(unit.BreathBar);
            BreathBars.Add(new BreathBarRecord(unit, bars));

            List<BreathRecord> GetBreathBarRecord(IBreathBar bar)
            {
                var list = new List<BreathRecord>();
                if (bar.TotalBusies > 0)
                    list.Add(new BreathRecord(BreathRecord.Types.Busy, bar.TotalBusies));
                if (bar.TotalCharged > 0)
                    list.Add(new BreathRecord(BreathRecord.Types.Charge, bar.TotalCharged));
                switch (bar.Perform.Activity)
                {
                    case IPerform.Activities.Attack:
                        if (bar.IsReposition)
                            list.Add(new BreathRecord(BreathRecord.Types.Placing, bar.Perform.DodgeSkill.Breath,
                                bar.Perform.DodgeSkill.Name));
                        list.Add(new BreathRecord(BreathRecord.Types.Attack, bar.Perform.CombatForm.Breath,
                            bar.Perform.CombatForm.Name));
                        break;
                    case IPerform.Activities.Recover:
                        list.Add(new BreathRecord(BreathRecord.Types.Exert, bar.Perform.ForceSkill.Breath,
                            bar.Perform.Recover.Name));
                        break;
                    case IPerform.Activities.Auto:
                        switch (bar.AutoPlan)
                        {
                            case CombatEvents.Offend:
                                if (bar.IsReposition)
                                    list.Add(new BreathRecord(BreathRecord.Types.Placing, bar.Perform.DodgeSkill.Breath,
                                        bar.Perform.DodgeSkill.Name));
                                list.Add(new BreathRecord(BreathRecord.Types.Attack, bar.Perform.CombatForm.Breath,
                                    bar.Perform.CombatForm.Name));
                                break;
                            case CombatEvents.Recover:
                                list.Add(new BreathRecord(BreathRecord.Types.Exert, bar.Perform.ForceSkill.Breath,
                                    bar.Perform.Recover.Name));
                                break;
                            case CombatEvents.Wait:
                            case CombatEvents.Surrender:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return list;
            }
        }

        public void SetForceRecovery(RecoveryRecord recover)
        {
            Major = MajorEvents.Recover;
            RecoverRec = recover;
        }

        public void SetAttack(AttackRecord combatForm)
        {
            Major = MajorEvents.Combat;
            AttackRec = combatForm;
        }

        public void AddRechargeRec(RechargeRecord rec)
        {
            RechargeRec.Add(rec);
        }

        public void SetSwitchTarget(SwitchTargetRecord rec)
        {
            SwitchTargetRec = rec;
        }

        public void SetSubEvent(SubEventRecord rec)
        {
            OtherEventRec.Add(rec);
        }

        public void SetEscape(EscapeRecord rec)
        {
            Major = MajorEvents.Escape;
            EscapeRec = rec;
        }
    }
}