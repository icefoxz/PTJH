using System;
using System.Linq;

namespace GameClient.Modules.DiziM
{
    public class ConValue : IGameCondition
    {
        public int Max { get; private set; }
        public int Value { get; private set; }
        public int Base { get; private set; }
        public double MaxRatio => 1d * Max / Base;
        public double ValueBaseRatio => 1d * Value / Base;
        public double ValueMaxRatio => 1d * Value / Max;
        public bool IsExhausted => Value <= 0;

        public ConValue()
        {
            
        }
        public ConValue(int fix, int max, int value)
        {
            Max = max < 0 ? fix : max;
            Value = value < 0 ? Max : value;
            Base = fix;
        }

        public ConValue(int max) : this(max, max, max)
        {

        }

        public ConValue(int max, int value) : this(max, max, value)
        {
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

        public void SetBase(int fix) => Base = fix;
        public void AddFix(int fix) => Base += fix;
        public void AddMax(int value, bool alignValue = true)
        {
            Max += value;
            if (alignValue) ClampValue();
        }

        private void ClampValue()
        {
            Max = Math.Clamp(Max, 0, Base);//先锁定最大值
            Value = Math.Clamp(Value, 0, Max);//后锁定状态值
        }

        public void Clone(IConditionValue con)
        {
            Max = con.Max;
            Value = con.Value;
            Base = con.Base;
        }

        private const char LBracket = '[';
        private const char RBracket = ']';
        private const char Comma = ',';
        public string Serialize() => $"{LBracket}{Value}{Comma}{Max}{Comma}{Base}{RBracket}";
        public static ConValue Deserialize(string text)
        {
            var data = text.Trim(LBracket, RBracket).Split(Comma).Select(int.Parse).ToArray();
            return new ConValue(data[2], data[1], data[0]);
        }

        public override string ToString() => $"{LBracket}{Value}/{Max}({Base}){RBracket}";
    }
}