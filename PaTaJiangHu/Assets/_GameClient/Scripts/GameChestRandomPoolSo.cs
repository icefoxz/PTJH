using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "id_随机池", menuName = "宝箱/随机池")]
internal class GameChestRandomPoolSo : GameChestSoBase
{
    [SerializeField] private GameChestSoBase[] 宝箱;
    private GameChestSoBase[] Chests => 宝箱;
    public override IGameChest GetChest() => Chests.OrderBy(_ => Random.Range(0, 1f)).FirstOrDefault()?.GetChest();
}