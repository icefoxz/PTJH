using System;
using System.Collections.Generic;
using _GameClient.Models;
using Server.Configs.Adventures;

namespace Server.Controllers
{
    /// <summary>
    /// 奖励控制器
    /// </summary>
    public class RewardController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private RewardContainer RewardContainer => Game.World.RewardContainer;

        public void SetRewards(ICollection<IGameReward> rewards)
        {
            RewardContainer.SetRewards(rewards);
            foreach (var reward in rewards)
            {
                foreach (var item in reward.AllItems) 
                    Faction.AddGameItem(item); //添加单个物品

                Faction.AddPackages(reward.Packages);
            }
        }
    }
}