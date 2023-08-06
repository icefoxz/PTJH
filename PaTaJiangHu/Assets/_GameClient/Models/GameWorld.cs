using AOT.Core;
using GameClient.System;

namespace GameClient.Models
{
    /// <summary>
    /// 游戏世界，管理所有游戏模型
    /// </summary>
    public class GameWorld
    {
        /// <summary>
        /// 玩家门派
        /// </summary>
        public Faction Faction { get; private set; }
        /// <summary>
        /// 奖励板, 用于存放或展示奖励信息
        /// </summary>
        public RewardBoard RewardBoard { get; } = new RewardBoard();
        /// <summary>
        /// 招募器, 用于招募角色
        /// </summary>
        public Recruiter Recruiter { get; } = new Recruiter();
        /// <summary>
        /// 历练
        /// </summary>
        public DiziAdventure Adventure { get; } = new DiziAdventure();
        /// <summary>
        /// 失踪
        /// </summary>
        public DiziLost Lost { get; } = new DiziLost();
        /// <summary>
        /// 生产
        /// </summary>
        public DiziProduction Production { get; } = new DiziProduction();

        public void SetFaction(Faction faction)
        {
            Faction = faction;
            Game.MessagingManager.Send(eventName: EventString.Faction_Init, string.Empty);
        }
    }
}