#if UNITY_EDITOR

using Server.Configs.BattleSimulation;
using Server.Configs.Skills;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(ForceFieldSo))]
public class SkillFieldSOEditor : Editor
{
    [Min(1)]private int level = 1; // Add a field to store the parameter value

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the SkillFieldSO script
        var so = (SkillFieldSo)target;

        // Create a label and an int field for the parameter
        EditorGUILayout.LabelField("测试 CombatSet 等级", EditorStyles.boldLabel);
        level = EditorGUILayout.IntField("等级", level);

        // Create a button that calls the TestFunction method with the parameter when pressed
        if (GUILayout.Button("配置结果"))
        {
            var arg = GenSimCombat();
            var set = so.GetCombatSet(level);
            var hardRate = set.GetHardRate(arg);
            var hardMul = set.GetHardDamageRatio(arg);
            var criRate = set.GetCriticalRate(arg);
            var criMul = set.GetCriticalMultiplier(arg);
            var mpDmg = set.GetMpDamage(arg);
            var mpCou = set.GetMpCounteract(arg);
            var dodgeRate = set.GetDodgeRate(arg);
            Debug.Log($"重击率: {hardRate}, 重击倍: {hardMul}, 会心率: {criRate}, 会心倍: {criMul}, 内使用: {mpDmg}, 内抵消: {mpCou}, 闪避率: {dodgeRate}");
        }
    }

    private CombatArgs GenSimCombat()
    {
        ISimCombat sim1 = new SimCombat("测试1");
        ISimCombat sim2 = new SimCombat("测试2");
        var test1 = new DiziCombatUnit(sim1, 0);
        var test2 = new DiziCombatUnit(sim2, 1);
        return new CombatArgs(test1, test2);
    }

    private class SimCombat : ISimCombat
    {
        public string Name { get; }
        public int Power => 0;
        public int Damage => 0;
        public int MaxHp => 0;
        public int Strength => 10;
        public int Agility => 10;
        public int Hp => 100;
        public int Mp => 100;
        public int Weapon => 0;
        public int Armor => 0;

        public SimCombat(string name)
        {
            Name = name;
        }
    }
}

#endif