﻿using System.Collections.Generic;
using System.Linq;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Modules.DiziM;
using GameClient.System;

namespace GameClient.Models
{
    /// <summary>
    /// 奖励栏模型, 用于存放或展示奖励信息
    /// </summary>
    public class RewardBoard : ModelBase
    {
        protected override string LogPrefix => "奖励栏";
        public IGameReward[] Rewards { get; private set; }

        public void SetReward(IGameReward reward)
        {
            Rewards = new IGameReward[] { reward };
            XDebug.Log(
                $"注册奖励: 物品x{reward.AllItems.Length}, " +
                $"包裹x{reward.Packages.Length}!");
            Game.MessagingManager.Send(EventString.Rewards_Prompt, string.Empty);
        }

        public void SetRewards(ICollection<IGameReward> rewards)
        {
            Rewards = rewards.ToArray();
            XDebug.Log(
                $"注册奖励: 物品x{rewards.SelectMany(r => r.AllItems).Count()}, " +
                $"包裹x{rewards.SelectMany(r => r.Packages).Count()}!");
            Game.MessagingManager.Send(EventString.Rewards_Prompt, string.Empty);
        }
    }
}