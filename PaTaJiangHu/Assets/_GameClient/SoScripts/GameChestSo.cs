using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Fields;
using GameClient.SoScripts.Items;
using UnityEngine;

namespace GameClient.SoScripts
{
    public interface IGameChest : IGameReward
    {
        public int Id { get; }
        public string Name { get; }
    }

    [CreateAssetMenu(fileName = "id_宝箱", menuName = "宝箱/宝箱")]
    internal class GameChestSo : GameChestSoBase
    {
        [SerializeField] private RewardField 奖励;
        private RewardField Reward => 奖励;
        public IAdvPackage[] Packages => Reward.Packages;
        public IStacking<IGameItem>[] AllItems => Reward.AllItems;

        public override IGameChest GetChest() => new GameChest(Id, Name, Packages, AllItems);
    }

    /// <summary>
    /// 宝箱父类, 主要用于其它宝箱实现的统一接口
    /// </summary>
    internal abstract class GameChestSoBase : AutoAtNamingObject
    {
        public abstract IGameChest GetChest();
        internal record GameChest(int Id, string Name, IAdvPackage[] Packages, IStacking<IGameItem>[] AllItems) : IGameChest
        {
            public int Id { get; } = Id;
            public string Name { get; } = Name;
            public IAdvPackage[] Packages { get; } = Packages;
            public IStacking<IGameItem>[] AllItems { get; } = AllItems;
        }
    }

    public static class GameChestSoExtension
    {
        public static IGameChest Combine(this ICollection<IGameChest> sos) =>
            new GameChestSoBase.GameChest(0, "组合宝箱", 
                sos.SelectMany(_ => _.Packages).ToArray(),
                sos.SelectMany(_ => _.AllItems).ToArray());
    }
}