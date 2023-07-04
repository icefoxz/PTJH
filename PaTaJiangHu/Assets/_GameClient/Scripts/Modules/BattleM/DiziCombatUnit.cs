using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using DiziM;
using Models;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Skills;
using Utls;

public interface IDiziCombatUnit : ICombatUnit
{
    string Guid { get; }
    ICombatCondition Mp { get; }
    ICombatAttribute Strength { get; }
    ICombatAttribute Agility { get; }
    ICombatSet GetCombatSet();
    IDiziEquipment Equipment { get; }
    ICombatGifted Gifted { get; }
    ICombatArmedAptitude ArmedAptitude { get; }
    ISkillMap<ISkillInfo> ForceInfo { get; }
    ISkillMap<ICombatSkillInfo> CombatInfo { get; }
    ISkillMap<ISkillInfo> DodgeInfo { get; }
}

/// <summary>
/// 弟子战斗单位
/// </summary>
public class DiziCombatUnit : CombatUnit, IDiziCombatUnit
{
    private DiziEquipment _equipment;
    private ICombatSet _skillCombatSet;
    private readonly List<CombatBuff> _buffs = new List<CombatBuff>();
    public IReadOnlyList<CombatBuff> Buffs => _buffs;

    public string Guid { get; private set; }
    public event Action<int> DiziDisarmed;

    public ICombatAttribute Strength => GetAttribute(Combat.Str);
    public ICombatAttribute Agility => GetAttribute(Combat.Agi);
    public ICombatCondition Mp => GetCondition(Combat.Mp);
    public ICombatSet GetCombatSet() => new[] { _skillCombatSet, Equipment.GetCombatSet() }.Combine();

    public IDiziEquipment Equipment => _equipment;
    public ICombatGifted Gifted { get; }
    public WeaponArmed Armed => Equipment.Weapon?.Armed ?? WeaponArmed.Unarmed;
    public ICombatArmedAptitude ArmedAptitude { get; private set; }
    public ISkillMap<ISkillInfo> ForceInfo { get; }
    public ISkillMap<ICombatSkillInfo> CombatInfo { get; }
    public ISkillMap<ISkillInfo> DodgeInfo { get; }

    public override int GetDamage() => (int)((1 + ArmedAptitude?.GetDamageRatio(Armed) ?? 0) * Strength.Value);
    public override int GetSpeed() => Agility.Value;

    public DiziCombatUnit(string guid, int teamId, string name, int strength, int agility, int hp, int mp,
        ICombatSet set, 
        ISkillMap<ISkillInfo> forceInfo, 
        ISkillMap<ICombatSkillInfo> combatInfo, 
        ISkillMap<ISkillInfo> dodgeInfo, 
        IDiziEquipment equipment = null, 
        ICombatGifted gifted = null,
        ICombatArmedAptitude aptitude = null)
        : base(teamId: teamId, name: name, hp: hp)
    {
        Guid = guid;
        AddAttribute(Combat.Str, strength);
        AddAttribute(Combat.Agi, agility);
        AddCondition(Combat.Mp, mp);
        _skillCombatSet = set;
        ForceInfo = forceInfo;
        CombatInfo = combatInfo;
        DodgeInfo = dodgeInfo;
        _equipment = equipment == null ? new DiziEquipment() : new DiziEquipment(equipment);
        ArmedAptitude = aptitude == null ? new CombatArmedAptitude() : new CombatArmedAptitude(aptitude);
        Gifted = gifted ?? CombatGifted.Empty;
        UpdateCalculate();
        Full();
    }

    public DiziCombatUnit(int teamId, Dizi dizi) : base(teamId, dizi.Name, dizi.Hp)
    {
        ForceInfo = dizi.Skill.Force;
        CombatInfo = dizi.Skill.Combat;
        DodgeInfo = dizi.Skill.Dodge;
        Guid = dizi.Guid;
        AddAttribute(Combat.Str, dizi.Strength);
        AddAttribute(Combat.Agi, dizi.Agility);
        AddCondition(Combat.Mp, dizi.Mp);
        _skillCombatSet = dizi.GetCombatSet();
        _equipment = new DiziEquipment(dizi.Equipment);
        ArmedAptitude = new CombatArmedAptitude(dizi.ArmedAptitude);
        Gifted = dizi.Gifted;
        UpdateCalculate();
        Full();
    }

    internal DiziCombatUnit(int teamId, CombatNpcSo npc, string guid = "Npc") : base(teamId, npc.name, npc.Hp)
    {
        Guid = guid;
        ForceInfo = npc.GetForceSkillinfo();
        CombatInfo = npc.GetCombatSkillInfo();
        DodgeInfo = npc.GetDodgeSkillInfo();
        AddAttribute(Combat.Str, npc.Strength);
        AddAttribute(Combat.Agi, npc.Agility);
        AddCondition(Combat.Mp, npc.Mp);
        _skillCombatSet = npc.GetCombatSet();
        _equipment = new DiziEquipment(npc.Equipment);
        ArmedAptitude = new CombatArmedAptitude(); //npc没有武学天赋
        Gifted = npc.Gifted;
        UpdateCalculate();
        Full();
    }

    public DiziCombatUnit(bool fullCondition, IDiziCombatUnit unit) : base(unit.TeamId, unit.Name, unit.Hp.Value, unit.Hp.Max,
        unit.Hp.Base)
    {
        ForceInfo = unit.ForceInfo;
        CombatInfo = unit.CombatInfo;
        DodgeInfo = unit.DodgeInfo;
        AddAttribute(Combat.Str, unit.Strength.Value, unit.Strength.Base);
        AddAttribute(Combat.Agi, unit.Agility.Value, unit.Agility.Base);
        AddCondition(Combat.Mp, unit.Mp.Value, unit.Mp.Max, unit.Mp.Base);
        _skillCombatSet = unit.GetCombatSet();
        Gifted = unit.Gifted;
        ArmedAptitude = new CombatArmedAptitude(unit.ArmedAptitude);
        _equipment = new DiziEquipment(unit.Equipment);
        UpdateCalculate();
        if (fullCondition) Full();
    }

    // 注意简易战斗是没有提供SkillInfo, 所以读取不到技能信息
    public DiziCombatUnit(ISimCombat s, int teamId) : base(teamId, s.Name, (int)s.Hp, s.MaxHp)
    {
        AddCondition(Combat.Mp, (int)s.Mp);
        AddAttribute(Combat.Str, (int)s.Strength);
        AddAttribute(Combat.Agi, (int)s.Agility);
        _skillCombatSet = CombatSet.Empty;
        _equipment = new DiziEquipment();
        ArmedAptitude = new CombatArmedAptitude();
        Gifted = CombatGifted.Empty;
        UpdateCalculate();
        Full();
    }

    //满状态
    private void Full()
    {
        Hp.Set(Hp.Max);
        Mp.Set(Mp.Max);
    }
    //伤害减免
    public int TakeReductionDamage(int damage,CombatArgs arg)
    {
        var (finalDamage, mpConsume) = CombatFormula.DamageReduction(damage: damage, arg: arg);
        AddMp(mp: -mpConsume);
        AddHp(hp: -finalDamage);
        return finalDamage;
    }

    public override string ToString() =>
        $"{Name}:{Strength},{Agility},血[{Hp.Value}/{Hp.Max}],内[{Mp.Value}/{Mp.Max}]|伤:{GetDamage()},速:{GetSpeed()}";

    public void AddMp(int mp)
    {
        if (mp == 0) return;
        var value = Math.Clamp(value: Mp.Value + mp, min: 0, max: Mp.Max);
        Mp.Set(value);
    }

    public void EquipmentDisarmed(IEquipment equipment)
    {
        _equipment.CombatDisarm(equipment);
        UpdateCalculate();
        DiziDisarmed?.Invoke(InstanceId);
    }

    public void EquipmentArmed(IEquipment equipment)
    {
        _equipment.CombatArm(equipment);
        UpdateCalculate();
    }
    public void BeforeStartRoundUpdate() => UpdateCalculate();
    // 更新状态值, 主要是更新当前buff与装备带来的属性和状态变化
    // 更新状态会调用配置, 所以调用入口尽量控制好更新节奏. 否则会是性能开销
    private void UpdateCalculate()
    {
        var str = (int)(Strength.Base + _equipment.GetPropAddon(DiziProps.Strength) + Buffs.Sum(b => b.GetEffectValue(Combat.Str)));
        Strength.Set(str);
        var agi = (int)(Agility.Base + _equipment.GetPropAddon(DiziProps.Agility) + Buffs.Sum(b => b.GetEffectValue(Combat.Agi)));
        Agility.Set(agi);
        var hp = (int)(Hp.Base + _equipment.GetPropAddon(DiziProps.Hp) + Buffs.Sum(b => b.GetEffectValue(Combat.Hp)));
        Hp.SetMax(hp, false);
        var mp = (int)(Mp.Base + _equipment.GetPropAddon(DiziProps.Mp) + Buffs.Sum(b => b.GetEffectValue(Combat.Mp)));
        Mp.SetMax(mp, false);
    }

    public IEnumerable<CombatBuff> GetSelfBuffs(int round, BuffManager<DiziCombatUnit> buffManager) =>
        _skillCombatSet.GetSelfBuffs(this, round, buffManager);

    public IEnumerable<CombatBuff> GetTargetBuffs(DiziCombatUnit target, CombatArgs args, int round,
        BuffManager<DiziCombatUnit> buffManager) =>
        _skillCombatSet.GetTargetBuffs(target, this, args, round, buffManager);

    public void AddBuff(CombatBuff buff, bool forceUpdate = false)                                    
    {
        _buffs.Add(buff);
        if (forceUpdate) UpdateCalculate();
    }

    public void RemoveBuff(CombatBuff buff, bool forceUpdate = false)
    {
        _buffs.Remove(buff);
        if (forceUpdate) UpdateCalculate();
    }

    /// <summary>
    /// 战斗单位装备类, 处理装备状态
    /// </summary>
    private class DiziEquipment : IDiziEquipment
    {
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }
        public IShoes Shoes { get; private set; }
        public IDecoration Decoration { get; private set; }
        private IEnumerable<IEquipment> AllEquipments =>
            new IEquipment[] { Weapon, Armor, Shoes, Decoration }.Where(e => e != null);

        public DiziEquipment()
        {
            
        }
        public DiziEquipment(IDiziEquipment equipment)
        {
            Weapon = equipment?.Weapon;
            Armor = equipment?.Armor;
            Shoes = equipment?.Shoes;
            Decoration = equipment?.Decoration;
        }
        public float GetPropAddon(DiziProps prop) => (int)AllEquipments.Sum(e => e.GetAddOn(prop));
        /// <summary>
        /// 卸下装备
        /// </summary>
        /// <param name="equipment"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CombatDisarm(IEquipment equipment)
        {
            switch (equipment.EquipKind)
            {
                case EquipKinds.Weapon:
                    Weapon = null;
                    break;
                case EquipKinds.Armor:
                    Armor = null;
                    break;
                case EquipKinds.Shoes:
                    Shoes = null;
                    break;
                case EquipKinds.Decoration:
                    Decoration = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        /// <summary>
        /// 上装备
        /// </summary>
        /// <param name="equipment"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void CombatArm(IEquipment equipment)
        {
            switch (equipment.EquipKind)
            {
                case EquipKinds.Weapon:
                    Weapon = (IWeapon)equipment;
                    break;
                case EquipKinds.Armor:
                    Armor = (IArmor)equipment;
                    break;
                case EquipKinds.Shoes:
                    Shoes = (IShoes)equipment;
                    break;
                case EquipKinds.Decoration:
                    Decoration = (IDecoration)equipment;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public ICombatSet GetCombatSet() => AllEquipments.Select(selector: e => e.GetCombatSet()).Combine();
    }

    private class CombatGifted : ICombatGifted 
    {
        public static readonly ICombatGifted Empty = new CombatGifted();
        public float DodgeRateMax { get; }
        public float CritRateMax { get; }
        public float HardRateMax { get; }
        public float CritDamageRate { get; }
        public float HardDamageRate { get; }
        public float MpDamageRate { get; }
        public float MpArmorRate { get; }

        public CombatGifted(ICombatGifted g)
        {
            DodgeRateMax = g.DodgeRateMax;
            CritRateMax = g.CritRateMax;
            HardRateMax = g.HardRateMax;
            CritDamageRate = g.CritDamageRate;
            HardDamageRate = g.HardDamageRate;
            MpDamageRate = g.MpDamageRate;
            MpArmorRate = g.MpArmorRate;
        }

        private CombatGifted()
        {
            
        }
    }
}

/// <summary>
/// 弟子攻击行为, 处理各种攻击逻辑
/// </summary>
public class DiziAttackBehavior : CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
{
    private int Round { get; }
    private int Combo { get; }
    protected BuffManager<DiziCombatUnit> BuffManager { get; }
    private Config.BattleConfig Config { get; }
    internal DiziAttackBehavior(int round, BuffManager<DiziCombatUnit> buffManager, Config.BattleConfig config, int combo = 1)
    {
        Round = round;
        BuffManager = buffManager;
        Config = config;
        Combo = combo;
    }

    //攻击行为
    public override DiziCombatPerformInfo[] Execute(DiziCombatUnit caster, DiziCombatUnit[] targets) 
    {
        var infos = new List<DiziCombatPerformInfo>();
        var combos = Combo; //获取连击信息
        for (var i = 0; i < combos; i++) //连击次数
        {
            foreach (var target in targets) //对所有执行目标历遍执行
            {
                var arg = new CombatArgs(caster, target, Round);
                var targetBuffs = caster.GetTargetBuffs(target, arg, Round, BuffManager); //获取目标buff
                foreach (var buff in targetBuffs) buff.Apply(target);

                var (dmg, mpConsume, isDodge, isHard, isCritical) = GetDamage(arg); //获取伤害
                var damage = (int)dmg;

                //生成行动记录
                var pfmInfo = new DiziCombatPerformInfo();
                pfmInfo.Set(caster);
                //添加入行动列表
                infos.Add(item: pfmInfo);
                //设定伤害类型为重击
                pfmInfo.SetHard(isHard);
                //设定伤害类型为会心
                pfmInfo.SetCritical(isCritical);

                //执行者消耗mp
                caster.AddMp(-mpConsume);
                //执行伤害
                var finalDamage = target.TakeReductionDamage(damage, arg); //有计算减免的伤害
                var canCounter = CounterJudgment(target);
                if (canCounter) CounterAttack(target, new CombatArgs(target, caster, Round), pfmInfo); //执行反击
                //生成反馈记录
                var response = new CombatResponseInfo<DiziCombatUnit, DiziCombatInfo>();
                response.Set(target);
                if (!isDodge)
                {
                    WeaponCompare(caster, target, pfmInfo.Performer, response.Target); //武器对比
                    ArmorConsumption(caster, target, pfmInfo.Performer, response.Target); //护甲消耗
                    DecorationConsumption(caster, target, response.Target); //饰品消耗
                }

                ShoesConsumption(caster, target, response.Target); //鞋子消耗
                //添加反馈记录
                pfmInfo.AddResponse(info: response);
                //记录伤害
                response.RegDamage(damage: finalDamage, isDodge);
            }
        }

        return infos.ToArray();
    }


    //守方鞋子
    private void ShoesConsumption(DiziCombatUnit caster, DiziCombatUnit target, DiziCombatInfo targetInfo)
    {
        if (caster.Equipment.Weapon == null || target.Equipment.Shoes == null) return;
        var casterWeapon = caster.Equipment.Weapon;
        var targetShoes = target.Equipment.Shoes;

        if (IsEquipmentBroken(targetShoes.Quality, casterWeapon.Quality))
        {
            target.EquipmentDisarmed(targetShoes);
            targetInfo.SetShoesDisarmed();
        }
    }
    
    //守方挂件
    private void DecorationConsumption(DiziCombatUnit caster, DiziCombatUnit target, DiziCombatInfo targetInfo)
    {
        if (caster.Equipment.Weapon == null || target.Equipment.Decoration == null) return;
        var casterWeapon = caster.Equipment.Weapon;
        var targetDecoration = target.Equipment.Decoration;

        if (IsEquipmentBroken(targetDecoration.Quality, casterWeapon.Quality))
        {
            target.EquipmentDisarmed(targetDecoration);
            targetInfo.SetDecorationDisarmed();
        }
    }

    //攻方武器与守方防具
    private void ArmorConsumption(DiziCombatUnit caster, DiziCombatUnit target, DiziCombatInfo casterInfo, DiziCombatInfo targetInfo)
    {
        if (caster.Equipment.Weapon == null || target.Equipment.Armor == null) return;
        var casterWeapon = caster.Equipment.Weapon;
        var targetArmor = target.Equipment.Armor;
        //这里填充武器品质差
        //是否执行者的武器被破坏

        if (IsEquipmentBroken(casterWeapon.Quality, targetArmor.Quality))
        {
            caster.EquipmentDisarmed(casterWeapon);
            casterInfo.SetWeaponDisarmed();
        }
        if (IsEquipmentBroken(targetArmor.Quality, casterWeapon.Quality))
        {
            target.EquipmentDisarmed(targetArmor);
            targetInfo.SetArmorDisarmed();
        }
    }

    private bool IsEquipmentBroken(int equipment, int comparer)
    {
        var brokenRatio = CombatFormula.EquipmentBrokenJudgment(equipment, comparer);
        var luck = Sys.Luck;
        return luck < brokenRatio;
    }

    //武器被打掉
    private void WeaponCompare(DiziCombatUnit caster, DiziCombatUnit target, DiziCombatInfo casterInfo,
        DiziCombatInfo targetInfo)
    {
        if (caster.Equipment.Weapon == null || target.Equipment.Weapon == null) return;
        var casterWeapon = caster.Equipment.Weapon;
        var targetWeapon = target.Equipment.Weapon;
        //这里填充武器品质差
        //是否执行者的武器被破坏

        if (IsEquipmentBroken(casterWeapon.Quality, targetWeapon.Quality))
        {
            caster.EquipmentDisarmed(casterWeapon);
            casterInfo.SetWeaponDisarmed();
        }

        if (IsEquipmentBroken(targetWeapon.Quality, casterWeapon.Quality))
        {
            target.EquipmentDisarmed(targetWeapon);
            targetInfo.SetWeaponDisarmed();
        }

        //bool IsWeaponBrokenFormula(int weapon, int comparer)
        //{
        //    var brokenRatio = 100; //CombatFormula.WeaponBrokenJudgment(weapon, comparer);
        //    var luck = Sys.Luck;
        //    return luck < brokenRatio;
        //}
    }

    //反击判断
    protected virtual bool CounterJudgment(DiziCombatUnit tar) => tar.IsAlive && false;//暂时不支持Counter

    //反击
    private void CounterAttack(DiziCombatUnit target,CombatArgs arg, DiziCombatPerformInfo perform)
    {
        var (damage, mpConsume, isDodge, isHard, isCritical) = GetDamage(arg);
        target.AddMp(-mpConsume);
        if (!isDodge) arg.Target.TakeReductionDamage(damage, arg);
        perform.SetCounterAttack(damage: arg.Caster.GetDamage(), isDodged: isDodge, isCritical); //记录反击伤害
        perform.SetHard(isHard);
    }
    //攻击伤害
    private (int damage, int mpConsume ,bool isDodge, bool isHard, bool isCritical) GetDamage(CombatArgs arg)
    {
        var (isDodge, dodgeRatio, dodgeRan) = CombatFormula.DodgeJudgment(arg, 1000);
        var (isHard, hardRatio, hardRan) = CombatFormula.HardJudgment(arg, 1000);
        var (isCritical, criticalRatio, criticalRan) = CombatFormula.CriticalJudgment(arg);
        var restraint = Config.Restraint; // 获取克制关系
        var damageRatio = restraint.ResolveRate(arg.Caster.Armed, arg.Target.Armed) * 0.01f; // 获取克制倍率
        (int damage, int mpConsume) result;
        if (isHard) result = CombatFormula.HardDamage(arg,damageRatio);
        else 
        if (isCritical) result = CombatFormula.CriticalDamage(arg,damageRatio);
        else result = CombatFormula.GeneralDamage(arg,damageRatio);

        return isDodge
            ? (0, 0, true, isHard, isCritical)
            : (result.damage, result.mpConsume, false, isHard, isCritical);
    }

    //选择目标
    public override DiziCombatUnit[] ChooseTargets(DiziCombatUnit caster, List<DiziCombatUnit> combatUnits)
    {
        var target = combatUnits.Where(predicate: c => c.TeamId != caster.TeamId)
            .Where(c => c.IsAlive)
            .OrderBy(keySelector: c => c.HpRatio)
            .FirstOrDefault();
        return target == null ? Array.Empty<DiziCombatUnit>() : new DiziCombatUnit[] { target };
    }
}

public class DiziCombatRound : Round<DiziCombatUnit, DiziRoundInfo, DiziCombatPerformInfo, DiziCombatInfo, Buff<DiziCombatUnit>>
{
    private BuffManager<DiziCombatUnit> BuffManager { get; set; }
    private Config.BattleConfig Config { get; }
    internal DiziCombatRound(List<DiziCombatUnit> combatUnits, BuffManager<DiziCombatUnit> buffManager, int round, Config.BattleConfig config) : base(
        combatUnits, buffManager, round)
    {
        BuffManager = buffManager;
        Config = config;
    }

    protected override void BeforeRoundExecute(DiziCombatUnit[] sortedAliveCombatUnits)
    {
        foreach (var caster in sortedAliveCombatUnits)
        {
            var selfBuffs = caster.GetSelfBuffs(RoundIndex, BuffManager).ToArray(); //获取自身buff
            foreach (var buff in selfBuffs) buff.Apply(caster); //添加buff
        }

        Array.ForEach(sortedAliveCombatUnits, unit => unit.BeforeStartRoundUpdate());
    }

    protected override CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
        ChooseBehavior(DiziCombatUnit unit) =>
        new DiziAttackBehavior(RoundIndex, BuffManager, Config);
}

public record DiziCombatPerformInfo :CombatPerformInfo<DiziCombatUnit, DiziCombatInfo>
{
    public bool IsHard { get; set; }
    public void SetHard(bool isHard) => IsHard = isHard;
    protected override StringBuilder Description(StringBuilder sb)
    {
        if (IsHard) sb.Append("重击,");
        return base.Description(sb);
    }
    //如果不重写, 调用不到base的ToString
    public override string ToString() => base.ToString();
}

public record DiziCombatInfo : CombatUnitInfo<DiziCombatUnit>
{
    public int Mp { get; set; }
    public int MaxMp { get; set; }
    public int WeaponId { get; set; }
    public int ArmorId { get; set; }
    public int ShoesId { get; set; }
    public int DecorationId { get; set; }

    public bool IsWeaponDisarmed { get; set; }
    public bool IsArmorDisarmed { get; set; }
    public bool IsShoesDisarmed { get; set; }
    public bool IsDecorationDisarmed { get; set; }
    public void SetWeaponDisarmed() => IsWeaponDisarmed = true;
    public void SetArmorDisarmed() => IsArmorDisarmed = true;
    public void SetShoesDisarmed() => IsShoesDisarmed = true;
    public void SetDecorationDisarmed() => IsDecorationDisarmed = true;

    public override void Set(DiziCombatUnit unit)
    {
        WeaponId = unit.Equipment.Weapon?.Id ?? -1;
        ArmorId = unit.Equipment.Armor?.Id ?? -1;
        ShoesId = unit.Equipment.Shoes?.Id ?? -1;
        DecorationId = unit.Equipment.Decoration?.Id ?? -1;
        Mp = unit.Mp.Value;
        MaxMp = unit.Mp.Max;
        base.Set(unit);
    }

    protected override StringBuilder Description(StringBuilder sb)
    {
        sb.Append($"MP:{Mp}/{MaxMp},");
        if (IsWeaponDisarmed) sb.Append("武器被打掉,");
        if (IsArmorDisarmed) sb.Append("防具被打掉,");
        if (IsShoesDisarmed) sb.Append("鞋子被打掉,");
        if (IsDecorationDisarmed) sb.Append("饰品被打掉,");
        return base.Description(sb);
    }
    //如果不重写, 调用不到base的ToString
    public override string ToString() => base.ToString();
}


public class DiziRoundInfo : RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
{
}