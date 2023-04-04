using _GameClient.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;


/// <summary>
/// 弟子战斗单位
/// </summary>
public class DiziCombatUnit : CombatUnit
{
    
    public int Mp { get; private set; }
    public int MaxMp { get; private set; }
    public int Agility { get; private set; }

    internal DiziCombatUnit(int teamId, Dizi dizi) 
        : base(teamId, dizi.Name, dizi.Hp, dizi.Strength, dizi.Agility)
    {
        Mp = dizi.Mp;
        MaxMp = dizi.Mp;
        Agility = dizi.Agility;
    }

    internal DiziCombatUnit(int teamId, CombatNpcSo npc) 
        : base(teamId, npc.Name, npc.Hp, npc.Strength, npc.Agility)
    {
        Mp = npc.Mp;
        MaxMp = npc.Mp;
        Agility = npc.Agility;
    }

    internal DiziCombatUnit(ICombatUnit unit) : base(unit){}
    internal DiziCombatUnit(ISimCombat dizi) : base(dizi)
    {
        Mp = dizi.Mp;
        MaxMp = dizi.Mp;
        Agility = dizi.Agility;
    }
    //伤害减免
    protected override int DamageReduction(int damage)
    {
        var (finalDamage, offset) = CombatFormula.DamageReduction(damage, Mp, MaxMp);
        Mp -= offset;
        return finalDamage;
    }

    public override string ToString() => $"{Name}[{Hp}/{MaxHp}],[{Mp}/{MaxMp}]";
}

/// <summary>
/// 弟子攻击行为, 处理各种攻击逻辑
/// </summary>
public class DiziAttackBehavior : CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo>
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
                var (damage, isDodge, isHard) = GetDamage(caster: caster, target: target);//获取伤害
                var finalDamage = (int)damage;

                //生成行动记录
                var perform = new DiziCombatPerformInfo();
                perform.Set(caster);

                //添加入行动列表
                infos.Add(item: perform);
                //设定战斗行动
                perform.SetHard(isHard);
                //执行伤害
                target.TakeDamage(damage: finalDamage);
                var canCounter = CounterJudgment(target);
                if (canCounter) CounterAttack(caster: target, target: caster, perform: perform);//反击执行
                //生成反馈记录
                var response = new CombatResponseInfo<DiziCombatUnit>();
                response.Set(target);

                //添加反馈记录
                perform.AddResponse(info: response);
                //记录伤害
                response.RegDamage(damage: finalDamage, isDodge);
            }
        }
        return infos.ToArray();
    }

    //反击判断
    protected virtual bool CounterJudgment(DiziCombatUnit tar) => tar.IsAlive && false;//暂时不支持Counter

    //反击
    private void CounterAttack(DiziCombatUnit caster, DiziCombatUnit target, CombatPerformInfo<DiziCombatUnit> perform)
    {
        var (damage, isDodge, isCritical) = GetDamage(caster, target);
        if (!isDodge) target.TakeDamage(damage: (int)damage);
        perform.SetCounterAttack(damage: caster.Damage, isDodged: isDodge, isCritical); //记录反击伤害
    }
    //攻击伤害
    private (float damage, bool isDodge, bool isHard) GetDamage(DiziCombatUnit caster, DiziCombatUnit target)
    {
        var (isDodge, dodgeRatio, dodgeRan) = CombatFormula.DodgeJudgment(caster, target, 1000);
        var (ishard, hardRatio, hardRan) = CombatFormula.HardJudgment(caster, target, 1000);

        var damage = isDodge ? 0
            : ishard ? CombatFormula.HardDamage(caster)
            : caster.Damage;
        return (damage, isDodge, ishard);
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

public class DiziCombatRound : Round<DiziCombatUnit, DiziCombatPerformInfo>
{
    public DiziCombatRound(List<DiziCombatUnit> combatUnits, BuffManager<DiziCombatUnit> buffManager) : base(combatUnits, buffManager)
    {
    }

    protected override CombatBehavior<DiziCombatUnit, DiziCombatPerformInfo> ChooseBehavior(DiziCombatUnit unit) => new DiziAttackBehavior();
}

public record DiziCombatPerformInfo :CombatPerformInfo<DiziCombatUnit>
{
    public bool IsHard { get; set; }
    public void SetHard(bool isHard) => IsHard = isHard;
}