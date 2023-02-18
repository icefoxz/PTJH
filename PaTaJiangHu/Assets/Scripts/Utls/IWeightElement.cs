using System;
using System.Collections.Generic;
using System.Linq;

namespace Utls
{
    public interface IWeightElement
    {
        int Weight { get; }
    }

    public static class WeightElementExtension
    {
        private static Random Random = new Random();
        /// <summary>
        /// 权重计算。如果权重值>0将随机，如果0将会是保底值(不会参与权重)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T WeightPick<T>(this IEnumerable<T> list) where T : IWeightElement =>
            list.WeightTake(1).FirstOrDefault();

        /// <summary>
        /// 权重计算。如果权重值>0将随机，如果0将会是保底值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static IEnumerable<T> WeightTake<T>(this IEnumerable<T> list, int take) where T : IWeightElement
        {
            var sortedList = SortWeightElements(list);
            var tookList = sortedList.Take(take).ToArray();
            var lastElement = tookList.Last();
            var resolveList = tookList.Concat(sortedList.Except(tookList).Where(s => s.Weight == lastElement.Weight)).ToArray();
            if (resolveList.Length >= take)
            {
                //避免相同权重的元素仅获取上面的,所以再次打乱;
                return resolveList.OrderByDescending(_ => Random.Next(0, 100)).Take(take);
            }
            return resolveList;
        }

        private static T[] SortWeightElements<T>(IEnumerable<T> list) where T : IWeightElement
        {
            var array = list.ToArray();
            var sum = array.Sum(e => e.Weight);
            return array.Select(e => ResolveWithZero(e, e.Weight, sum))
                .OrderByDescending(w => w.random).Select(s => s.obj).ToArray();
            /*var resolved = array.Select(e => ResolveWithZero(e, e.Weight, sum)).ToArray();
            //Print(resolved);
            var sorted = resolved
                .OrderByDescending(w => w.random).ToArray();
            //Console.WriteLine("********Sorted**********");
            //Print(sorted);
            return sorted.Select(s => s.obj).ToArray();
            //var ss = list.Select(ResolveWithZero).OrderByDescending(w => w.weight).ToArray();
            //var rr = ss.Select(s => s.obj).ToArray();
            //return rr;
            */
        }

        //private static void Print<T>((T obj, double random)[] resolve) where T : IWeightElement
        //{
        //    foreach (var (obj, random) in resolve) Console.WriteLine($"w={obj.Weight}, r={random}");
        //}

        private static (T obj, double random) ResolveWithZero<T>(T o, int weight, double sum) where T : IWeightElement
        {
            var percentage = (weight / sum * 100d);
            var ran = Random.Next(1, (int)percentage);
            //Console.WriteLine($"p={percentage}, r={ran}");
            return o.Weight > 0 ? (o, ran) : (o, 0);
        }
    }
}