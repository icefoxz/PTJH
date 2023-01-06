using System;
using MyBox;
using UnityEngine;

namespace Server.Configs.Skills
{
    [Serializable] internal class SoValueConfig
    {
        public enum Modes
        {
            [InspectorName("无")] None,
            [InspectorName("比率")] Ratio,
            [InspectorName("替换")] Replace,
            [InspectorName("增值")] AddOn,
        }

        [SerializeField] private Modes 模式;

        [ConditionalField(nameof(模式), true, Modes.None)] [SerializeField]
        private float _value = 1;

        private Modes Mode => 模式;
        private float Value => _value;

        public int GetValue(int baseValue) => Mode switch
        {
            Modes.None => baseValue,
            Modes.Ratio => (int)(Value * baseValue),
            Modes.Replace => (int)Value,
            Modes.AddOn => (int)(Value + baseValue),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}