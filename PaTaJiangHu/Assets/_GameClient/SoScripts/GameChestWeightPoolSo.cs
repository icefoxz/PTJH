using System;
using AOT._AOT.Utls;
using UnityEngine;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "id_权重池", menuName = "宝箱/权重池")]
    internal class GameChestWeightPoolSo : GameChestSoBase
    {
        [SerializeField] private WeightElement[] 设定;
        private WeightElement[] WeightElements => 设定;
        public override IGameChest GetChest() => WeightElements.WeightPick()?.So.GetChest();
        [Serializable]private class WeightElement : IWeightElement
        {
            [SerializeField] private GameChestSoBase 宝箱;
            [SerializeField] private int 权重;
            public GameChestSoBase So => 宝箱;
            public int Weight => 权重;
        }
    }
}