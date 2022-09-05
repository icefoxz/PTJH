using System;
using System.Collections.Generic;

namespace BattleM
{
    public interface IAttackRecord 
    {
        IUnitRecord Unit { get; }
        IConsumeRecord Target { get; }
        ICombatForm Form { get; }
        DamageFormula DamageFormula { get; }
    }
    public interface IDodgeRecord
    {
        IUnitRecord Unit { get; }
        IDodgeForm Form { get; }
        DodgeFormula DodgeFormula { get; }
    }
    public interface IParryRecord 
    {
        IUnitRecord Unit { get; }
        IParryForm Form { get; }
        ParryFormula ParryFormula { get; }
    }
    
    public interface IUnitRecord : IStatusRecord
    {
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
        public IConditionValue Tp { get; }
        public IConditionValue Mp { get; }
    }
    public interface IConsumeRecord
    {
        string UnitName { get; }
        IStatusRecord Before { get; }
        IStatusRecord After { get; }
    }

    public record EventRecord: FightFragment
    {
        public static EventRecord Instance(ICombatUnit t, Types type) => new(t, type);
        public IUnitRecord Unit { get; }
        public override int CombatId => Unit.CombatId;

        private EventRecord(ICombatUnit unit, Types type)
        {
            Unit = new UnitRecord(unit);
            Type = type;
        }

        public override Types Type { get; }
    }
    public record SwitchTargetRecord: FightFragment
    {
        public static SwitchTargetRecord Instance(ICombatUnit t, int targetId) =>
            new(t.CombatId, targetId);
        public int TargetId { get; }
        public override int CombatId { get; }
        public override Types Type => Types.SwitchTarget;

        private SwitchTargetRecord(int combatId, int targetId)
        {
            CombatId = combatId;
            TargetId = targetId;
        }

    }

    public record ConsumeRecord : FightFragment, IConsumeRecord
    {
        public static ConsumeRecord Instance() => new ConsumeRecord();

        public string UnitName { get; private set; }
        public IStatusRecord Before { get; private set; }
        public IStatusRecord After { get; private set; }
        public override Types Type { get; } = Types.Consume;
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
        public IConditionValue Tp { get; }
        public IConditionValue Mp { get; }

        public StatusRecord(int combatId, ICombatStatus status, int breath)
        {
            Breath = breath;
            CombatId = combatId;
            var s = status.Clone();
            Hp = s.Hp;
            Tp = s.Tp;
            Mp = s.Mp;
        }

        public override string ToString() => $"Hp{Hp},Tp{Tp},Mp{Mp}";
    }

    public record ConsumeRecord<T> : ConsumeRecord
    {
        public static ConsumeRecord<T> Instance(T form)
        {
            var c = new ConsumeRecord<T>();
            return c.SetForm(form);
        }

        public T Form { get; private set; }

        public ConsumeRecord<T> SetForm(T form)
        {
            Form = form;
            return this;
        }
    }

    /// <summary>
    /// 闪避公式
    /// </summary>
    public struct DodgeFormula
    {
        private const float BusyAlignment = 0.5f;
        private const int DistanceAlignment = 10;
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
        public int DodgeCount => (Agility * (50 + Dodge) / 100) - 20;
        /// <summary>
        /// 闪避值
        /// </summary>
        public int Finalize => (int)((DodgeCount + DistanceAlign) * BusyAlign);

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
        public int ParryCount => (int)((Agility + Strength) / 2f * (50 + Parry * 10f) / 100);
        public int Finalize => (int)((ParryCount + DistanceAlign) * BusyAlign);
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
        public static int Damage(int damage) => (int)(damage * 0.2f);
    }
    /// <summary>
    /// 伤害公式
    /// </summary>
    public struct DamageFormula
    {
        private const float MpRateAlign = 0.01f;
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
    }
    /// <summary>
    /// 护甲公式
    /// </summary>
    public struct ArmorFormula
    {
        private const float MpRateAlign = 0.1f;
        /// <summary>
        /// 装备护甲
        /// </summary>
        public int Armor;
        /// <summary>
        /// 内力使用
        /// </summary>
        public int Mp;
        /// <summary>
        /// 内力转化
        /// </summary>
        public int MpRate;
        public int MpArmor => (int)(Mp * MpRateAlign * MpRate);
        public int Finalize => Armor + MpArmor;

        private ArmorFormula(int armor, int mp, int mpRate)
        {
            Armor = armor;
            Mp = mp;
            MpRate = mpRate;
        }
        public static ArmorFormula Instance(int armor, int mp, int mpRate) => new(armor, mp, mpRate);
        public override string ToString() => $"防:{Finalize}\n甲({Armor})内:{Mp}({MpRate})={MpArmor}";
    }

    public struct RecoverFormula
    {
        private const float MpRateAlign = 0.1f;
        /// <summary>
        /// 内力使用
        /// </summary>
        public int Mp;
        /// <summary>
        /// 息补充
        /// </summary>
        public int Breath;
        /// <summary>
        /// 内力转化
        /// </summary>
        public int MpRate;
        public float MpRatio => MpRateAlign * MpRate;
        public int Finalize => (int)((Mp + Breath) * MpRatio);
        /// <summary>
        /// 公式反算，需要多少内力以达到gap最终值
        /// </summary>
        /// <param name="gap"></param>
        /// <param name="mpRate"></param>
        /// <param name="breath"></param>
        /// <returns></returns>
        private RecoverFormula(int mp, int breath, int mpRate)
        {
            Mp = mp;
            Breath = breath;
            MpRate = mpRate;
        }
        public static int MpRequire(int gap, int mpRate, int breath) => (int)(gap / (MpRateAlign * mpRate) - breath);
        public static RecoverFormula Instance(int mp, int breath, int mpRate) => new(mp, breath, mpRate);
        public override string ToString() => $"回:{Finalize}\n息({Breath})+内({Mp})转({MpRate}){MpRatio:F}";
    }

    public sealed record DodgeRecord : FightFragment, IDodgeRecord
    {
        public IUnitRecord Unit { get; }
        public IDodgeForm Form { get; }
        public DodgeFormula DodgeFormula { get; }
        public override Types Type { get; } = Types.Dodge;
        public override int CombatId => Unit.CombatId;

        public DodgeRecord(ICombatUnit unit, IDodgeForm form, DodgeFormula dodgeFormula)
        {
            Unit = new UnitRecord(unit);
            Form = form;
            DodgeFormula = dodgeFormula;
        }

        public override string ToString()
        {
            return $"{Unit}\n{Form}\n{DodgeFormula}";
        }
    }

    public sealed record ParryRecord : FightFragment, IParryRecord
    {
        public IUnitRecord Unit { get; }
        public IParryForm Form { get; }
        public ParryFormula ParryFormula { get; }
        public override Types Type { get; } = Types.Parry;
        public override int CombatId => Unit.CombatId;

        public ParryRecord(ICombatUnit unit, IParryForm form, ParryFormula parryFormula)
        {
            Unit = new UnitRecord(unit);
            Form = form;
            ParryFormula = parryFormula;
        }

        public override string ToString() => $"{Unit}\n{Form}\n{ParryFormula}";
    }

    public sealed record AttackRecord : FightFragment, IAttackRecord
    {
        public IUnitRecord Unit { get; }
        public IConsumeRecord Target { get; }
        public ICombatForm Form { get; }
        public override Types Type { get; } = Types.Attack;
        public override int CombatId => Unit.CombatId;
        public DamageFormula DamageFormula { get; }

        public AttackRecord(ICombatUnit unit,  IConsumeRecord target, ICombatForm combatForm, DamageFormula damageFormula, bool isFling)
        {
            Unit = new UnitRecord(unit);
            Form = combatForm;
            DamageFormula = damageFormula;
            Target = target;
            if (isFling)
                Type = Types.Fling;
        }

        public override string ToString()
        {
            return $"{Unit}\n{Form}";
        }
    }

    public sealed record UnitRecord : IUnitRecord
    {
        public string Name { get; }
        public int CombatId { get; }
        public int Breath { get; }
        public int Strength { get; }
        public int Agility { get; }
        public int Position { get; }
        public IConditionValue Hp { get; }
        public IConditionValue Tp { get; }
        public IConditionValue Mp { get; }
        public IEquip Equip { get; }

        public UnitRecord(ICombatUnit unit)
        {
            CombatId = unit.CombatId;
            Name = unit.Name;
            Strength = unit.Strength;
            Agility = unit.Agility;
            Position = unit.Position;
            Breath = unit.BreathBar.TotalBreath;
            var s = unit.Status.Clone();
            Hp = s.Hp;
            Tp = s.Tp;
            Mp = s.Mp;
            Equip = new Equipment(unit.Equipment);
        }

        public override string ToString() => $"{Name}({Position}),力({Strength})敏({Agility}),血{Hp},气{Tp},内{Mp}";

        private record Equipment : IEquip
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
        public IUnitRecord Unit { get; }
        public IUnitRecord Target { get; }
        public override Types Type { get; } = Types.Position;
        public override int CombatId => Unit.CombatId;

        public PositionRecord(int newPos, ICombatUnit unit, ICombatUnit target)
        {
            NewPos = newPos;
            Unit = new UnitRecord(unit);
            Target = new UnitRecord(target);
        }

    }

    public interface IFightFragment
    {
        int Index { get; }
        FightFragment.Types Type { get; }
        FightFragment On<T>(Action<T> func) where T : IFightFragment;
        void OnRec(Action<ConsumeRecord> onConsume,
            Action<PositionRecord> onPositioning,
            Action<AttackRecord> onAttack,
            Action<DodgeRecord> onDodge,
            Action<ParryRecord> onParry,
            Action<EventRecord> onEvent,
            Action<SwitchTargetRecord> onSwitchTarget);
    }
    public abstract record FightFragment : IFightFragment
    {
        public enum Types
        {
            None,
            /// <summary>
            /// 消耗
            /// </summary>
            Consume,
            /// <summary>
            /// 攻击
            /// </summary>
            Attack,
            /// <summary>
            /// 招架
            /// </summary>
            Parry,
            /// <summary>
            /// 身法
            /// </summary>
            Dodge,
            /// <summary>
            /// 换位
            /// </summary>
            Position,
            /// <summary>
            /// 尝试逃跑
            /// </summary>
            TryEscape,
            /// <summary>
            /// 逃跑暗器
            /// </summary>
            Fling,
            /// <summary>
            /// 逃跑
            /// </summary>
            Escaped,
            /// <summary>
            /// 死亡
            /// </summary>
            Death,
            /// <summary>
            /// 晕厥
            /// </summary>
            Exhausted,
            /// <summary>
            /// 等待
            /// </summary>
            Wait,
            SwitchTarget
        }
        public int Index { get; private set; }
        public abstract Types Type { get; }
        public abstract int CombatId { get; }
        public void SetIndex(int index) => Index = index;

        public FightFragment On<T>(Action<T> action) where T : IFightFragment
        {
            if(this is T obj)
                action(obj);
            return this;
        }

        public void OnRec(Action<ConsumeRecord> onConsume,
            Action<PositionRecord> onPositioning,
            Action<AttackRecord> onAttack,
            Action<DodgeRecord> onDodge,
            Action<ParryRecord> onParry,
            Action<EventRecord> onEvent,
            Action<SwitchTargetRecord> onSwitchTarget)
        {
            switch (Type)
            {
                case Types.Consume:
                    onConsume?.Invoke((ConsumeRecord)this);
                    break;
                case Types.Attack:
                case Types.Fling:
                    onAttack?.Invoke((AttackRecord)this);
                    break;
                case Types.Parry:
                    onParry?.Invoke((ParryRecord)this);
                    break;
                case Types.Dodge:
                    onDodge?.Invoke((DodgeRecord)this);
                    break;
                case Types.Position:
                    onPositioning?.Invoke((PositionRecord)this);
                    break;
                case Types.TryEscape:
                case Types.Escaped:
                case Types.Death:
                case Types.Exhausted:
                case Types.Wait:
                    onEvent?.Invoke((EventRecord)this);
                    break;
                case Types.SwitchTarget:
                    onSwitchTarget?.Invoke((SwitchTargetRecord)this);
                    break;
                case Types.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    public record FightRoundRecord
    {
        private readonly List<FightFragment> _records;
        private int _index = 0;
        public bool IsFightEnd { get; private set; }
        public int Round { get; }

        public IReadOnlyList<IFightFragment> Records => _records;
        
        public FightRoundRecord(int round)
        {
            Round = round;
            _records = new List<FightFragment>();
        }

        public void SetFightEnd() => IsFightEnd = true;

        public void Add(FightFragment fragment)
        {
            fragment.SetIndex(_index++);
            _records.Add(fragment);
        }
    }
}