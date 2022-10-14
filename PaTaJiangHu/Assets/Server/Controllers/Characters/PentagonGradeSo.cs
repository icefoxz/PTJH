using System;
using UnityEngine;

namespace Server.Controllers.Characters
{
    [CreateAssetMenu(fileName = "PentagonGrade", menuName = "配置/五维配置")]
    public class PentagonGradeSo : ScriptableObject
    {
        public enum Elements
        {
            Strength,
            Agility,
            Hp,
            Tp,
            Mp,
        }
        [SerializeField] private MinMaxVectorInt 血;
        [SerializeField] private MinMaxVectorInt 气;
        [SerializeField] private MinMaxVectorInt 内;
        [SerializeField] private MinMaxVectorInt 力量;
        [SerializeField] private MinMaxVectorInt 敏捷;
        private MinMaxVectorInt Strength => 力量;
        private MinMaxVectorInt Agility => 敏捷;
        private MinMaxVectorInt Hp => 血;
        private MinMaxVectorInt Tp => 气;
        private MinMaxVectorInt Mp => 内;
        public int GenerateProp(Elements element)
        {
            return element switch
            {
                Elements.Strength => Strength.Generate(),
                Elements.Agility => Agility.Generate(),
                Elements.Hp => Hp.Generate(),
                Elements.Tp => Tp.Generate(),
                Elements.Mp => Mp.Generate(),
                _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
            };
        }

    }
}