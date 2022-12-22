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
            var resolveList = tookList.Concat(sortedList.Except(tookList).Where(s=>s.Weight == lastElement.Weight)).ToArray();
            if (resolveList.Length >= take)
            {
                //避免相同权重的元素仅获取上面的,所以再次打乱;
                return resolveList.OrderByDescending(_ => Sys.Random.Next(0, 100)).Take(take); 
            }
            return resolveList;
        }

        private static T[] SortWeightElements<T>(IEnumerable<T> list) where T : IWeightElement
        {
            return list.Select(ResolveWithZero)
                .OrderByDescending(w => w.weight)
                .Select(s => s.obj).ToArray();
            //var ss = list.Select(ResolveWithZero).OrderByDescending(w => w.weight).ToArray();
            //var rr = ss.Select(s => s.obj).ToArray();
            //return rr;
        }

        private static (T obj, int weight) ResolveWithZero<T>(T w,int randomWeight) where T : IWeightElement =>
            w.Weight > 0 ? (w, Sys.Random.Next(1, w.Weight)) : (w, 0);
    }
}