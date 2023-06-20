using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace Utls
{
    public static class Sys
    {
        public static Random Random { get; } = new Random(DateTime.Now.Millisecond);
        public static RandomValueGenerator RandomValueGenerator { get; } = new RandomValueGenerator();
        /// <summary>
        /// 运气,获取0-99取1值
        /// </summary>
        public static int Luck => Random.Next(100);
        public static bool RandomBool() => Random.NextDouble() >= 0.5;
        public static int[] RandomElementValue(int elements, int min, int max, int sum)
        {
            return RandomValueGenerator.Generate(elements, min, max, sum);
            //if (elements == 0)
            //    throw new InvalidOperationException("元素不可为0!");
            //if (elements * max < sum)
            //    throw new InvalidOperationException($"元素数量:{elements}, 最大值={max}, 总={elements * max} 低于总数:{sum}!");
            //var array = new int[elements];
            //var minTotal = min * elements;
            //var dynamicValue = sum - minTotal;
            //var index = 0;
            //for (var i = 0; i < array.Length; i++) array[i] = min;
            //var loop = 0;
            //while (dynamicValue != 0)//循环直到把dynamic值消耗殆尽
            //{
            //    var current = array[index];//当前元素值
            //    var maxRan = max - current;//最大随机值
            //    if (maxRan > dynamicValue)//如果当前元素已足够填入最大值, 直接填入并退出循环
            //    {
            //        array[index] += dynamicValue;
            //        break;
            //    }

            //    if (maxRan > 0)//如果最大值大于0才可以赋值
            //    {
            //        var ran = Random.Next(1, dynamicValue < maxRan ? dynamicValue : maxRan);
            //        array[index] += ran;
            //        dynamicValue -= ran;
            //    }

            //    index++;
            //    if (index >= elements) index = 0;
            //    loop++;
            //}

            //return MinValueAligner(array, min, 10);
        }
        private static int[] MinValueAligner(int[] array, int min, int randomValue)
        {
            var ranArray = array.Where(i => i > min).ToList();
            var max = array.Max();
            var minArray = array.Where(i => i <= min).ToArray();
            var alignedValue = 0;
            for (var i = 0; i < minArray.Length; i++)
            {
                alignedValue += randomValue + 1;
                minArray[i] += Random.Next(0, alignedValue);
            }

            for (var i = 0; i < ranArray.Count; i++)
            {
                if (ranArray[i] == max)
                    ranArray[i] -= alignedValue;
            }

            ranArray.AddRange(minArray);
            return ranArray.ToArray();
        }
    }

    public static class UnityDebugExtension
    {
        public static void Log<T>(this T gameObject, string message = null,
            [CallerMemberName] string method = null) where T : Component =>
            Debug.Log($"{gameObject.name}.{method}():{message}");
    }
    public static class Vector2IntExtension
    {
        public static int RandomXYRange(this Vector2Int vector) => Sys.Random.Next(vector.x, vector.y + 1);
    }

    public static class IntExtension
    {
        /// <summary>
        /// 计算百分比
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double Percentage(this double value, double max) => 100d * value / max;
        /// <summary>
        /// 计算百分比
        /// </summary>
        /// <param name="value"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int PercentInt(this int value, int max) => (int)Percentage(value, max);
    }
}