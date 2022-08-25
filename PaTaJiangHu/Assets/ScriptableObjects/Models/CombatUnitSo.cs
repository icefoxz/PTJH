using BattleM;
using UnityEngine;

[CreateAssetMenu(fileName = "combatUnit",menuName = "战斗测试/战斗单位")]
public class CombatUnitSo : ScriptableObject,ICombatUnit
{
    public int CombatId { get; }
    public IEquipment Equipment { get; }
    public int Position { get; }
    public int StandingPoint { get; }
    public string Name { get; }
    public bool IsExhausted { get; }
    public int Distance(ICombatInfo target)
    {
        throw new System.NotImplementedException();
    }

    public bool IsCombatRange(ICombatInfo unit)
    {
        throw new System.NotImplementedException();
    }

    public bool IsTargetRange()
    {
        throw new System.NotImplementedException();
    }

    public int Strength { get; }
    public int Agility { get; }
    public ICombatStatus Status { get; }
    public IBreathBar BreathBar { get; }
    public IForce ForceSkill { get; }
    public IMartial CombatSkill { get; }
    public IDodge DodgeSkill { get; }
    public void SetStandingPoint(int standingPoint)
    {
        throw new System.NotImplementedException();
    }

    public void SetStrategy(CombatUnit.Strategies strategy)
    {
        throw new System.NotImplementedException();
    }

    public void SetCombatId(int combatId)
    {
        throw new System.NotImplementedException();
    }

    public bool IsCombatFormAvailable(ICombatForm form)
    {
        throw new System.NotImplementedException();
    }
}
