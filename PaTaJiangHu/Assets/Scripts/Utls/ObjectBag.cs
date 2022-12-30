using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using ILRuntime.Reflection;

namespace Utls
{
    public class ObjectBag
    {
        private static Type Stringtype => typeof(string);
        public string[] Bag { get; }
        public string Data { get; }
        public override string ToString() => Data;

        public ObjectBag()
        {
            
        }

        public ObjectBag(string dataText)
        {
            Data = dataText;
            Bag = string.IsNullOrWhiteSpace(dataText) ? Array.Empty<string>() : dataText.Split('␇');
        }

        public static string Serialize(params object[] objs)
        {
            var bag = new string[objs.Length];
            for (var index = 0; index < objs.Length; index++)
            {
                var o = objs[index];
                if (o is string s)
                {
                    bag[index] = s;
                    continue;
                }
                if(o.GetType().IsClass)
                {
                    bag[index] = Json.Serialize(o);
                    continue;
                }
                bag[index] = o switch
                {
                    int i => i.ToString(),
                    bool b => b ? 1.ToString(): 0.ToString(),
                    long l => l.ToString(),
                    float f => f.ToString(CultureInfo.InvariantCulture),
                    char c => c.ToString(),
                    double d=> d.ToString(CultureInfo.InvariantCulture),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            return string.Join('␇', bag);
        }

        public static ObjectBag DeSerialize(string dataText) => new(dataText);

        public int GetInt(int index) => int.Parse(GetValue(index));
        public float GetFloat(int index) => float.Parse(GetValue(index));
        public double GetDouble(int index) => double.Parse(GetValue(index));
        public char GetChar(int index) => char.Parse(GetValue(index));
        public long GetLong(int index) => long.Parse(GetValue(index));
        public bool GetBool(int index) => GetInt(index) == 1;
        public T Get<T>(int index) where T : class
        {
            var value = GetValue(index);
            var type = typeof(T);
            if (type == Stringtype) return value as T;
            var result = Json.Deserialize<T>(value);
            if (result == null) XDebug.LogWarning($"ObjectBag.Get: {value}\n Convert is null!");
            return result;
        }
        private string GetValue(int index) => Bag[index];
    }
}
