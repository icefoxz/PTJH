using System;
using GameClient.Models;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "生产地图名", menuName = "状态玩法/生产/生产地图")]
    internal class ProductionMapSo : AdventureMapSoBase
    {
        [SerializeField] private ActivityTypes 生产类型;
        private enum ActivityTypes
        {
            Farming, // 务农
            Trading, // 贸易
            Brewing, // 酿酒
            Alchemy, // 炼丹
            Herbalist, // 药师
        }
        public override AdvActivityTypes ActivityType => 生产类型 switch {
            ActivityTypes.Farming => AdvActivityTypes.Farming,
            ActivityTypes.Trading => AdvActivityTypes.Trading,
            ActivityTypes.Brewing => AdvActivityTypes.Brewing,
            ActivityTypes.Alchemy => AdvActivityTypes.Alchemy,
            ActivityTypes.Herbalist => AdvActivityTypes.Herbalist,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}