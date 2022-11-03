using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Controllers.Characters
{
    [CreateAssetMenu(fileName = "GradeConfig", menuName = "配置/资质配置")]
    internal class GradeConfigSo : ScriptableObject
    {
        public enum Grades
        {
            D,
            C,
            B,
            A,
            S,
        }

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

        public (int strength, int agility, int hp, int mp, int Stamina, int inventorySlot)
            GenerateFromGrade(int grade)
        {
            return GeneratePops(GradeConfigs((Grades)grade));

            (int strength, int agility, int hp, int mp, int Stamina, int inventorySlot) GeneratePops(
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

            public (int strength, int agility, int hp, int mp) GeneratePentagon()
            {
                return (Pentagon.Generate(PentagonGradeSo.Elements.Strength),
                    Pentagon.Generate(PentagonGradeSo.Elements.Agility),
                    Pentagon.Generate(PentagonGradeSo.Elements.Hp),
                    Pentagon.Generate(PentagonGradeSo.Elements.Mp));
            }
        }

        [Serializable]
        private class PentagonConfig
        {
            [SerializeField] private PentagonGradeSo 力;
            [SerializeField] private PentagonGradeSo 敏;
            [SerializeField] private PentagonGradeSo 血;
            [SerializeField] private PentagonGradeSo 内;

            private PentagonGradeSo Strength => 力;
            private PentagonGradeSo Agility => 敏;
            private PentagonGradeSo Hp => 血;
            private PentagonGradeSo Mp => 内;

            public int Generate(PentagonGradeSo.Elements element)
            {
                return element switch
                {
                    PentagonGradeSo.Elements.Strength => Strength.GenerateProp(element),
                    PentagonGradeSo.Elements.Agility => Agility.GenerateProp(element),
                    PentagonGradeSo.Elements.Hp => Hp.GenerateProp(element),
                    PentagonGradeSo.Elements.Mp => Mp.GenerateProp(element),
                    _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
                };
            }
        }
    }
}