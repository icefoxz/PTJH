using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Utls
{
    public static class GameLinqExtension
    {
        public static T RandomPick<T>(this IEnumerable<T> enumerable, bool allowDefault, int randomValue = 100, [CallerMemberName]string methodName = null)
        {
            var array = enumerable.ToArray();
            if (!allowDefault && array.Length == 0)
                throw new InvalidOperationException($"{methodName}.{nameof(RandomPick)}: array is null or empty!");
            var obj = array.OrderByDescending(_ => Sys.Random.Next(randomValue)).FirstOrDefault();
            return obj;
        }

        public static T RandomPick<T>(this IEnumerable<T> enumerable, int randomValue = 100) =>
            RandomPick(enumerable, false, randomValue);

        public static T[] RandomTake<T>(this IEnumerable<T> enumerable, int take, int randomValue = 100) =>
            enumerable.OrderByDescending(_ => Sys.Random.Next(randomValue)).Take(take).ToArray();
    }
}
