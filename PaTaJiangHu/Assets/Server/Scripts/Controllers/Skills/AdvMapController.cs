using System;
using System.Linq;
using Server.Controllers.Adventures;
using UnityEngine;

public interface IAdvMapController
{
    void LoadMap();
}

public class AdvMapController : IAdvMapController
{
    private Configure Config { get; }

    internal AdvMapController(Configure config)
    {
        Config = config;
    }

    private AdvMapSo SelectedSo { get; set; }
    public void LoadMap()
    {
        SelectedSo = Config.MapSo;
        var map = new Map(SelectedSo);
        Game.MessagingManager.Send(EventString.Test_AdvMapLoad, map);
    }

    public class Map
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SoName { get; set; }
        public Place[] Places { get; set; }
        public Path[] Paths { get; set; }

        public Map(){ }
        internal Map(AdvMapSo so)
        {
            Name = so.Name;
            SoName = so.name;
            Id = so.Id;
            var placeMap = so.Places.Select((place, index) => (place, new Place(place, index)))
                .ToDictionary(p => p.place, p => p.Item2);
            Paths = so.Paths
                .Select((path, index) => new Path(path, index, placeMap[path.Place].Index, placeMap[path.To].Index))
                .ToArray();
            Places = placeMap.Select(m => m.Value).ToArray();
        }
    }
    public class Path
    {
        public int Index { get; set; }
        public int EventMax { get; set; }
        public int EventMin { get; set; }
        public int UnlockLevel { get; set; }
        public int ExLingCost { get; set; }
        public int PlaceIndex { get; set; }
        public int ToIndex { get; set; }
        public Path() { }
        public Path(IAdvPath p, int index, int placeIndex, int toIndex)
        {
            UnlockLevel = p.ExLingCost;
            ExLingCost = p.ExLingCost;
            EventMin = p.EventRange.Min;
            EventMax = p.EventRange.Max;
            Index = index;
            PlaceIndex = placeIndex;
            ToIndex = toIndex;
        }
    }
    public class Place
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Stories { get; set; }

        public Place() { }
        public Place(IAdvPlace p, int index)
        {
            Name = p.Name;
            Index = index;
            Stories = p.Stories.Length;
        }
    }

    [Serializable] internal class Configure
    {
        [SerializeField] private AdvMapSo 地图;

        public AdvMapSo MapSo => 地图;
    }
}