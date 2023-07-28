using System;
using System.Collections.Generic;
using AOT.Core;
using GameClient.SoScripts.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "DataCfgSo", menuName = "游戏配置/数据配置")]
    public class DataConfigSo : ScriptableObject
    {
        [SerializeField] private WeaponFieldSo[] 武器;
        [SerializeField] private ArmorFieldSo[] 防具;
        [SerializeField] private ShoesFieldSo[] 鞋子;
        [SerializeField] private DecorationFieldSo[] 挂件;
        [SerializeField] private MedicineFieldSo[] 药品;
        [SerializeField] private BookFieldSo[] 书籍;
        [SerializeField] private AdvItemFieldSo[] 历练道具;
        [FormerlySerializedAs("功能道具")][SerializeField] private ComprehendItemSo[] 领悟道具;

        public WeaponFieldSo[] Weapons => 武器;
        public ArmorFieldSo[] Armors => 防具;
        public ShoesFieldSo[] Shoes => 鞋子;
        public DecorationFieldSo[] Decorations => 挂件;
        public MedicineFieldSo[] Medicines => 药品;
        public BookFieldSo[] Books => 书籍;
        public AdvItemFieldSo[] AdvItems => 历练道具;
        public ComprehendItemSo[] ComprehendItems => 领悟道具;
        public ICollection<IFunctionItem> StoryItems => Array.Empty<IFunctionItem>();//暂时没有故事道具
    }
}