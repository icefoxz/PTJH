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
        //奖励记录器
        private RewardBoard RewardBoard => Game.World.RewardBoard;

        public void SetRewards(ICollection<IGameReward> rewards)
        {
            RewardBoard.SetRewards(rewards);
            foreach (var reward in rewards) AddRewardToFaction(reward);
        }

        public void SetReward(IGameReward reward, bool setToBoard)
        {
            if (setToBoard) RewardBoard.SetReward(reward);
            AddRewardToFaction(reward);
        }

        private void AddRewardToFaction(IGameReward reward)
        {
            foreach (var item in reward.AllItems)
                Faction.AddGameItem(item); //添加单个物品
            Faction.AddPackages(reward.Packages);
        }
    }
}