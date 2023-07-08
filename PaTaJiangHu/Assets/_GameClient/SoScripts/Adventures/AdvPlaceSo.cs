﻿using System;
using System.Linq;
using AOT._AOT.Utls;
using GameClient.SoScripts.Items;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    public interface IAdvPlace
    {
        int Id { get; }
        string Name { get; }
        IAdvStory[] Stories { get; }
        IAdvStory WeighPickStory();
        IAdvStory RandomPickStory();
    }

    /// <summary>
    /// 故事地点, 根据权重从故事池中给出故事
    /// </summary>
    [CreateAssetMenu(fileName = "id_地点名",menuName = "状态玩法/历练/地点")]
    internal class AdvPlaceSo : AutoAtNamingObject, IAdvPlace
    {
        [SerializeField] private AdvStoryWeight[] 故事;

        private AdvStoryWeight[] AdvStories
        {
            get
            {
                if (故事.Length == 0)
                    throw new NotImplementedException($"{nameof(AdvPlaceSo)}.{name}.故事数 = 0!");

                if (故事.Any(s => s.Story == null))
                    throw new NotImplementedException($"{nameof(AdvPlaceSo)}.{name}.故事 = null!");
                return 故事;
            }
        }

        public IAdvStory[] Stories => AdvStories.Select(s => s.Story).ToArray();
        public IAdvStory WeighPickStory() => AdvStories.WeightPick().Story;
        public IAdvStory RandomPickStory() => AdvStories.RandomPick().Story;

        [Serializable] private class AdvStoryWeight : IWeightElement
        {
            [SerializeField] private int 权重 = 1;
            [SerializeField] private AdvStorySo 故事;
            public IAdvStory Story => 故事;
            public int Weight => 权重;
        }
    }
}