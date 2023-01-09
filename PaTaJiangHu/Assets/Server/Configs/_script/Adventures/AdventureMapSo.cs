using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.Items;
using UnityEngine;
using Utls;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_历练地图名", menuName = "事件/上层/历练地图")]
    internal class AdventureMapSo : AutoDashNamingObject
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private AdventureMapSo So;

        [SerializeField] private MajorPlaceConfig 固定里数触发配置;
        //[SerializeField] private MinorPlaceConfig 随机触发配置;
        [SerializeField] private int 回程秒数 = 10;

        public int JourneyReturnSec => 回程秒数;
        //private MinorPlaceConfig MinorPlace => 随机触发配置;
        private MajorPlaceConfig MajorPlace => 固定里数触发配置;

        //public int MinorMile => MinorPlace.Mile;
        public IAdvPlace[] PickMajorPlace(int fromMiles, int toMiles) => MajorPlace.GetRandomPlace(fromMiles, toMiles);
        //public IAdvPlace PickMinorPlace() => MinorPlace.GetWeightedPlace();
        public int[] ListMajorMiles() => MajorPlace.GetAllTriggerMiles();
        [Serializable]
        private class MajorPlaceConfig
        {
            [SerializeField] private MilePlace[] 地点配置;
            private MilePlace[] MilePlaces => 地点配置;

            public IAdvPlace[] GetRandomPlace(int fromMile, int toMiles)
            {
                return MilePlaces.Where(p => fromMile < p.Mile && p.Mile <= toMiles)
                    .GroupBy(p => p.Mile)
                    .OrderBy(p => p.Key)
                    .Select(places => (IAdvPlace)places.RandomPick().PlaceSos.RandomPick())
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
    }
}