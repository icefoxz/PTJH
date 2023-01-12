using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = System.Random;

namespace Utls
{
    public static class Sys
    {
        public static Random Random { get; } = new Random(DateTime.Now.Millisecond);
        public static bool RandomBool() => Random.NextDouble() >= 0.5;
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