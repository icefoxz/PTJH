using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AOT.Core.Dizi;
using GameClient.Modules.DiziM;

namespace GameClient.Modules.BattleM
{
    /// <summary>
    /// 战斗单位接口, 但不包含战斗行为, 如果战斗相关的扩展请继承<see cref="CombatUnit"/>
    /// </summary>
    public interface ICombatUnit
    {
        /// <summary>
        /// 战斗唯一身份识别Id
        /// </summary>
        int InstanceId { get; }
        string Name { get; }
        ICombatCondition Hp { get; }
        int GetDamage();
        int GetSpeed();
        int TeamId { get; }
        bool IsDead => Hp.IsExhausted;
        bool IsAlive => !IsDead;
        double HpRatio => Hp.ValueMaxRatio;
        /// <summary>
        /// 设定身份Id, 用于标识战斗单位
        /// </summary>
        /// <param name="instanceId"></param>
        void SetInstanceId(int instanceId);
    }
    /** by leo@icefoxz
 * 回合制战斗框架:
 * 1.生成BuffManager作为Buff全局管理器
 * 2.生成CombatUnit作为战斗单位
 * 3.生成Round执行Execute可获取RoundInfo, 并且执行了回合制自动战斗.
 *
 * 其它:
 * CombatBehaviour : 作为战斗行为的类.每个战斗单位的轮次中会执行一次.
 *      目前写了AttackBehavior执行攻击.可扩展执行补血,赋buff等行为.调用规则交给Round.ChooseBehaviour执行.
 * BuffManager : 主要管理buff生成与释放, 并且在Round的开始,结束以及每个单位轮次中会调用.
 * CombatPerformInfo : 记录战斗每次行为执行的信息, 注意是一个轮次中, 一个单位(同时一个行为)可能产出多次执行(例如:攻击多目标)
 * CombatResponseInfo : 战斗执行反馈信息, 在每次执行后, 对方会产出执行后的反馈信息.
 * CombatUnitInfo : 战斗单位信息, 主要是每次的执行都可能产出不一样的战斗单位状态,
 *      同一个单位有可能多次被攻击, 所以用这个可以更详细的记录到每次执行后的状态.
 */

//战斗单位的属性管理类, 主要为了可以更好的扩展
    public abstract class CombatantBase
    {
        private readonly List<Attribute> _attributes = new List<Attribute>();
        private readonly List<Condition> _conditions = new List<Condition>();

        public IReadOnlyList<ICombatAttribute> Attributes => _attributes;
        public IReadOnlyList<ICombatCondition> Conditions => _conditions;
        protected int GetAttributeValue(string name) => GetAttribute(name).Value;
        protected int GetAttributeBase(string name) => GetAttribute(name).Base;
        protected int GetConditionValue(string name) => GetCondition(name).Value;
        protected int GetConditionMax(string name) => GetCondition(name).Max;
        protected int GetConditionBase(string name) => GetCondition(name).Base;

        protected void AddAttribute(string name, int value, int @base = -1) => _attributes.Add(new Attribute(name, value, @base == -1 ? value : @base));
        protected void AddCondition(string name, int value, int max = -1, int @base = -1)
        {
            if(max == -1) max = value;
            _conditions.Add(new Condition(name, value, max, @base == -1 ? max : @base));
        }

        protected ICombatAttribute GetAttribute(string name) => _attributes.Single(a => a.Name == name);
        protected ICombatCondition GetCondition(string name) => _conditions.Single(a => a.Name == name);

        private class Attribute : ICombatAttribute
        {
            public Attribute(string name, int value, int @base)
            {
                Name = name;
                Value = value;
                Base = @base;
            }
            public Attribute(string name, int value)
            {
                Name = name;
                Value = value;
                Base = value;
            }

            public string Name { get; }
            public int Value { get; private set; }
            public int Base { get; }

            public void Add(int value)
            {
                Value += value;
            }

            public void Set(int value)
            {
                Value = value;
            }

            public override string ToString() => $"{Name}:{Value}({Base})";
        }
        private class Condition : ConValue, ICombatCondition
        {
            public string Name { get; }

            public Condition(string name, int value, int max, int @base) : base(@base, max, value)
            {
                Name = name;
            }
        }
    }
    /// <summary>
    /// 'ICombatAttribute' 是战斗属性的接口。属性表示战斗者的某种特性，如生命值、攻击力等。
    /// </summary>
    public interface ICombatAttribute
    {
        string Name { get; }
        int Value { get; }
        int Base { get; }
        void Add(int value);
        void Set(int value);
    }
    /// <summary>
    /// 战斗单位,
    /// </summary>
    public abstract class CombatUnit : CombatantBase, ICombatUnit
    {
        public int InstanceId { get; private set; }
        public string Name { get; protected set; }
        //血量
        public ICombatCondition Hp => GetCondition(Combat.Hp);
        //队伍
        public int TeamId { get; }

        protected CombatUnit(ICombatUnit u) : this(teamId: u.TeamId, name: u.Name, u.Hp.Value, u.Hp.Max, u.Hp.Base)
        {
        }

        protected CombatUnit(int teamId, string name, int hp, int maxHp = -1, int baseHp = -1)
        {
            if (maxHp == -1)
                maxHp = hp;
            if (baseHp == -1)
                baseHp = maxHp;
            Name = name;
            TeamId = teamId;
            AddCondition(Combat.Hp, hp, maxHp, baseHp);
        }

        //伤害
        public abstract int GetDamage();
        //速度
        public abstract int GetSpeed();

        public void SetInstanceId(int instanceId) => InstanceId = instanceId;

        public int AddHp(int hp)
        {
            if (hp == 0) return Hp.Value;
            var clampValue = Math.Clamp(Hp.Value + hp, 0, Hp.Max);
            Hp.Set(clampValue);
            return Hp.Value;
        }

        //是否阵亡
        public bool IsDead => Hp.IsExhausted;
        //是否还活着
        public bool IsAlive => !IsDead;
        //血量比率
        public double HpRatio => GetCondition(Combat.Hp).ValueBaseRatio;
    }
//回合记录器
    public class RoundInfo<TUnit, TInfo, TUnitInfo> where TUnit : ICombatUnit
        where TInfo : CombatPerformInfo<TUnit, TUnitInfo>
        where TUnitInfo : ICombatUnitInfo<TUnit>, new()
    {
        private readonly Dictionary<TUnit, List<TInfo>> _infoMap;

        public int RoundCount { get; private set; } //回合数

        //每个单位在每个回合的战斗活动
        public IReadOnlyDictionary<TUnit, List<TInfo>> UnitInfoMap => _infoMap;

        public RoundInfo()
        {
            _infoMap = new Dictionary<TUnit, List<TInfo>>();
        }

        public void SetRound(int round)
        {
            RoundCount = round;
        }
        public void AddUnitActionInfo(TUnit unit, TInfo performInfo)
        {
            if (!UnitInfoMap.ContainsKey(key: unit))
            {
                _infoMap[key: unit] = new List<TInfo>();
            }

            _infoMap[key: unit].Add(item: performInfo);
        }
    }

//战斗回合
    public class Round<TUnit,TRoundInfo ,TPerformInfo, TUnitInfo, TBuff> 
        where TUnit : CombatUnit
        where TRoundInfo : RoundInfo<TUnit, TPerformInfo, TUnitInfo>, new()
        where TPerformInfo : CombatPerformInfo<TUnit, TUnitInfo>, new()
        where TUnitInfo : ICombatUnitInfo<TUnit>, new()
        where TBuff : Buff<TUnit>
    {
        private readonly List<TUnit> _combatUnits;
        public int RoundIndex { get; }
        //所有战斗单位,不可移除
        public IReadOnlyList<TUnit> CombatUnits => _combatUnits;

        //buff管理器
        public IBuffHandler<TUnit> BuffHandler { get; }

        public Round(List<TUnit> combatUnits, IBuffHandler<TUnit> buffHandler, int roundIndex)
        {
            _combatUnits = combatUnits;
            BuffHandler = buffHandler;
            RoundIndex = roundIndex;
        }
        //执行回合函数前的前置逻辑, 一般上用来上buff
        protected virtual void BeforeRoundExecute(TUnit[] sortedAliveCombatUnits){}
        //执行回合
        public TRoundInfo Execute()
        {
            var info = new TRoundInfo();
            info.SetRound(RoundIndex);
            var sortedCombatUnits = CombatUnits.Where(c => c.IsAlive).OrderByDescending(keySelector: x => x.GetSpeed()).ToArray();
            BeforeRoundExecute(sortedCombatUnits);
            BuffHandler.OnRoundStart(); //开始回合buff执行
            foreach (var combatUnit in sortedCombatUnits)
            {
                if (combatUnit.IsDead) break; //如果单位已阵亡
                BuffHandler.OnCombatStart(); //当开始战斗时buff调用
                var combatBehavior = ChooseBehavior(unit: combatUnit); //选择战斗行为
                var targets = combatBehavior.ChooseTargets(caster: combatUnit, combatUnits: _combatUnits); //选择执行目标
                var performInfos = combatBehavior.Execute(caster: combatUnit, targets: targets); //执行战斗
                //记录战斗信息
                foreach (var performInfo in performInfos)
                    info.AddUnitActionInfo(unit: combatUnit, performInfo: performInfo);
            }

            BuffHandler.OnRoundEnd(); //结束回合buff执行
            return info;
        }

        protected virtual CombatBehavior<TUnit, TPerformInfo, TUnitInfo> ChooseBehavior(TUnit unit)
        {
            // 根据当前状态和其他因素选择适合的CombatBehavior
            return new AttackBehavior<TUnit, TPerformInfo, TUnitInfo>();
        }
    }

    /// <summary>
    /// 战斗行为
    /// </summary>
    public abstract class CombatBehavior<TUnit, TInfo, TUnitInfo> where TUnit : CombatUnit
        where TInfo : CombatPerformInfo<TUnit, TUnitInfo>, new()
        where TUnitInfo : ICombatUnitInfo<TUnit>, new()
    {
        protected static Random Random { get; } = new Random();
        public abstract TInfo[] Execute(TUnit caster, TUnit[] targets);

        public abstract TUnit[] ChooseTargets(TUnit caster, List<TUnit> combatUnits);
        protected double GetRandom() => Random.NextDouble();
    }

    /// <summary>
    /// 常规攻击行为, 支持反击,连击,并且连击中可反击, 并且反击或是连击都是独立生成闪避率
    /// </summary>
    public class AttackBehavior<TUnit,TInfo,TUnitInfo> : CombatBehavior<TUnit, TInfo, TUnitInfo> 
        where TUnit : CombatUnit 
        where TInfo : CombatPerformInfo<TUnit,TUnitInfo>, new() 
        where TUnitInfo : ICombatUnitInfo<TUnit>,new()
    {
        private float CriticalMultiplier => 1.5f;
        private int Combos { get; set; } = 1;//注意, 如果设成0会不攻击而一直没有单位离开

        public override TInfo[] Execute(TUnit caster, TUnit[] targets)
        {
            var infos = new List<TInfo>();
            var combos = Combos; //获取连击信息
            for (var i = 0; i < combos; i++) //连击次数
            {
                foreach (var target in targets) //对所有执行目标历遍执行
                {
                    var (damage, isDodge, isCritical) = GetDamage(caster: caster, target: target); //获取伤害
                    var finalDamage = (int)damage;

                    //生成行动记录
                    var perform = new TInfo();
                    perform.Set(caster);

                    //添加入行动列表
                    infos.Add(item: perform);
                    //设定战斗行动
                    perform.SetCritical(isCritical: isCritical);
                    //执行伤害
                    target.AddHp(-finalDamage);
                    var isCounter = target.IsAlive && Random.Next(0, 100) < 10; //10%反击
                    if (isCounter) CounterAttack(caster: target, target: caster, perform: perform); //反击执行
                    //生成反馈记录
                    var response = new CombatResponseInfo<TUnit, TUnitInfo>();
                    response.Set(target);

                    //添加反馈记录
                    perform.AddResponse(info: response);
                    //记录伤害
                    response.RegDamage(damage: finalDamage, isDodge);

                }
            }
            return infos.ToArray();
        }

        //反击
        private void CounterAttack(TUnit caster, TUnit target, CombatPerformInfo<TUnit, TUnitInfo> perform)
        {
            var (damage, isDodge, isCritical) = GetDamage(caster, target);
            if (!isDodge) target.AddHp((int)-damage);
            perform.SetCounterAttack(damage: caster.GetDamage(), isDodged: isDodge, isCritical); //记录反击伤害
        }

        private (float damage, bool isDodge, bool isCritical) GetDamage(TUnit caster, TUnit target)
        {
            var isDodge = Random.Next(0, 100) < 15;//15%闪避
            var isCritical = Random.Next(0, 100) < 15;//15%暴击
            var damage = isDodge ? 0
                : isCritical ? caster.GetDamage() * CriticalMultiplier
                : caster.GetDamage();
            return (damage, isDodge, isCritical);
        }

        public override TUnit[] ChooseTargets(TUnit caster, List<TUnit> combatUnits)
        {
            var target = combatUnits.Where(predicate: c => c.TeamId != caster.TeamId)
                .Where(c => c.IsAlive)
                .OrderBy(keySelector: c => c.HpRatio)
                .FirstOrDefault();
            return target == null ? Array.Empty<TUnit>() : new TUnit[] { target };
        }
    }

    /// <summary>
    /// 战斗执行信息, 记录了战斗行为以及对手的反馈.
    /// 目前仅仅是记录攻击方式, 包括暴击, 连击
    /// </summary>
    public record CombatPerformInfo<TUnit, TUnitInfo>
        where TUnit : ICombatUnit where TUnitInfo : ICombatUnitInfo<TUnit>, new()
    {
        public TUnitInfo Performer { get; set; }

        public CombatResponseInfo<TUnit, TUnitInfo> Response { get; set; }

        //是否会心
        public bool IsCritical { get; set; }

        //补血(吸血)
        public int Heal { get; set; }

        //是否反击
        public bool IsCounterAttack { get; set; }

        //是否反击暴击
        public bool IsCounterAttackCritical { get; set; }

        //反击伤害
        public int CounterAttackDamage { get; set; }

        //反击被闪避
        public bool IsCounterAttackDodged { get; set; }

        public void Set(TUnit unit)
        {
            Performer = new TUnitInfo();
            Performer.Set(unit);
        }

        public void SetCritical(bool isCritical) => IsCritical = isCritical;

        public void AddResponse(CombatResponseInfo<TUnit, TUnitInfo> info) => Response = info;

        public void SetCounterAttack(int damage, bool isDodged, bool isCritical)
        {
            IsCounterAttack = true;
            CounterAttackDamage = damage;
            IsCounterAttackDodged = isDodged;
            IsCounterAttackCritical = isCritical;
        }

        public void SetHeal(int heal) => Heal = heal;

        protected virtual StringBuilder Description(StringBuilder sb)
        {
            if (IsCritical) sb.Append(value: "会心一击,");
            if (Heal != 0) sb.Append(value: $"治疗{Heal}点,");
            if (IsCounterAttack) sb.Append(value: "反击,");
            if (IsCounterAttackCritical) sb.Append(value: "暴击,");
            if (CounterAttackDamage != 0) sb.Append(value: $"反击造成({CounterAttackDamage})点伤害,");
            if (IsCounterAttackDodged) sb.Append(value: "反击被闪避,");
            return sb;
        }

        public override string ToString() => Description(new StringBuilder()).ToString();
    }

    public interface ICombatUnitInfo<in TUnit> where TUnit : ICombatUnit
    {
        void Set(TUnit unit);
    }
//战斗单位状态
    public record CombatUnitInfo<TUnit> : ICombatUnitInfo<TUnit> where TUnit : ICombatUnit
    {
        public int InstanceId { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int MaxHp { get; set; }
        public int Damage { get; set; }
        public int Speed { get; set; }
        public int TeamId { get; set; }

        public virtual void Set(TUnit unit)
        {
            InstanceId = unit.InstanceId;
            Name = unit.Name;
            Hp = unit.Hp.Value;
            MaxHp = unit.Hp.Max;
            Damage = unit.GetDamage();
            Speed = unit.GetSpeed();
            TeamId = unit.TeamId;
        }

        public bool IsDead => Hp <= 0;
        public bool IsAlive => !IsDead;
        public float HpRatio => 1f * Hp / MaxHp;

        protected virtual StringBuilder Description(StringBuilder sb)
        {
            sb.Append(value: $"[{InstanceId}]{Name},");
            sb.Append(value: $"Hp:{Hp}/{MaxHp},");
            sb.Append(value: $"Dmg:{Damage},");
            sb.Append(value: $"Spd:{Speed},");
            return sb;
        }

        public override string ToString()=> Description(new StringBuilder()).ToString();
    }
    /// <summary>
    /// 战斗反馈信息, 记录被攻击单位的反馈信息.
    /// 主要标记是否闪避, 反击, 其中包括反击被(攻击方)闪避, 最终伤害以及补血信息
    /// </summary>
    public record CombatResponseInfo<TUnit,TUnitInfo> where TUnit : ICombatUnit where TUnitInfo : ICombatUnitInfo<TUnit>, new()
    {
        public TUnitInfo Target { get; set; }
        //最终伤害
        public int FinalDamage { get; set; }
        //最终补血
        public int FinalHeal { get; set; }
        //攻击闪避
        public bool IsDodge { get; set; }

        public void Set(TUnit unit)
        {
            Target = new TUnitInfo();
            Target.Set(unit);
        }

        public void RegDamage(int damage, bool isDodge)
        {
            FinalDamage = damage;
            IsDodge = isDodge;
        }

        public void SetHeal(int heal) => FinalHeal = heal;

        protected virtual StringBuilder Description(StringBuilder sb)
        {
            if (FinalDamage != 0) sb.Append(value: $"受到({FinalDamage})点伤害,");
            if (IsDodge) sb.Append(value: "闪避,");
            if (FinalHeal != 0) sb.Append(value: $"治疗({FinalHeal})点,");
            return sb;
        }
        public override string ToString() => Description(new StringBuilder()).ToString();
    }

    public interface IBuffHandler<TUnit> where TUnit : ICombatUnit
    {
        int GetBuffInstanceId();
        void RemoveBuff(IBuff<TUnit> buff);
        void AddBuff(IBuff<TUnit> buff);
        void OnRoundStart();
        void OnCombatStart();
        void OnRoundEnd();
    }

//全局Buff管理器, 处理历遍统一调用回合开始,回合结束等方法
    public class BuffManager<TUnit> : IBuffHandler<TUnit> where TUnit : ICombatUnit
    {
        private List<IBuff<TUnit>> Buffs { get; }
        private List<TUnit> AllUnits { get; }
        public BuffManager(List<TUnit> allUnits)
        {
            AllUnits = allUnits;
            Buffs = new List<IBuff<TUnit>>();
        }
        //buff Id 种子
        private int BuffInstanceSeed { get; set; }
        public int GetBuffInstanceId() => ++BuffInstanceSeed;

        // 添加buff, 确保调用入口只有一个
        public void AddBuff(IBuff<TUnit> buff)
        {
            var b = Buffs.SingleOrDefault(m => m.InstanceId == buff.InstanceId);
            if (b == null) Buffs.Add(buff);
        }

        // 移除buff, 确保调用入口只有一个
        public void RemoveBuff(IBuff<TUnit> buff)
        {
            var b = Buffs.SingleOrDefault(b => b.InstanceId == buff.InstanceId);
            if (b == null)
                throw new NullReferenceException($"buff[{buff.InstanceId}]!");
            Buffs.Remove(b);
        }

        public IReadOnlyList<IBuff<TUnit>> GetBuffs(TUnit caster) => Buffs.Where(predicate: b => b.Caster.Equals(caster)).ToList();

        /// <summary>
        /// 每回合开始时调用, 历遍所有buff, 调用OnRoundStart
        /// </summary>
        public void OnRoundStart()
        {
            foreach (var buff in Buffs.ToArray()) buff.OnRoundStartTrigger(AllUnits);
        }
        /// <summary>
        /// 每回合结束时调用, 历遍所有buff, 调用OnRoundEnd
        /// </summary>
        public void OnRoundEnd()
        {
            foreach (var buff in Buffs.ToArray()) buff.OnRoundEndTrigger(AllUnits);
        }
        /// <summary>
        /// 战斗开始时调用, 历遍所有buff, 调用OnCombatStart
        /// </summary>
        public void OnCombatStart()
        {
            foreach (var map in Buffs.ToArray()) map.OnCombatStartTrigger(AllUnits);
        }
    }

    public interface IBuff<TUnit> where TUnit : ICombatUnit
    {
        //buff唯一Id
        int InstanceId { get; }
        //buff名称(识别类型)
        string TypeId { get; }
        //发动者
        TUnit Caster { get; }
        //目标
        TUnit Target { get; }
        /// <summary>
        /// buff持续回合数,默认为1,一般上每回合减1,为0时移除buff. 当然也有不递减的buff, 而只要被标记为0的时候就会被移除.
        /// </summary>
        int Lasting { get; }
        void OnRoundStartTrigger(List<TUnit> units);
        void OnRoundEndTrigger(List<TUnit> units);
        void OnCombatStartTrigger(List<TUnit> units);
        void AddLasting(int value = 1);
        void SubLasting(int value = 1);
        /// <summary>
        /// 赋buff的入口
        /// </summary>
        /// <param name="target"></param>
        void Apply(TUnit target);
    }

    public abstract class Buff<TUnit> : IBuff<TUnit> where TUnit : ICombatUnit
    {
        private IBuffHandler<TUnit> BuffHandlerBase { get; }
        //buff唯一Id
        public int InstanceId { get; }
        //buff名称(识别类型)
        public abstract string TypeId { get; }
        //发动者
        public TUnit Caster { get; }
        //目标
        public TUnit Target { get; private set; }

        /// <summary>
        /// buff持续回合数,默认为1,一般上每回合减1,为0时移除buff. 当然也有不递减的buff, 而只要被标记为0的时候就会被移除.
        /// </summary>
        public int Lasting { get; private set; } = 1;

        protected Buff(TUnit caster, IBuffHandler<TUnit> buffHandler, int lasting = 1)
        {
            BuffHandlerBase = buffHandler;
            Caster = caster;
            InstanceId = buffHandler.GetBuffInstanceId();
            Lasting = lasting;
        }

        public abstract void OnRoundStartTrigger(List<TUnit> units);

        public abstract void OnRoundEndTrigger(List<TUnit> units);

        public abstract void OnCombatStartTrigger(List<TUnit> units);

        /// <summary>
        /// <see cref="Lasting"/>加1
        /// </summary>
        public void AddLasting(int value = 1)
        {
            Lasting = Math.Max(Lasting + value, 0);
        }

        /// <summary>
        /// <see cref="Lasting"/>减1,<see cref="Lasting"/>为0时移除buff
        /// </summary>
        public void SubLasting(int value = 1)
        {
            Lasting = Math.Max(Lasting - value, 0);
            if (Lasting <= 0)
            {
                BuffHandlerBase.RemoveBuff(this);
                OnRemoveBuff();
            }
        }

        protected virtual void OnRemoveBuff()
        {
        }

        /// <summary>
        /// 赋buff的入口
        /// </summary>
        /// <param name="target"></param>
        public void Apply(TUnit target)
        {
            Target = target;
            BuffHandlerBase.AddBuff(this);
            OnApplyBuff();
        }

        protected virtual void OnApplyBuff(){}
    }
}