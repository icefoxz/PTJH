using System;
using System.Linq;
using AOT.Core.Dizi;
using AOT.Utls;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts.Factions
{
    [CreateAssetMenu(fileName = "RecruitConfig", menuName = "弟子/招募配置")]
    internal class RecruitConfigSo : ScriptableObject
    {
        [SerializeField] private RecruitCost 招募成本;
        [SerializeField] private VisitorConfig 来访者配置;
        private RecruitCost Cost => 招募成本;
        private VisitorConfig VisitorCfg => 来访者配置;
        public int VisitorIntervalMins => VisitorCfg.VisitorIntervalMins;

        public (int silver, int yuanBao) GetCost() => (Cost.Silver, Cost.YuanBao);
        public IVisitorDizi GetRandomVisitorDizi()=> VisitorCfg.GetRandomVisitorDizi();
        public IVisitorDizi GetVisitorDiziByGrade(ColorGrade grade) => VisitorCfg.GetVisitorDiziByGrade(grade);
        public int GetVisitorBuyCost(ColorGrade grade) => VisitorCfg.GetVisitorBuyCost(grade);
        public int GetVisitorBuyCost(int grade) => GetVisitorBuyCost((ColorGrade)grade);
        public int GetVisitorSellCost(ColorGrade grade) => VisitorCfg.GetVisitorSellRandomCost(grade);
        public int GetVisitorSellCost(int grade) => GetVisitorSellCost((ColorGrade)grade);

        [Serializable] private class RecruitCost
        {
            [SerializeField] private int 元宝;
            [SerializeField] private int 银两;

            public int YuanBao => 元宝;
            public int Silver => 银两;
        }

        [Serializable]
        private class VisitorConfig
        {
            [SerializeField] private int 来访者间隔分钟 = 5;
            [SerializeField] private VisitorDizi[] 弟子来访配置;
            public int VisitorIntervalMins => 来访者间隔分钟;

            private VisitorDizi[] VisitorDizis => 弟子来访配置;

            // 获取随机的弟子来访配置
            public IVisitorDizi GetRandomVisitorDizi() => VisitorDizis.WeightPick();

            // 根据品质获取弟子来访配置
            public IVisitorDizi GetVisitorDiziByGrade(ColorGrade grade) =>
                VisitorDizis.Where(d => d.Grade == grade).First();

            public int GetVisitorBuyCost(ColorGrade grade) =>
                VisitorDizis.Where(d => d.Grade == grade).First().Buy;

            public int GetVisitorSellRandomCost(ColorGrade grade)
            {
                var vec = VisitorDizis.Where(d => d.Grade == grade).First().Sell;
                return UnityEngine.Random.Range(vec.x, vec.y);
            }
        }

        [Serializable] private class VisitorDizi : IVisitorDizi, IWeightElement
        {
            [SerializeField] private ColorGrade 品质;
            [SerializeField] private int 招募元宝;
            [SerializeField][MinMaxSlider(0,200,true)] private Vector2Int 征收元宝;
            [SerializeField] private int 权重;
            public ColorGrade Grade => 品质;
            public int Buy => 招募元宝;
            public Vector2Int Sell => 征收元宝;
            public int Weight => 权重;
        }

    }
}