using System;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Configs._script.Adventures
{
    public interface IAdvPlace
    {
        int Id { get; }
        string Name { get; }
        IAdvInterStory[] Stories { get; }
        IAdvInterStory GetStory();
    }

    /// <summary>
    /// 副本地图
    /// </summary>
    [CreateAssetMenu(fileName = "id_地点名",menuName = "副本/地点")]
    internal class AdvPlaceSo : ScriptableObject, IAdvPlace
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private AdvStoryWeight[] 故事;

        private AdvStoryWeight[] AdvStories => 故事;
        public int Id => _id;
        public string Name => _name;
        public IAdvInterStory[] Stories => AdvStories.Select(s => s.Story).ToArray();
        public IAdvInterStory GetStory() => AdvStories.WeightPick().Story;

        [Serializable] private class AdvStoryWeight : IWeightElement
        {
            [SerializeField] private int 权重 = 0;
            [SerializeField] private AdvInterStorySo 故事;
            public IAdvInterStory Story => 故事;
            public int Weight => 权重;
        }
    }
}