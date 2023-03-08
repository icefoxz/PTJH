using System;
using Server.Configs.BattleSimulation;
using UnityEditor;
using UnityEngine;
using Utls;

public class TestSimulationOutput : MonoBehaviour
{
    [SerializeField] private BattleSimulatorConfigSo SimSo;
    [SerializeField] private Unit 玩家单位;
    [SerializeField] private Unit 敌人单位;
    private Unit Player => 玩家单位;
    private Unit Enemy => 敌人单位;

    private (ISimCombat player,ISimCombat enemy)GetCombatUnit()
    {
        var p = GetSimulation(Player);
        PrintCombat(p);
        var e = GetSimulation(Enemy);
        PrintCombat(e);
        return (p, e);
    }

    private static void PrintCombat(ISimCombat com) =>
        print($"设定完毕!,名字={com.Name},战力[{com.Power}],攻击力[{com.Offend}],抗击力[{com.Defend}]");

    public void StartBattle()
    {
        var(player, enemy) = GetCombatUnit();
        var outcome = SimSo.CountSimulationOutcome(player, enemy);
        for (var i = 0; i < outcome.CombatMessages.Length; i++)
        {
            var message = outcome.CombatMessages[i];
            print(message);
        }
        print(outcome.IsPlayerWin ? $"{player.Name}获胜!" : $"{enemy.Name}获胜!");
        print($"战斗结束!总: 回={outcome.Rounds}, 玩家战后血量 = {outcome.PlayerRemaining}, 敌人战后血量 = {outcome.EnemyRemaining}");
    }
    private ISimCombat GetSimulation(Unit unit) =>
        SimSo.GetSimulation(unit.Name, unit.Strength, unit.Agility, unit.Hp, unit.Mp, unit.WeaponDamage,
            unit.ArmorAddHp);

    [Serializable]private class Unit
    {
        [SerializeField] private string 名字;
        [SerializeField] private int 力;
        [SerializeField] private int 敏;
        [SerializeField] private int 血;
        [SerializeField] private int 内;
        [SerializeField] private int 武器伤害;
        [SerializeField] private int 防具加血量;

        public string Name => 名字;
        public int Strength => 力;
        public int Agility => 敏;
        public int Hp=> 血;
        public int Mp => 内;
        public int WeaponDamage => 武器伤害;
        public int ArmorAddHp => 防具加血量;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestSimulationOutput))]
public class TestSimEditor : Editor
{
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("点击开始战斗"))
        {
            var script = (TestSimulationOutput)target;
            script.StartBattle();
        }
        base.OnInspectorGUI();
    }
}
#endif