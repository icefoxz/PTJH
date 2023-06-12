using System;
using Models;
using Server.Configs.Skills;
using UnityEngine;

public class SkillLevelStrategyTests : MonoBehaviour
{
    [SerializeField]private SkillLevelStrategySo LevelStrategy;
    // A Test behaves as an ordinary method
    void Start() => SkillLevelStrategyTestsSimplePasses();
    public void SkillLevelStrategyTestsSimplePasses()
    {
        // Use the Assert class to test conditions
        var level1CombatSet = LevelStrategy.GetCombatSet(1);
        var com1 = CombatArgs.InstanceCombatUnit(guid: Guid.NewGuid().ToString(), name: "Test1", hp: 100, mp: 100, strength: 20, agility: 20, teamId: 0, combat: level1CombatSet, null);
        var com2 = CombatArgs.InstanceCombatUnit(guid: Guid.NewGuid().ToString(), name: "Test2", hp: 100, mp: 100, strength: 30, agility: 30, teamId: 1, combat: level1CombatSet, null);
        var arg = new CombatArgs(com1, com2, 1);
        var dodgeRate = level1CombatSet.GetDodgeRate(arg);
        var criticalRate = level1CombatSet.GetCriticalRate(arg);
        var hardRate = level1CombatSet.GetHardRate(arg);
        var hardDamageRatio = level1CombatSet.GetHardDamageRatioAddOn(arg);
        var mpdDamage = level1CombatSet.GetMpUses(arg);
        var mpCounteract = level1CombatSet.GetDamageMpArmorRate(arg);
        print(
            $"闪避率:{dodgeRate}, 会心率:{criticalRate}, 重击率:{hardRate}, 倍率:{hardDamageRatio}, Mp伤:{mpdDamage}, Mp护罩:{mpCounteract}");
    }

}
