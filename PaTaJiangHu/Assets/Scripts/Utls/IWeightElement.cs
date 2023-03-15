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
        /// 权重计算。如果权重值>0将随机，如果0将会是保底值(不会参与权重),
        /// 多个0权重元素也保持随机属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T WeightPick<T>(this IEnumerable<T> list) where T : IWeightElement =>
            list.WeightTake(1).FirstOrDefault();

        /// <summary>
        /// 权重计算。如果权重值>0将随机，如果0将会是保底值,
        /// 0权重不参与权重计算, 多个0权重元素也保持随机属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public static IEnumerable<T> WeightTake<T>(this IEnumerable<T> list, int take) where T : IWeightElement
        {
            var sortedList = SortWeightElements(list);
            return sortedList.Take(take);
        }

        private static T[] SortWeightElements<T>(IEnumerable<T> list) where T : IWeightElement
        {
            var array = list.ToArray();
            var sum = array.Sum(e => e.Weight);
            return array.Select(e => GetRandomWeightResolveWithZero(e, e.Weight, sum))
                .OrderByDescending(w => w.random).Select(s => s.obj).ToArray();
        }

        private static (T obj, double random) GetRandomWeightResolveWithZero<T>(T o, int weight, double sum) where T : IWeightElement
        {
            if (weight <= 0 || sum <= 0) return (o, Random.Next(100));//0权重将会在百位内(1以上的权重不会有百位)
            var percentage = (weight / sum * 10000d);//大于0以上的权重将根据百分比随机万位数(百位的小数点往后挪2位)
            if (percentage <= 100)
                throw new InvalidOperationException($"权重值异常: 当前权重 ={weight}, 总权重 ={sum}, 而权重百分比={percentage}");
            var ran = Random.Next(100, (int)percentage);
            return (o, ran);
        }
    }
}