using System.Collections.Generic;
using System;
using System.Linq;
using Models;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Utls;

public interface IDiziCombatUnit : ICombatUnit
{
    string Guid { get; }
    int Mp { get; }
    int MaxMp { get; }
    int Strength { get; }
    int Agility { get; }
    ICombatSet CombatSet { get; }
    IDiziEquipment Equipment { get; }
}

/// <summary>
/// 弟子战斗单位
/// </summary>
public class DiziCombatUnit : CombatUnit, IDiziCombatUnit
{
    private DiziEquipment _equipment;
    private string _name;
    private int _strength;
    private int _agility;
    private int _maxHp;
    private int _maxMp;

    private int _hp;
    private int _mp;

    public string Guid { get; private set; }
    /**
     * 重写基本属性是为了调整对武器的加成作用
     */
    public override string Name => _name;
    public override int MaxHp => _maxHp + _equipment.GetPropAddon(DiziProps.Hp);
    public override int Damage => Strength; //暂时攻击力直接引用力量
    public override int Speed => Agility; //暂时速度直接引用敏捷
    public int MaxMp => _maxMp + _equipment.GetPropAddon(DiziProps.Mp);
    public int Strength => _strength;
    public int Agility => _agility;

    //当前hp与mp是状态
    public override int Hp => _hp;
    public int Mp => _mp;

    public ICombatSet CombatSet { get; private set; }

    public IDiziEquipment Equipment => _equipment;
    public int GetStrength() => Strength + _equipment.GetPropAddon(DiziProps.Strength);
    public int GetAgility() => Agility + _equipment.GetPropAddon(DiziProps.Agility);
    public int GetHp() => Hp + _equipment.GetPropAddon(DiziProps.Hp);
    public int GetMp() => Mp + _equipment.GetPropAddon(DiziProps.Mp);

    internal DiziCombatUnit(string guid,int teamId, string name, int strength, int agility, int hp, int mp, ICombatSet set, IDiziEquipment equipment = null)
        : base(teamId: teamId)
    {
        Guid = guid;
        _name = name;
        _strength = strength;
        _agility = agility;
        _maxHp = hp;
        _maxMp = mp;
        CombatSet = set;
        _equipment = equipment == null ? new DiziEquipment() : new DiziEquipment(equipment);
        StatusFull();
    }
    //填满hp与mp
    private void StatusFull()
    {
        _hp = MaxHp; //状态最后才赋值
        _mp = MaxMp; //状态最后才赋值
    }

    internal DiziCombatUnit(int teamId, Dizi dizi) 
        : base(teamId: teamId)
    {
        Guid = dizi.Guid;
        _name = dizi.Name;
        _strength = dizi.Strength;
        _maxHp = dizi.Hp;
        _maxMp = dizi.Mp;
        _agility = dizi.Agility;
        _strength = dizi.Strength;
        CombatSet = dizi.GetCombatSet();
        _equipment = new DiziEquipment(dizi.Equipment);
        StatusFull();
    }

    internal DiziCombatUnit(int teamId, CombatNpcSo npc) : base(teamId: teamId)
    {
        _name = npc.Name;
        _strength = npc.Strength;
        _agility = npc.Agility;
        _maxHp = npc.Hp;
        _maxMp = npc.Mp;
        CombatSet = npc.GetCombatSet();
        _equipment = new DiziEquipment(npc.Equipment);
        StatusFull();
    }

    internal DiziCombatUnit(IDiziCombatUnit unit) : base(teamId: unit.TeamId)
    {
        _name = unit.Name;
        _strength = unit.Strength;
        _agility = unit.Agility;
        _maxHp = unit.Hp;
        _maxMp = unit.Mp;
        CombatSet = unit.CombatSet;
        _equipment = new DiziEquipment(unit.Equipment);
        //DiziCombatUnit 接口是直接复制属性,不会重置状态
        _hp = unit.Hp;
        _maxMp = unit.Mp;
    }

    public DiziCombatUnit(ISimCombat s, int teamId) : base(teamId: teamId)
    {
        _name = s.Name;
        _maxMp = (int)s.Mp;
        _maxHp = (int)s.Hp;
        _agility = (int)s.Agility;
        _strength = (int)s.Strength;
        CombatSet = Server.Configs.Battles.CombatSet.Empty;
        _equipment = new DiziEquipment();
        StatusFull();
    }
    
    //伤害减免
    public int TakeReductionDamage(int damage,CombatArgs arg)
    {
        var (finalDamage, mpConsume) = CombatFormula.DamageReduction(damage: damage, arg: arg);
        AddMp(mp: -mpConsume);
        AddHp(hp: -finalDamage);
        return finalDamage;
    }

    public override string ToString() => $"{Name}:力[{Strength}],敏[{Agility}],血[{Hp}/{MaxMp}],内[{Mp}/{MaxMp}]|伤:{Damage},速:{Speed}";

    public void AddMp(int mp)
    {
        _mp += mp;
        Math.Clamp(value: _hp, min: 0, max: MaxMp);
    }

    public override int AddHp(int hp)
    {
        _hp += hp;
        Math.Clamp(_hp, 0, MaxHp);
        return hp;
    }

    public void EquipmentDisarmed(IEquipment equipment) => _equipment.CombatDisarm(equipment);
    public void EquipmentArmed(IEquipment equipment) => _equipment.CombatArm(equipment);

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
            Weapon = equipment.Weapon;
            Armor = equipment.Armor;
            Shoes = equipment.Shoes;
            Decoration = equipment.Decoration;
        }
        public int GetPropAddon(DiziProps prop) => (int)AllEquipments.Sum(e => e.GetAddOn(prop));
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
}

/// <summary>
/// 弟子攻击行为, 处理各种攻击逻辑
/// </summary>
public class DiziAttackBehavior : CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
{
    private int Combo { get; } = 1;

    //攻击行为
    public override DiziCombatPerformInfo[] Execute(DiziCombatUnit caster, DiziCombatUnit[] targets)
    {
        var infos = new List<DiziCombatPerformInfo>();
        var combos = Combo;//获取连击信息
        for (var i = 0; i < combos; i++)//连击次数
        {
            foreach (var target in targets)//对所有执行目标历遍执行
            {
                var arg = new CombatArgs(caster, target);
                var (dmg, isDodge, isHard, isCritical) = GetDamage(arg);//获取伤害
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

                //执行伤害
                var finalDamage = target.TakeReductionDamage(damage, arg);//有计算减免的伤害
                var canCounter = CounterJudgment(target);
                if (canCounter) CounterAttack(new CombatArgs(target, caster), pfmInfo);//反击执行
                //生成反馈记录
                var response = new CombatResponseInfo<DiziCombatUnit, DiziCombatInfo>();
                response.Set(target);
                if(!isDodge)
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
    private void CounterAttack(CombatArgs arg, DiziCombatPerformInfo perform)
    {
        var (damage, isDodge, isHard ,isCritical) = GetDamage(arg);
        if (!isDodge) arg.Target.TakeReductionDamage((int)damage, arg);
        perform.SetCounterAttack(damage: arg.Caster.Damage, isDodged: isDodge, isCritical); //记录反击伤害
        perform.SetHard(isHard);
    }
    //攻击伤害
    private (float damage, bool isDodge, bool isHard, bool isCritical) GetDamage(CombatArgs arg)
    {
        var (isDodge, dodgeRatio, dodgeRan) = CombatFormula.DodgeJudgment(arg, 1000);
        var (ishard, hardRatio, hardRan) = CombatFormula.HardJudgment(arg, 1000);
        var (isCritical, criticalRatio, criticalRan) = CombatFormula.CriticalJudgment(arg);

        int damage;
        if (ishard) damage = CombatFormula.HardDamage(arg);
        else 
        if (isCritical) damage = CombatFormula.CriticalDamage(arg);
        else damage = CombatFormula.GeneralDamage(arg);
        return isDodge ? (0, true, ishard, isCritical) : (damage, false, ishard, isCritical);
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

public class DiziCombatRound : Round<DiziCombatUnit, DiziRoundInfo, DiziCombatPerformInfo, DiziCombatInfo>
{
    public DiziCombatRound(List<DiziCombatUnit> combatUnits, BuffManager<DiziCombatUnit> buffManager) : base(
        combatUnits, buffManager)
    {
    }

    protected override CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
        ChooseBehavior(DiziCombatUnit unit) => new DiziAttackBehavior();
}

public record DiziCombatPerformInfo :CombatPerformInfo<DiziCombatUnit, DiziCombatInfo>
{
    public bool IsHard { get; set; }
    public void SetHard(bool isHard) => IsHard = isHard;
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
        Mp = unit.Mp;
        MaxMp = unit.MaxMp;
        base.Set(unit);
    }
}


public class DiziRoundInfo : RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
{

}