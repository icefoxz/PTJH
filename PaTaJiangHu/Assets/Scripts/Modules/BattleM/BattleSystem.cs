using System;
using System.Collections.Generic;
using System.Linq;


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
    int Hp { get; }
    int MaxHp { get; }
    int Damage { get; }
    int Speed { get; }
    int TeamId { get; }
    bool IsDead => Hp <= 0;
    bool IsAlive => !IsDead;
    float HpRatio => 1f * Hp / MaxHp;
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

/// <summary>
/// 战斗单位,
/// </summary>
public class CombatUnit : ICombatUnit
{
    public int InstanceId { get; private set; }
    public virtual string Name { get; protected set; }
    //血量
    public virtual int Hp { get; protected set; }
    //最大血量
    public virtual int MaxHp { get; protected set; }
    //伤害
    public virtual int Damage { get; protected set; }
    //速度
    public virtual int Speed { get; protected set; }
    //队伍
    public int TeamId { get; }

    protected CombatUnit(int teamId)
    {
        TeamId = teamId;
    }

    public CombatUnit(ICombatUnit u)
    {
        Name = u.Name;
        Hp = u.Hp;
        MaxHp = u.MaxHp;
        Damage = u.Damage;
        Speed = u.Speed;
        TeamId = u.TeamId;
    }
    public CombatUnit(int teamId, string name, int maxHp, int damage, int speed)
    {
        Name = name;
        MaxHp = maxHp;
        Hp = maxHp;
        Damage = damage;
        Speed = speed;
        TeamId = teamId;
    }

    public void SetInstanceId(int instanceId) => InstanceId = instanceId;

    public int TakeDamage(int damage)
    {
        var finalDamage = DamageReduction(damage);
        Hp -= finalDamage;
        if (Hp < 0)
        {
            Hp = 0;
        }
        return finalDamage;
    }

    /// <summary>
    /// 伤害减免
    /// </summary>
    /// <param name="damage"></param>
    /// <returns></returns>
    protected virtual int DamageReduction(int damage)
    {
        return damage;
    }

    //是否阵亡
    public bool IsDead => Hp <= 0;
    //是否还活着
    public bool IsAlive => !IsDead;
    //血量比率
    public float HpRatio => 1f * Hp / MaxHp;
    //补血
    public void Heal(int amount)
    {
        Hp += amount;
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }
}
//回合记录器
public class RoundInfo<TUnit, TInfo, TUnitInfo> where TUnit : ICombatUnit
    where TInfo : CombatPerformInfo<TUnit, TUnitInfo>
    where TUnitInfo : ICombatUnitInfo<TUnit>, new()
{
    private readonly Dictionary<TUnit, List<TInfo>> _infoMap;

    //每个单位在每个回合的战斗活动
    public IReadOnlyDictionary<TUnit, List<TInfo>> UnitInfoMap => _infoMap;

    public RoundInfo()
    {
        _infoMap = new Dictionary<TUnit, List<TInfo>>();
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
public class Round<TUnit,TRoundInfo ,TPerformInfo, TUnitInfo> 
    where TUnit : CombatUnit
    where TRoundInfo : RoundInfo<TUnit, TPerformInfo, TUnitInfo>, new()
    where TPerformInfo : CombatPerformInfo<TUnit, TUnitInfo>, new()
    where TUnitInfo : ICombatUnitInfo<TUnit>, new()
{
    private readonly List<TUnit> _combatUnits;

    //所有战斗单位,不可移除
    public IReadOnlyList<TUnit> CombatUnits => _combatUnits;

    //buff管理器
    public BuffManager<TUnit> BuffManager { get; }

    public Round(List<TUnit> combatUnits, BuffManager<TUnit> buffManager)
    {
        _combatUnits = combatUnits;
        BuffManager = buffManager;
    }

    //执行回合
    public TRoundInfo Execute()
    {
        var info = new TRoundInfo();
        var sortedCombatUnits = CombatUnits.Where(c => c.IsAlive).OrderByDescending(keySelector: x => x.Speed);
        BuffManager.OnRoundStart(); //开始回合buff执行
        foreach (var combatUnit in sortedCombatUnits)
        {
            if (combatUnit.IsDead) break; //如果单位已阵亡
            BuffManager.OnCombatStart(combatUnit); //当开始战斗时buff调用
            var combatBehavior = ChooseBehavior(unit: combatUnit); //选择战斗行为
            var targets = combatBehavior.ChooseTargets(caster: combatUnit, combatUnits: _combatUnits); //选择执行目标
            var performInfos = combatBehavior.Execute(caster: combatUnit, targets: targets); //执行战斗
            //记录战斗信息
            foreach (var performInfo in performInfos)
                info.AddUnitActionInfo(unit: combatUnit, performInfo: performInfo);
        }

        BuffManager.OnRoundEnd(); //结束回合buff执行
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
                target.TakeDamage(damage: finalDamage);
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
        if (!isDodge) target.TakeDamage(damage: (int)damage);
        perform.SetCounterAttack(damage: caster.Damage, isDodged: isDodge, isCritical); //记录反击伤害
    }

    private (float damage, bool isDodge, bool isCritical) GetDamage(TUnit caster, TUnit target)
    {
        var isDodge = Random.Next(0, 100) < 15;//15%闪避
        var isCritical = Random.Next(0, 100) < 15;//15%暴击
        var damage = isDodge ? 0
            : isCritical ? caster.Damage * CriticalMultiplier
            : caster.Damage;
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
public record CombatPerformInfo<TUnit,TUnitInfo> where TUnit : ICombatUnit where TUnitInfo : ICombatUnitInfo<TUnit>, new()
{
    public TUnitInfo Performer { get; set; }
    public CombatResponseInfo<TUnit,TUnitInfo> Response { get; set; }
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

    public void AddResponse(CombatResponseInfo<TUnit,TUnitInfo> info) => Response = info;

    public void SetCounterAttack(int damage, bool isDodged, bool isCritical)
    {
        IsCounterAttack = true;
        CounterAttackDamage = damage;
        IsCounterAttackDodged = isDodged;
        IsCounterAttackCritical = isCritical;
    }

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
        Hp = unit.Hp;
        MaxHp = unit.MaxHp;
        Damage = unit.Damage;
        Speed = unit.Speed;
        TeamId = unit.TeamId;
    }

    public bool IsDead => Hp <= 0;
    public bool IsAlive => !IsDead;
    public float HpRatio => 1f * Hp / MaxHp;
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
    public bool IsDodged { get; set; }

    public void Set(TUnit unit)
    {
        Target = new TUnitInfo();
        Target.Set(unit);
    }

    public void RegDamage(int damage, bool isDodge)
    {
        FinalDamage = damage;
        IsDodged = isDodge;
    }

    public void SetHeal(int heal) => FinalHeal = heal;
}
//全局Buff管理器
public class BuffManager<TUnit> where TUnit : ICombatUnit
{
    private List<BuffCombatMapper> BuffMappers { get; }
    private List<TUnit> AllUnits { get; }
    public BuffManager(List<TUnit> allUnits)
    {
        AllUnits = allUnits;
        BuffMappers = new List<BuffCombatMapper>();
    }
    //buff Id 种子
    private int BuffInstanceSeed { get; set; }
    public int GetBuffInstanceId() => ++BuffInstanceSeed;

    public void AddBuff(Buff<TUnit> buff, TUnit owner)
    {
        var mapper = BuffMappers.SingleOrDefault(b => b.Buff.InstanceId == buff.InstanceId);
        if (mapper == null)
        {
            mapper = new BuffCombatMapper(buff, owner);
            BuffMappers.Add(mapper);
            return; //不增加Stack,因为buff本身生成的时候就包涵Stack了
        }

        mapper.Buff.AddStack();
    }

    public void RemoveBuff(Buff<TUnit> buff)
    {
        var mapper = BuffMappers.SingleOrDefault(b => b.Buff.InstanceId == buff.InstanceId);
        if (mapper == null)
            throw new NullReferenceException($"buff[{buff.InstanceId}]!");
        BuffMappers.Remove(mapper);
    }

    public IReadOnlyList<Buff<TUnit>> GetBuffs(TUnit owner) => BuffMappers.Where(predicate: b => b.Owner.Equals(owner)).Select(b => b.Buff).ToList();

    public IReadOnlyList<Buff<TUnit>> GetAllBuffs() => BuffMappers.Select(b => b.Buff).ToList();

    public void OnRoundStart()
    {
        foreach (var map in BuffMappers.ToArray()) map.Buff.OnRoundStart(map.Owner, AllUnits);
    }
    public void OnRoundEnd()
    {
        foreach (var map in BuffMappers.ToArray()) map.Buff.OnRoundEnd(map.Owner, AllUnits);
    }
    public void OnCombatStart(TUnit unit)
    {
        foreach (var map in BuffMappers.Where(b => b.Owner.Equals(unit)).ToArray()) map.Buff.OnCombatStart(unit, AllUnits);
    }
    //buff与战斗单位的映射关系
    private class BuffCombatMapper : IEquatable<BuffCombatMapper>
    {
        public Buff<TUnit> Buff { get; }
        public TUnit Owner { get; }

        public BuffCombatMapper(Buff<TUnit> buff, TUnit owner)
        {
            Buff = buff;
            Owner = owner;
        }

        #region Equals
        public bool Equals(BuffCombatMapper? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Buff.InstanceId == other.Buff.InstanceId;
        }
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BuffCombatMapper)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return Buff.InstanceId.GetHashCode() * 397;
            }
        }
        public static bool operator ==(BuffCombatMapper? left, BuffCombatMapper? right) => Equals(left, right);
        public static bool operator !=(BuffCombatMapper? left, BuffCombatMapper? right) => !Equals(left, right);
        #endregion
    }
}
public abstract class Buff<TUnit> where TUnit : ICombatUnit
{
    protected BuffManager<TUnit> _buffManager;
    public int Stacks { get; private set; }
    //buff唯一Id
    public int InstanceId { get; }

    public Buff(BuffManager<TUnit> buffManager, int stacks = 1)
    {
        _buffManager = buffManager;
        InstanceId = buffManager.GetBuffInstanceId();
        Stacks = stacks;
    }

    public abstract void OnRoundStart(TUnit owner, List<TUnit> units);

    public abstract void OnRoundEnd(TUnit owner, List<TUnit> units);

    public abstract void OnCombatStart(TUnit owner, List<TUnit> units);

    public void AddStack()
    {
        Stacks++;
    }

    public void RemoveStack()
    {
        Stacks--;
        if (Stacks <= 0)
        {
            _buffManager.RemoveBuff(this);
        }
    }
}