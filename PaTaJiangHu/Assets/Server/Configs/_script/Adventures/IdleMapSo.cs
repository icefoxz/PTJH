﻿using System;
using BattleM;
using System.Collections.Generic;
using MyBox;
using Server.Configs.Items;
using UnityEngine;
using Utls;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_闲置映像表", menuName = "闲置/闲置地图")]
    internal class IdleMapSo : AutoHashNamingObject
    {
        protected bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private IdleMapSo So;
        [SerializeField]private MappingField 映像配置;

        private MappingField Map => 映像配置;

        [Serializable] private class MappingField
        {
            [SerializeField] private int 触发秒数 = 60;
            [SerializeField] private StoryWeight[] 故事配置;

            private int TriggerInSecs => 触发秒数;
            private StoryWeight[] StoryFields => 故事配置;

            [Serializable]
            private class StoryWeight : IWeightElement
            {
                [ConditionalField(true, nameof(ChangeElementName))] [SerializeField] [ReadOnly]
                private string _name;
                [SerializeField] private AdvStorySo 故事;
                [SerializeField] private int 权重;

                private AdvStorySo StorySo => 故事;
                public int Weight => 权重;

                private bool ChangeElementName()
                {
                    var storyName = StorySo?.Name ?? "未配置故事";
                    _name = $"权重:{Weight}, {storyName}";
                    return true;
                }
            }

        }
    }
}