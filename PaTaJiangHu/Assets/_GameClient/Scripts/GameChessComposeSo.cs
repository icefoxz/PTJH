using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "id_组合", menuName = "宝箱/组合")]
internal class GameChessComposeSo : GameChestSoBase
{
    [SerializeField] private GameChestSoBase[] 宝箱;
    private GameChestSoBase[] Chests => 宝箱;

    public override IGameChest GetChest() => Chests.Select(c=>c.GetChest()).ToArray().Combine();
}