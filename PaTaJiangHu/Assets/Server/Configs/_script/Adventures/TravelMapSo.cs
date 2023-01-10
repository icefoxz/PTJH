using System;
using System.Text;
using MyBox;
using UnityEngine;
using Utls;

namespace Server.Configs.Adventures
{
    public interface IAdvMap
    {
        int Id { get; }
        string Name { get; }
        IAdvPlace[] Places { get; }
        IAdvPath[] Paths { get; }
    }

    public interface IAdvPath
    {
        IAdvPlace Place { get; }
        IAdvPlace To { get; }
        int ExLingCost { get; }
        int StaminaCost { get; }
        int UnlockLevel { get; }
        IMinMax EventRange { get; }
    }

    [CreateAssetMenu(fileName = "id_副本地图名", menuName = "副本/副本地图")]
    internal class TravelMapSo : ScriptableObject, IAdvMap
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private AdvPlaceSo[] 地点;
        [SerializeField] private AdvPath[] 路径;

        public int Id => _id;
        public string Name => _name;
        public IAdvPlace[] Places => 地点;
        public IAdvPath[] Paths => 路径;

        /// <summary>
        /// 路径
        /// </summary>
        [Serializable]private class AdvPath : IAdvPath
        {
            private bool SetElementName()
            {
                if (Place != null)
                {
                    var to = To == null ? string.Empty : To.Name;
                    var content = new StringBuilder();
                    if (ExLingCost != default) content.Append($"令({ExLingCost})");
                    if (StaminaCost!= default) content.Append($"体({StaminaCost})");
                    if (UnlockLevel!= default) content.Append($"等({UnlockLevel})");
                    if (_eventCount is not { Min: 0, Max: 0 }) content.Append($"事件[{故事数.Min}-{故事数.Max}]");
                    _name = $"{Place.Name}->{to}[{content}]";
                }
                else
                {
                    _name = string.Empty;
                }

                return true;
            }

            [ConditionalField(true,nameof(SetElementName))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private AdvPlaceSo 地点;
            [SerializeField] private AdvPlaceSo 目标点;
            [SerializeField] private int 消耗行动令;
            [SerializeField] private int 消耗体力;
            [SerializeField] private int 解锁等级;
            [SerializeField] private MinMaxInt 故事数;
            private MinMaxInt _eventCount => 故事数;

            public IAdvPlace Place => 地点;
            public IAdvPlace To => 目标点;
            public int ExLingCost => 消耗行动令;
            public int StaminaCost => 消耗体力;
            public int UnlockLevel => 解锁等级;
            public IMinMax EventRange => new MinMax(故事数);
        }
        [Serializable]private class MinMax : IMinMax
        {
            [SerializeField] private int _min;
            [SerializeField] private int _max;

            public int Min => _min;
            public int Max => _max;

            public MinMax(MinMaxInt m)
            {
                _min = m.Min;
                _max = m.Max;
            }
        }
    }
}