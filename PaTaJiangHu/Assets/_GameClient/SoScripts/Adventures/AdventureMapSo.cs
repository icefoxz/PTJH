﻿using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Utls;
using GameClient.Models;
using GameClient.SoScripts.Items;
using MyBox;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    public interface IAutoAdvMap
    {
        AdvActivityTypes ActivityType { get; }
        int FixReturnSec { get; }
        bool IsFixReturnTime { get; }
        int ActionLingCost { get; }
        Sprite Image { get; }
        string About { get; }
        string Name { get; }
        int Id { get; }
        bool PossibleLost(ITerm term);
    }

    internal abstract class AdventureMapSoBase : AutoHashNamingObject, IAutoAdvMap
    {
        [SerializeField] private int 执行令消耗;
        [SerializeField] private LostStrategySo 失踪策略;
        [SerializeField] private MajorPlaceConfig 固定里数触发配置;
        [SerializeField] private MinorPlaceConfig 小故事;
        [SerializeField] protected bool 固定回程秒数;
        [ConditionalField(nameof(固定回程秒数))][SerializeField] private int 回程秒数 = 10;
        [SerializeField] private Sprite 图片;
        [SerializeField] [TextArea]private string 说明;
        public LostStrategySo LostStrategy => 失踪策略;
        public bool IsFixReturnTime => 固定回程秒数;
        public abstract AdvActivityTypes ActivityType { get; }
        public int FixReturnSec => 回程秒数;
        private MajorPlaceConfig MajorPlace => 固定里数触发配置;
        private MinorPlaceConfig MinorPlace => 小故事;
        public int ActionLingCost => 执行令消耗;
        public Sprite Image => 图片;
        public string About => 说明;
        public int MaxMiles => MajorPlace.MaxMiles;
        public bool PossibleLost(ITerm term) => LostStrategy.IsInTerm(term);

        public IAdvPlace[] PickMajorPlace(int fromMiles, int toMiles) => fromMiles != toMiles
            ? MajorPlace.GetRandomPlace(fromMiles, toMiles).Select(m => m.place).ToArray()
            : Array.Empty<IAdvPlace>();

        public IAdvPlace[] PickAllTriggerPlaces(int fromMiles, int toMiles)
        {
            var majorMap = MajorPlace.GetRandomPlace(fromMiles, toMiles);
            //如果from == to表示完全没走过,所以产出地方(还在原地)
            if (fromMiles != toMiles)
            {
                var minorList = MinorPlace.GetAllTriggerMiles(fromMiles, toMiles)
                    .Except(majorMap.Select(m => m.mile))
                    .ToArray();
                var minorMap = MinorPlace.GetRandomPlace(minorList);
                return majorMap.Concat(minorMap).OrderBy(m => m.mile).Select(m => m.place).ToArray();
            }
            return Array.Empty<IAdvPlace>();
        }

        /// <summary>
        /// 列出所有大故事的里数
        /// </summary>
        /// <returns></returns>
        public int[] ListMajorMiles() => MajorPlace.GetAllTriggerMiles();

        /// <summary>
        /// 列出所有小故事的里数
        /// </summary>
        /// <param name="fromMiles"></param>
        /// <param name="toMiles"></param>
        /// <returns></returns>
        public int[] ListMinorMiles(int fromMiles, int toMiles) => MinorPlace.GetAllTriggerMiles(fromMiles, toMiles);

        /// <summary>
        /// 列出所有小故事的里数
        /// </summary>
        /// <returns></returns>
        public int[] ListMinorMiles() => MinorPlace.GetAllTriggerMiles();

        [Serializable]
        protected class MajorPlaceConfig
        {
            [SerializeField] private MilePlace[] 地点配置;
            private MilePlace[] MilePlaces => 地点配置;
            public int MaxMiles => MilePlaces.Max(m => m.Mile);
            public (int mile, IAdvPlace place)[] GetRandomPlace(int fromMile, int toMiles)
            {
                return MilePlaces.Where(p => fromMile < p.Mile && p.Mile <= toMiles)
                    .GroupBy(p => p.Mile)
                    .OrderBy(p => p.Key)
                    .Select(places => (places.Key, (IAdvPlace)places.RandomPick<MilePlace>().PlaceSos.RandomPick()))
                    .ToArray();
            }

            public int[] GetAllTriggerMiles() => MilePlaces.Select(o => o.Mile).ToArray();

            [Serializable]
            private class MilePlace
            {
                [SerializeField] private int 里;
                [SerializeField] private AdvPlaceSo[] 地点;
                public int Mile => 里;
                public AdvPlaceSo[] PlaceSos
                {
                    get
                    {
                        if (地点.Any(p => p == null))
                            throw new NotImplementedException($"{nameof(AdventureMapSo)}.地点=null!");
                        return 地点;
                    }
                }
            }

        }

        [Serializable]
        protected class MinorPlaceConfig
        {
            [SerializeField] private MilePlace[] 地点配置;
            private MilePlace[] MilePlaces => 地点配置;

            public (int mile, IAdvPlace place)[] GetRandomPlace(int[] mileList) =>
                mileList.Select(m => (m, (IAdvPlace)MilePlaces
                        .Where(p => p.IsInRange(m))
                        .RandomPick().PlaceSo))
                    .ToArray();

            public int[] GetAllTriggerMiles(int fromMiles, int toMiles) =>
                MilePlaces.SelectMany(m => m.GetAllTriggerMiles(fromMiles, toMiles)).Distinct().ToArray();
            public int[] GetAllTriggerMiles() =>
                MilePlaces.SelectMany(m => m.GetAllTriggerMiles()).Distinct().ToArray();

            [Serializable]
            private class MilePlace
            {
                private bool RenameElement()
                {
                    _name = PlaceSo == null ? string.Empty : $"{Mile.x}~{Mile.y}: {PlaceSo.Name}";
                    return true;
                }

                [ConditionalField(true, nameof(RenameElement))][SerializeField][ReadOnly] private string _name;
                [SerializeField] private Vector2Int 里;
                [SerializeField] private AdvPlaceSo 地点;
                [SerializeField] private int 里数间隔;
                public Vector2Int Mile
                {
                    get
                    {
                        if (里.y < 里.x)
                            throw new NotImplementedException("注意,当前里x数大于y!");
                        return 里;
                    }
                }

                public int MileInterval => 里数间隔;
                public AdvPlaceSo PlaceSo => 地点;
                
                // 是否在触发范围内
                public bool IsInRange(int miles)=> miles >= Mile.x && miles <= Mile.y;
                
                // 算出所有触发里数
                public int[] GetAllTriggerMiles(int fromMiles = 0, int toMiles = -1)
                {
                    var x = Mile.x;
                    var y = Mile.y;
                    var interval = MileInterval;
                    var miles = new List<int>();
                    var current = x;
                    while (current < y)
                    {
                        if (current > fromMiles)
                            if (toMiles == -1 || current <= toMiles)
                                miles.Add(current);

                        current += interval;
                    }

                    return miles.ToArray();
                }
            }
        }
    }

    [CreateAssetMenu(fileName = "id_历练地图名", menuName = "状态玩法/历练/历练地图")]
    internal class AdventureMapSo : AdventureMapSoBase
    {
        //[SerializeField] private MinorPlaceConfig 随机触发配置;

        //private MinorPlaceConfig MinorPlace => 随机触发配置;

        //public int MinorMile => MinorPlace.Mile;

        //[Serializable] private class MinorPlaceConfig
        //{
        //    [SerializeField] private readonly int 里;
        //    [SerializeField] private PlaceWeight[] 地点;

        //    public int Mile => 里;
        //    private PlaceWeight[] PlaceSos => 地点;
        //    public AdvPlaceSo GetWeightedPlace() => PlaceSos.WeightPick().PlaceSo;

        //    [Serializable] private class PlaceWeight : IWeightElement
        //    {
        //        [SerializeField] private int 权重;
        //        [SerializeField] private AdvPlaceSo 地点;

        //        int IWeightElement.Weight => 权重;
        //        public AdvPlaceSo PlaceSo => 地点;
        //    }
        //}
        public override AdvActivityTypes ActivityType { get; } = AdvActivityTypes.Adventure;
    }
}