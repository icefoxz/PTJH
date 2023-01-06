using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;
using Utls;
using Random = UnityEngine.Random;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "GradeConfig", menuName = "资质/弟子生成配置")]
    internal class GradeConfigSo : ScriptableObject
    {
        public enum Grades
        {
            [InspectorName("白")]F,
            [InspectorName("绿")]E,
            [InspectorName("篮")]D,
            [InspectorName("紫")]C,
            [InspectorName("橙")]B,
            [InspectorName("红")]A,
            [InspectorName("金")]S,
        }
        public static string GetColorTitle(Grades grade) => grade switch
        {
            Grades.F => "白",
            Grades.E => "绿",
            Grades.D => "篮",
            Grades.C => "紫",
            Grades.B => "橙",
            Grades.A => "红",
            Grades.S => "金",
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
        };

        public enum Props
        {
            Stamina,
            InventorySlot
        }

        [SerializeField] private GradeConfig[] _gradeConfigs;
        private Dictionary<Grades, GradeConfig> _configsCache;

        private GradeConfig GradeConfigs(Grades grade)
        {
            _configsCache ??= _gradeConfigs.ToDictionary(g => g.Grade, g => g);
            return _configsCache[grade];
        }
        public CombatFieldSo GenerateCombatSkill(int grade) => GradeConfigs((Grades)grade).GenerateCombatSkill();
        public ForceFieldSo GenerateForceSkill(int grade) => GradeConfigs((Grades)grade).GenerateForceSkill();
        public DodgeFieldSo GenerateDodgeSkill(int grade) => GradeConfigs((Grades)grade).GenerateDodgeSkill();

        public (GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp, int Stamina, int bagSlot)
            GenerateFromGrade(int grade)
        {
            return GeneratePops(GradeConfigs((Grades)grade));

            (GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp, int Stamina, int bagSlot) GeneratePops(
                GradeConfig config)
            {
                var (strength, agility, hp, mp) = config.GeneratePentagon();
                var stamina = config.GenerateProp(Props.Stamina);
                var inventory = config.GenerateProp(Props.InventorySlot);
                return (strength, agility, hp, mp, stamina, inventory);
            }
        }


        [Serializable]
        private class GradeConfig
        {
            private bool RenameElement()
            {
                var gradeText = Grade switch
                {
                    Grades.F => "白",
                    Grades.E => "绿",
                    Grades.D => "篮",
                    Grades.C => "紫",
                    Grades.B => "橙",
                    Grades.A => "红",
                    Grades.S => "金",
                    _ => throw new ArgumentOutOfRangeException()
                };
                var combatText = CombatSkillGradeSo == null ? "武功缺失!" : string.Empty;
                var forceText = ForceSkillGradeSo == null ? "内功缺失!" : string.Empty;
                var dodgeText = DodgeSkillGradeSo == null ? "轻功缺失!" : string.Empty;
                _name = gradeText + "Pen:" + 随机五维.Length + " " + combatText + forceText + dodgeText;
                return true;
            }
            [ConditionalField(true, nameof(RenameElement))][SerializeField][ReadOnly] private string _name;
            [SerializeField] public Grades Grade;
            [SerializeField] private MinMaxVectorInt 体力;
            [SerializeField] private MinMaxVectorInt 背包格;
            [SerializeField] private PentagonConfig[] 随机五维;
            [SerializeField] private CombatSkillGradeSo 初始武功配置;
            [SerializeField] private ForceSkillGradeSo 初始内功配置;
            [SerializeField] private DodgeSkillGradeSo 初始轻功配置;

            private MinMaxVectorInt Stamina => 体力;
            private MinMaxVectorInt InventorySlot => 背包格;
            private CombatSkillGradeSo CombatSkillGradeSo => 初始武功配置;
            private ForceSkillGradeSo ForceSkillGradeSo => 初始内功配置;
            private DodgeSkillGradeSo DodgeSkillGradeSo => 初始轻功配置;

            private PentagonConfig Pentagon => 随机五维.RandomPick();

            public int GenerateProp(Props prop)
            {
                return prop switch
                {
                    Props.Stamina => Stamina.Generate(),
                    Props.InventorySlot => InventorySlot.Generate(),
                    _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
                };
            }
            private readonly PentagonGradeSo.Elements[] ElementArray = new PentagonGradeSo.Elements[]
            {
                PentagonGradeSo.Elements.Strength,
                PentagonGradeSo.Elements.Agility,
                PentagonGradeSo.Elements.Hp,
                PentagonGradeSo.Elements.Mp,
            };

            public CombatFieldSo GenerateCombatSkill() => CombatSkillGradeSo.PickSkill();
            public ForceFieldSo GenerateForceSkill() => ForceSkillGradeSo.PickSkill();
            public DodgeFieldSo GenerateDodgeSkill() => DodgeSkillGradeSo.PickSkill();
            
            public (GradeValue<int> strength, GradeValue<int> agility, GradeValue<int> hp, GradeValue<int> mp) GeneratePentagon()
            {
                var ran = Pentagon.Generate(ElementArray.OrderByDescending(_ => Random.Range(0, 10)))
                    .ToDictionary(r => r.element, r => (r.value, r.grade));

                return (InstanceValueGrade(ran[PentagonGradeSo.Elements.Strength]),
                        InstanceValueGrade(ran[PentagonGradeSo.Elements.Agility]),
                        InstanceValueGrade(ran[PentagonGradeSo.Elements.Hp]),
                        InstanceValueGrade(ran[PentagonGradeSo.Elements.Mp])
                    );
            }

            private GradeValue<int> InstanceValueGrade((int value, SkillGrades grade) t) => new(t.value, (int)t.grade);
        }

        [Serializable]
        private class PentagonConfig
        {
            [SerializeField] private PentagonGradeSo 随1;
            [SerializeField] private PentagonGradeSo 随2;
            [SerializeField] private PentagonGradeSo 随3;
            [SerializeField] private PentagonGradeSo 随4;

            private PentagonGradeSo[] Ran => new []{ 随1, 随2, 随3, 随4 };

            public (PentagonGradeSo.Elements element, int value, SkillGrades grade)[] Generate(IEnumerable<PentagonGradeSo.Elements> array) =>
                array.Select((e, i) =>
                {
                    var r = Ran[i].GenerateProp(e);
                    return  (e, r.Item1, r.Item2);
                }).ToArray();
        }
    }
}