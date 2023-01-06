using System;
using System.Linq;
using Server.Configs.Items;
using UnityEngine;
using Utls;

namespace Server.Configs.Adventures
{
    public interface IAdvPlace
    {
        int Id { get; }
        string Name { get; }
        IAdvStory[] Stories { get; }
        IAdvStory GetStory();
    }

    /// <summary>
    /// 故事地点, 根据权重从故事池中给出故事
    /// </summary>
    [CreateAssetMenu(fileName = "id_地点名",menuName = "事件/上层/地点")]
    internal class AdvPlaceSo : AutoDashNamingObject, IAdvPlace
    {
        [SerializeField] private AdvStoryWeight[] 故事;

        private AdvStoryWeight[] AdvStories => 故事;
        public IAdvStory[] Stories => AdvStories.Select(s => s.Story).ToArray();
        public IAdvStory GetStory() => AdvStories.WeightPick().Story;

        [Serializable] private class AdvStoryWeight : IWeightElement
        {
            [SerializeField] private int 权重 = 0;
            [SerializeField] private AdvStorySo 故事;
            public IAdvStory Story => 故事;
            public int Weight => 权重;
        }
    }
}