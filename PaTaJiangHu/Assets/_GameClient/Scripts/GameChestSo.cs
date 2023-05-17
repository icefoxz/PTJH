using Core;
using Server.Configs.Adventures;
using Server.Configs.Fields;
using Server.Configs.Items;
using UnityEngine;

public interface IGameChest : IGameReward
{
    public int Id { get; }
    public string Name { get; }
}
[CreateAssetMenu(fileName = "id_宝箱", menuName = "挑战/宝箱")]
internal class GameChestSo : AutoAtNamingObject,IGameChest
{
    [SerializeField] private RewardField 奖励;
    private RewardField Reward => 奖励;
    public IAdvPackage[] Packages => Reward.Packages;
    public IStacking<IGameItem>[] AllItems => Reward.AllItems;
}