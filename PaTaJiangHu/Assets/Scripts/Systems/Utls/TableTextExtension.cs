using System.Linq;

namespace Systems.Utls
{
    public static class TableTextExtension
    {
        public static int ToInt(this string text) => int.Parse(text);
        public static int[] ToInts(this string text, char split = ',') =>
            text.Split(split).Where(s => !string.IsNullOrWhiteSpace(s)).Select(int.Parse).ToArray();
        public static bool ToBool(this string text, string compare) => text.Equals(compare);
        public static bool ToBool(this string text, bool zeroIsTrue = false)
        {
            if (zeroIsTrue) return text.ToInt() == 0;
            return text.ToInt() > 0;
        }
    }
}