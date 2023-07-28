using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "门派配置", menuName = "门派/配置", order = 1)]
    [Serializable] internal class FactionConfigSo : SerializedScriptableObject
    {
        [SerializeField]private InventoryConstraints 藏宝阁限制;
        [SerializeField][DictionaryDrawerSettings(KeyLabel = "等级",ValueLabel = "弟子人数")]private Dictionary<int, int> LevelMap {get; set; } = new Dictionary<int, int>();
        private InventoryConstraints InventoryConstraint => 藏宝阁限制;

        public Dictionary<ItemType, int> InventoryLimit => new()
        {
            { ItemType.Book, InventoryConstraint.Books },
            { ItemType.Equipment, InventoryConstraint.Equipment },
            { ItemType.FunctionItem, InventoryConstraint.Misc },
        };

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

        // 库存限制
        [Serializable]private class InventoryConstraints
        {
            [SerializeField] private int 装备;
            [SerializeField] private int 秘籍;
            [SerializeField] private int 其它;

            public int Equipment => 装备;
            public int Books => 秘籍;
            public int Misc => 其它;
        }
    }
}