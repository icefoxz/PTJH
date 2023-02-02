using System;
using MyBox;
using UnityEngine;

namespace Server.Configs.SoUtls
{
    [Serializable]
    internal class ValueMapping<T>
    {
        private bool AutoName()
        {
            _name = $"{From}%~{To}%:{Value}";
            return true;
        }

        [ConditionalField(true, nameof(AutoName))] [ReadOnly] [SerializeField]
        private string _name;

        [SerializeField] private int 从;
        [SerializeField] private int 至;
        [SerializeField] private T 值;

        private int From => 从;
        private int To => 至;
        public T Value => 值;

        public bool IsInCondition(int value) => value >= From && value <= To;
    }
}