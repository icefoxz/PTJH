using System;
using System.Linq;

namespace BattleM
{
    public class ConValue : IGameCondition
    {
        public int Max { get; private set; }
        public int Value { get; private set; }
        public int Fix { get; private set; }
        public float MaxRatio => 1f * Max / Fix;
        public float ValueFixRatio => 1f * Value / Fix;
        public float ValueMaxRatio => 1f * Value / Max;
        public bool IsExhausted => Value <= 0;

        public ConValue()
        {
            
        }
        public ConValue(int fix, int max = -1, int value = -1)
        {
            Max = max < 0 ? fix : max;
            Value = value < 0 ? Max : value;
            Fix = fix;
        }

        public void Add(int value)
        {
            if (Max <= 0) return;
            Value += value;
            ClampValue();
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
            if (alignValue) ClampValue();
        }

        public void SetFix(int fix) => Fix = fix;
        public void AddFix(int fix) => Fix += fix;
        public void AddMax(int value, bool alignValue = true)
        {
            Max += value;
            if (alignValue) ClampValue();
        }

        private void ClampValue()
        {
            Value = Math.Clamp(Value, 0, Max);
            Max = Math.Clamp(Max, 0, Fix);
        }

        public void Clone(IConditionValue con)
        {
            Max = con.Max;
            Value = con.Value;
            Fix = con.Fix;
        }

        private const char LBracket = '[';
        private const char RBracket = ']';
        private const char Comma = ',';
        public string Serialize() => $"{LBracket}{Value}{Comma}{Max}{Comma}{Fix}{RBracket}";
        public static ConValue Deserialize(string text)
        {
            var data = text.Trim(LBracket, RBracket).Split(Comma).Select(int.Parse).ToArray();
            return new ConValue(data[2], data[1], data[0]);
        }

        public override string ToString() => $"{LBracket}{Value}/{Max}({Fix}){RBracket}";
    }
}