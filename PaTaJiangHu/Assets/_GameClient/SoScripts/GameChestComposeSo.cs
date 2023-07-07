using System.Linq;
using UnityEngine;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "id_组合", menuName = "宝箱/组合")]
    internal class GameChestComposeSo : GameChestSoBase
    {
        [SerializeField] private GameChestSoBase[] 宝箱;
        private GameChestSoBase[] Chests => 宝箱;

        public override IGameChest GetChest() => Chests.Select(c=>c.GetChest()).ToArray().Combine();
    }
}