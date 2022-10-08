using System;
using System.Collections.Generic;
using System.Linq;

namespace Utls
{
    public static class GameLinqExtension
    {
        public static T RandomPick<T>(this IEnumerable<T> enumerable, bool allowDefault, int randomValue = 100)
        {
            var array = enumerable.ToArray();
            if (!allowDefault && array.Length == 0)
                throw new InvalidOperationException($"{nameof(RandomPick)}: array is null or empty!");
            var obj = array.OrderByDescending(_ => Sys.Random.Next(randomValue)).FirstOrDefault();
            return obj;
        }

        public static T RandomPick<T>(this IEnumerable<T> enumerable, int randomValue = 100) =>
            RandomPick(enumerable, false, randomValue);

        public static T[] RandomTake<T>(this IEnumerable<T> enumerable, int take, int randomValue = 100) =>
            enumerable.OrderByDescending(_ => Sys.Random.Next(randomValue)).Take(take).ToArray();
    }
}
