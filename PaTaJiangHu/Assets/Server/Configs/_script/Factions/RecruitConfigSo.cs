using System;
using UnityEngine;

namespace Server.Configs.Factions
{
    [CreateAssetMenu(fileName = "RecruitConfig", menuName = "弟子/招募配置")]
    internal class RecruitConfigSo : ScriptableObject
    {
        [SerializeField] private RecruitCost 招募成本;

        private RecruitCost Cost => 招募成本;

        public (int silver, int yuanBao) GetCost() => (Cost.Silver, Cost.YuanBao);
        [Serializable] private class RecruitCost
        {
            [SerializeField] private int 元宝;
            [SerializeField] private int 银两;

            public int YuanBao => 元宝;
            public int Silver => 银两;
        }
    }
}