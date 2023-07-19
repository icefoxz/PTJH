using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "门派配置", menuName = "门派/配置", order = 1)]
    [Serializable] internal class FactionConfigSo : SerializedScriptableObject
    {
        [SerializeField][DictionaryDrawerSettings(KeyLabel = "等级",ValueLabel = "弟子人数")]private Dictionary<int, int> LevelMap {get; set; } = new Dictionary<int, int>();

        public int GetDiziLimit(int level)
        {
            var factionLevel = LevelMap?.FirstOrDefault(x => x.Key <= level)?? throw new NullReferenceException($"门派配置中没有找到等级{level}的配置");
            return factionLevel.Value;
        }
        private class FactionLevel
        {
            public int Level;
            public int Dizi;
        }
    }
}