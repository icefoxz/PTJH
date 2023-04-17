using System;

namespace DiziM
{
    /// <summary>
    /// 2维状态值[value/max]
    /// </summary>
    public class StatusValue
    {
        public int Max { get; private set; }
        public int Value { get; private set; }
        public float Ratio => 1f * Value / Max;
        public bool IsExhausted => Value <= 0;

        public StatusValue(int max, int value = -1)
        {
            Max = max;
            Value = value == -1 ? max : value;
        }
        public void Add(int value)
        {
            if (Max <= 0) return;
            Value += value;
            Value = Math.Clamp(Value, 0, Max);
        }

        public int Squeeze(int value)
        {
            if (Value > value)
            {
                Add(-value);
                return value;
            }
            value = Value;
            Value = 0;
            return value;
        }
        public void Set(int value) => Value = value;

        public void SetMax(int max, bool alignValue = true)
        {
            Max = max;
            if (Value > Max && alignValue)
                Value = Max;
        }
        public void AddMax(int value, bool alignValue = true)
        {
            Max += value;
            if (Value > Max && alignValue)
                Value = Max;
        }

        private const char LBracket = '[';
        private const char RBracket = ']';
        public override string ToString() => $"{LBracket}{Value}/{Max}{RBracket}";
    }
}
