#if UNITY_EDITOR

using Server.Configs.BattleSimulation;
using Server.Configs.Skills;
using UnityEditor;
using UnityEngine;


public abstract class SkillFieldSOEditor : Editor
{
    [Min(1)]protected int level = 1; // Add a field to store the parameter value
    [Min(1)]protected int cAgi = 10; // Add a field to store the parameter value
    [Min(1)]protected int cStr = 10; // Add a field to store the parameter value
    [Min(1)]protected int tAgi = 10; // Add a field to store the parameter value
    [Min(1)]protected int tStr = 10; // Add a field to store the parameter value
    [Min(1)]protected int cHp = 100; // Add a field to store the parameter value
    [Min(1)]protected int tHp = 100; // Add a field to store the parameter value
    [Min(1)]protected int cMp = 100; // Add a field to store the parameter value
    [Min(1)]protected int tMp = 100; // Add a field to store the parameter value

    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Get a reference to the SkillFieldSO script
        var so = (SkillFieldSo)target;

        // Create a label and an int field for the parameter
        EditorGUILayout.LabelField("测试 CombatSet 等级", EditorStyles.boldLabel);
        level = EditorGUILayout.IntField("技能等级", level);
        cStr = EditorGUILayout.IntField("攻力", cStr);
        tStr = EditorGUILayout.IntField("守力", tStr);
        cAgi = EditorGUILayout.IntField("攻敏", cAgi);
        tAgi = EditorGUILayout.IntField("守敏", tAgi);
        cHp = EditorGUILayout.IntField("攻血", cHp);
        tHp = EditorGUILayout.IntField("守血", tHp);
        cMp = EditorGUILayout.IntField("攻内", cMp);
        tMp = EditorGUILayout.IntField("守内", tMp);

        // Create a button that calls the TestFunction method with the parameter when pressed
        if (GUILayout.Button("测试-导出配置结果"))
        {
            var arg = GenSimCombat();
            var set = so.GetCombatSet(level);
            var hardRate = set.GetHardRate(arg);
            var hardMul = 1 + set.GetHardDamageRatio(arg);
            var criRate = set.GetCriticalRate(arg);
            var criMul = 1 + set.GetCriticalDamageRatio(arg);
            var mpDmg = set.GetMpDamage(arg);
            var mpCou = set.GetMpCounteract(arg);
            var dodgeRate = set.GetDodgeRate(arg);
            Debug.Log($"{arg.Caster}, {arg.Target} \n重击率: {hardRate}, 重击倍: {hardMul}, 会心率: {criRate}, 会心倍: {criMul}, 内使用: {mpDmg}, 内抵消: {mpCou}, 闪避率: {dodgeRate}");
        }
    }

    private CombatArgs GenSimCombat()
    {
        ISimCombat sim1 = new SimCombat("攻", cStr, cAgi, cHp, cMp);
        ISimCombat sim2 = new SimCombat("守", tStr, tAgi, tHp, tMp);
        var test1 = new DiziCombatUnit(sim1, 0);
        var test2 = new DiziCombatUnit(sim2, 1);
        return new CombatArgs(test1, test2);
    }

    private record SimCombat : ISimCombat
    {
        public string Name { get; }
        public int Power { get; set; }
        public int Damage { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public float Strength { get; set; } = 10;
        public float Agility { get; set; } = 10;
        public float Hp { get; set; } = 100;
        public float Mp { get; set; } = 100;
        public int Weapon { get; set; }
        public int Armor { get; set; }

        public SimCombat(string name)
        {
            Name = name;
        }

        public SimCombat(string name, int strength, int agility, int hp, int mp)
        {
            Name = name;
            Power = 0;
            Damage = strength;
            MaxHp = hp;
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
            Weapon = 0;
            Armor = 0;
        }
    }
}

#endif