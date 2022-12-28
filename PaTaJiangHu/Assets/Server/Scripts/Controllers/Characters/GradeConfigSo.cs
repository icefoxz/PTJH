using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;
using Random = UnityEngine.Random;

namespace Server.Controllers.Characters
{
    [CreateAssetMenu(fileName = "GradeConfig", menuName = "配置/资质配置")]
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

        public (int strength, int agility, int hp, int mp, int Stamina, int bagSlot)
            GenerateFromGrade(int grade)
        {
            return GeneratePops(GradeConfigs((Grades)grade));

            (int strength, int agility, int hp, int mp, int Stamina, int bagSlot) GeneratePops(
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
            [SerializeField] public Grades Grade;
            [SerializeField] private MinMaxVectorInt 体力;
            [SerializeField] private MinMaxVectorInt 背包格;
            [SerializeField] private PentagonConfig[] 随机五维;

            private MinMaxVectorInt Stamina => 体力;
            private MinMaxVectorInt InventorySlot => 背包格;

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
            public (int strength, int agility, int hp, int mp) GeneratePentagon()
            {
                var ran = Pentagon.Generate(ElementArray.OrderByDescending(_ => Random.Range(0, 10)))
                    .ToDictionary(r => r.Item1, r => r.Item2);

                return (ran[PentagonGradeSo.Elements.Strength],
                        ran[PentagonGradeSo.Elements.Agility],
                        ran[PentagonGradeSo.Elements.Hp],
                        ran[PentagonGradeSo.Elements.Mp]
                    );
            }
        }

        [Serializable]
        private class PentagonConfig
        {
            [SerializeField] private PentagonGradeSo 随1;
            [SerializeField] private PentagonGradeSo 随2;
            [SerializeField] private PentagonGradeSo 随3;
            [SerializeField] private PentagonGradeSo 随4;

            private PentagonGradeSo[] Ran => new []{ 随1, 随2, 随3, 随4 };

            public (PentagonGradeSo.Elements, int)[] Generate(IEnumerable<PentagonGradeSo.Elements> array) =>
                array.Select((e, i) => (e, Ran[i].GenerateProp(e))).ToArray();
        }
    }
}