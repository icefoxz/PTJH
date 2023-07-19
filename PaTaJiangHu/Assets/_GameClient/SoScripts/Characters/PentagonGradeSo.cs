using System;
using AOT.Core.Dizi;
using GameClient.Modules.DiziM;
using UnityEngine;

namespace GameClient.SoScripts.Characters
{
    [CreateAssetMenu(fileName = "PentagonGrade", menuName = "弟子/产出/五维配置")]
    public class PentagonGradeSo : ScriptableObject
    {
        public enum Elements
        {
            Strength,
            Agility,
            Hp,
            Mp,
        }

        [SerializeField] private DiziGrades _grade;
        [SerializeField] private MinMaxVectorInt 血;
        [SerializeField] private MinMaxVectorInt 内;
        [SerializeField] private MinMaxVectorInt 力量;
        [SerializeField] private MinMaxVectorInt 敏捷;
        private MinMaxVectorInt Strength => 力量;
        private MinMaxVectorInt Agility => 敏捷;
        private MinMaxVectorInt Hp => 血;
        private MinMaxVectorInt Mp => 内;
        private DiziGrades Grade => _grade;
        public (int, DiziGrades) GenerateProp(Elements element)
        {
            return element switch
            {
                Elements.Strength => (Strength.Generate(), Grade),
                Elements.Agility => (Agility.Generate(), Grade),
                Elements.Hp => (Hp.Generate(), Grade),
                Elements.Mp => (Mp.Generate(), Grade),
                _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
            };
        }

    }
}