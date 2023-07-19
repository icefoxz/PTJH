using System.Collections.Generic;
using System.Linq;

namespace AOT.Utls
{
    public static class GameLinqExtension
    {
        public static T RandomPick<T>(this IEnumerable<T> enumerable, int randomValue = 100) =>
            RandomTake(enumerable, 1, randomValue).First();

        public static T[] RandomTake<T>(this IEnumerable<T> enumerable, int take, int randomValue = 100)
        {
            var array = enumerable.Select(e => (Sys.Random.Next(randomValue), e)).OrderByDescending(e => e.Item1).ToArray();
            var tookList = array.Take(take).ToArray();
            var lastElement = tookList.Last();
            var resolveList = tookList.Concat(array.Except(tookList)
                .Where(s => s.Item1 == lastElement.Item1)).Select(e => e.e).ToArray();
            if (resolveList.Length >= take)
            {
                //避免相同权重的元素仅获取上面的,所以再次打乱;
                return resolveList.OrderByDescending(_ => Sys.Random.Next(0, randomValue)).Take(take).ToArray();
            }
            return resolveList;
        }
    }
}
