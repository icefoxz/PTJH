using _GameClient.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;
using Dizi = Models.Dizi;

public interface IDiziCombatUnit : ICombatUnit
{
    string Guid { get; }
    int Mp { get; }
    int MaxMp { get; }
    int Strength { get; }
    int Agility { get; }
    ICombatSet Combat { get; }
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
    public ICombatSet Combat { get; }

    internal DiziCombatUnit(int teamId, Dizi dizi) 
        : base(teamId, dizi.Name, dizi.Hp, dizi.Strength, dizi.Agility)
    {
        Guid = dizi.Guid;
        Mp = dizi.Mp;
        MaxMp = dizi.Mp;
        Agility = dizi.Agility;
        Strength = dizi.Strength;
        Combat = dizi.GetBattle();
    }

    internal DiziCombatUnit(int teamId, CombatNpcSo npc) 
        : base(teamId, npc.Name, npc.Hp, npc.Strength, npc.Agility)
    {
        Mp = npc.Mp;
        MaxMp = npc.Mp;
        Agility = npc.Agility;
        Strength = npc.Strength;
        Combat = npc.GetCombatSet();
    }

    internal DiziCombatUnit(IDiziCombatUnit unit) : base(unit)
    {
        Mp = unit.Mp;
        MaxMp = unit.Mp;
        Agility = unit.Agility;
        Strength = unit.Strength;
        Combat = unit.Combat;
    }

    public DiziCombatUnit(ISimCombat s, int teamId) : base(teamId, s.Name, s.MaxHp, s.Damage, s.Agility)
    {
        Mp = s.Mp;
        MaxMp = s.Mp;
        Agility = s.Agility;
        Strength = s.Strength;
    }
    
    //伤害减免
    public int TakeReductionDamage(int damage,CombatArgs arg)
    {
        var (finalDamage, mpConsume) = CombatFormula.DamageReduction(damage, arg);
        AddMp(-mpConsume);
        AddHp(-finalDamage);
        return finalDamage;
    }

    public override string ToString() => $"{Name}[{Hp}/{MaxHp}],[{Mp}/{MaxMp}]";

    public void AddMp(int mp)
    {
        Mp += mp;
        Math.Clamp(Mp, 0, MaxMp);
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