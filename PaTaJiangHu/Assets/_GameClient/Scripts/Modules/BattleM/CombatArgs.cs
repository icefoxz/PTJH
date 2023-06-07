using System;
using Models;

/// <summary>
/// 战斗参数, 用于传递战斗信息, 所有信息都是当前状态记录, 除了<see cref="ICombatSet"/>
/// </summary>
public class CombatArgs : EventArgs
{
    public int Round { get; set; }
    public DiziCombatUnit Caster { get; }
    public DiziCombatUnit Target { get; }

    public CombatArgs(DiziCombatUnit caster, DiziCombatUnit target, int round)
    {
        Caster = caster;
        Target = target;
        Round = round;
    }

    public (DiziCombatUnit self,DiziCombatUnit other) GetUnits(CombatUnit unit)
    {
        if (unit.InstanceId == Caster.InstanceId)
            return (Caster, Target);
        if (unit.InstanceId == Target.InstanceId)
            return (Target, Caster);
        throw new InvalidOperationException($"{unit.InstanceId}.{unit}不属于此数据组!");
    }

    public static DiziCombatUnit InstanceCombatUnit(string guid, string name, int hp, int mp, int strength, int agility,
        int teamId, ICombatSet combat, IDiziEquipment equipment) => new(false, new DiziCombatUnit(guid: guid,
        teamId: teamId, name: name, strength: strength, agility: agility, hp: hp, mp: mp, set: combat, equipment: equipment));

    public static CombatArgs Instance(Dizi a,Dizi b, int round)
    {
        var aCombat = InstanceCombatUnit(guid: a.Guid, name: a.Name, hp: a.Hp, mp: a.Mp, strength: a.Strength,
            agility: a.Agility, teamId: 0, combat: a.GetCombatSet(), a.Equipment);
        var bCombat = InstanceCombatUnit(guid: b.Guid, name: b.Name, hp: b.Hp, mp: b.Mp, strength: b.Strength,
            agility: b.Agility, teamId: 1, combat: b.GetCombatSet(), b.Equipment);
        return new CombatArgs(caster: aCombat, target: bCombat, round);
    }
}