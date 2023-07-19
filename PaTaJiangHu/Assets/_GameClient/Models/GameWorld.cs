using AOT.Core;
using GameClient.System;

namespace GameClient.Models
{
    /// <summary>
    /// 游戏世界，管理所有游戏模型
    /// </summary>
    public class GameWorld
    {
        private Faction _faction;

        /// <summary>
        /// 玩家门派
        /// </summary>
        public Faction Faction => _faction;
        /// <summary>
        /// 奖励板, 用于存放或展示奖励信息
        /// </summary>
        public RewardBoard RewardBoard { get; } = new RewardBoard();
        /// <summary>
        /// 招募器, 用于招募角色
        /// </summary>
        public Recruiter Recruiter { get; } = new Recruiter();

        public void SetFaction(Faction faction)
        {
            _faction = faction;
            Game.MessagingManager.Send(eventName: EventString.Faction_Init, string.Empty);
        }
    }
}