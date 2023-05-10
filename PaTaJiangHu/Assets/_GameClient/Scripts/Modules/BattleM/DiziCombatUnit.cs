using System.Collections.Generic;
using System;
using System.Linq;
using Models;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;

public interface IDiziCombatUnit : ICombatUnit
{
    string Guid { get; }
    int Mp { get; }
    int MaxMp { get; }
    int Strength { get; }
    int Agility { get; }
    ICombatSet Combat { get; }
    IDiziEquipment Equipment { get; }
}

/// <summary>
/// 弟子战斗单位
/// </summary>
public class DiziCombatUnit : CombatUnit, IDiziCombatUnit
{
    public string Guid { get; private set; }
    public int Mp { get; private set; }
    public int MaxMp { get; private set; }
    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public ICombatSet Combat { get; private set; }
    public IDiziEquipment Equipment { get; }

    internal DiziCombatUnit(int teamId, Dizi dizi) 
        : base(teamId: teamId, name: dizi.Name, maxHp: dizi.Hp, damage: dizi.Strength, speed: dizi.Agility)
    {
        Guid = dizi.Guid;
        Mp = dizi.Mp;
        MaxMp = dizi.Mp;
        Agility = dizi.Agility;
        Strength = dizi.Strength;
        Combat = dizi.GetCombatSet();
        Equipment = dizi.Equipment;
    }

    internal DiziCombatUnit(int teamId, CombatNpcSo npc) 
        : base(teamId: teamId, name: npc.Name, maxHp: npc.Hp, damage: npc.Strength, speed: npc.Agility)
    {
        Mp = npc.Mp;
        MaxMp = npc.Mp;
        Agility = npc.Agility;
        Strength = npc.Strength;
        Combat = npc.GetCombatSet();
        Equipment = npc.Equipment;
    }

    internal DiziCombatUnit(IDiziCombatUnit unit) : base(u: unit)
    {
        Mp = unit.Mp;
        MaxMp = unit.Mp;
        Agility = unit.Agility;
        Strength = unit.Strength;
        Combat = unit.Combat;
        Equipment = unit.Equipment;
    }

    public DiziCombatUnit(ISimCombat s, int teamId) : base(teamId: teamId, name: s.Name, maxHp: s.MaxHp, damage: s.Damage, speed: (int)s.Agility)
    {
        Mp = (int)s.Mp;
        MaxMp = (int)s.Mp;
        Agility = (int)s.Agility;
        Strength = (int)s.Strength;
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
        Mp += mp;
        Math.Clamp(value: Mp, min: 0, max: MaxMp);
    }

    public void EquipmentUpdate(IEquipment equipment)
    {
        if (Equipment == null) return;
        IDiziCombatUnit update = Equipment.CombatDisarm(TeamId, equipment);
        Mp = update.Mp;
        MaxMp = update.MaxMp;
        Strength = update.Strength;
        Agility = update.Agility;
        Combat = update.Combat;
        Hp = update.Hp;
        MaxHp = update.MaxHp;
        Damage = update.Damage;
        Speed = update.Speed;
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
                //添加反馈记录
                pfmInfo.AddResponse(info: response);
                //记录伤害
                response.RegDamage(damage: finalDamage, isDodge);
            }
        }
        return infos.ToArray();
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

    public override void Set(DiziCombatUnit unit)
    {
        Mp = unit.Mp;
        MaxMp = unit.MaxMp;
        base.Set(unit);
    }
}


public class DiziRoundInfo : RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>
{

}