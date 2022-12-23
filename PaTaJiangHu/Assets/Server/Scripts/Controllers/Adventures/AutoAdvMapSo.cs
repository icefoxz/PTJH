using System;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_历练地图名", menuName = "历练/地图")]
    internal class AutoAdvMapSo : ScriptableObject
    {
        [SerializeField] private MajorPlaceConfig 固定里数触发配置;
        [SerializeField] private MinorPlaceConfig 随机触发配置;
        [SerializeField] private int 回程秒数 = 10;

        public int JourneyReturnSec => 回程秒数;
        private MinorPlaceConfig MinorPlace => 随机触发配置;
        private MajorPlaceConfig MajorPlace => 固定里数触发配置;

        public int MajorMile => MajorPlace.Mile;
        public int MinorMile => MinorPlace.Mile;
        public IAdvPlace PickMajorPlace() => MajorPlace.GetRandomPlace();
        public IAdvPlace PickMinorPlace() => MinorPlace.GetWeightedPlace();

        [Serializable] private class MajorPlaceConfig
        {
            [SerializeField] private int 里;
            [SerializeField] private AdvPlaceSo[] 地点;

            public int Mile => 里;
            public AdvPlaceSo[] PlaceSos => 地点;
            public AdvPlaceSo GetRandomPlace() => PlaceSos.RandomPick();
        }
        [Serializable] private class MinorPlaceConfig
        {
            [SerializeField] private readonly int 里;
            [SerializeField] private PlaceWeight[] 地点;

            public int Mile => 里;
            private PlaceWeight[] PlaceSos => 地点;
            public AdvPlaceSo GetWeightedPlace() => PlaceSos.WeightPick().PlaceSo;

            [Serializable] private class PlaceWeight : IWeightElement
            {
                [SerializeField] private int 权重;
                private readonly AdvPlaceSo 地点;

                int IWeightElement.Weight => 权重;
                public AdvPlaceSo PlaceSo => 地点;
            }
        }
    }
}