using System.Collections.Generic;
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
        public Faction Faction { get; private set; } = new Faction();
        /// <summary>
        /// 奖励板, 用于存放或展示奖励信息
        /// </summary>
        public RewardBoard RewardBoard { get; } = new RewardBoard();
        /// <summary>
        /// 招募器, 用于招募角色
        /// </summary>
        public Recruiter Recruiter { get; } = new Recruiter();

        public DiziState State { get; } = new DiziState();

        public void SetFaction(Faction faction)
        {
            Faction = faction;
            Game.MessagingManager.Send(eventName: EventString.Faction_Init, string.Empty);
        }

        //弟子状态管理类
        public class DiziState
        {
            /// <summary>
            /// 无状态, 用于存放不属于任何状态的弟子, <br/>
            /// 所有控制器必须在开始状态切换从此表获取弟子. <br/>
            /// 结束状态也把弟子放入此表
            /// </summary>
            private readonly Dictionary<string, Dizi> _stateless = new Dictionary<string, Dizi>();
            /// <summary>
            /// 历练
            /// </summary>
            public Adventure_ActivityManager Adventure { get; } = new Adventure_ActivityManager();
            /// <summary>
            /// 失踪
            /// </summary>
            public Lost_ActivityManager Lost { get; } = new Lost_ActivityManager();
            /// <summary>
            /// 生产
            /// </summary>
            //public Production_ActivityManager Production { get; } = new Production_ActivityManager();
            /// <summary>
            /// 发呆
            /// </summary>
            public Idle_ActivityManager Idle { get; } = new Idle_ActivityManager();

            public void AddStateless(Dizi dizi)
            {
                _stateless.Add(dizi.Guid, dizi);
                dizi.SetState(null);
                Game.MessagingManager.Send(EventString.Dizi_Stateless_Start, dizi.Guid);
            }

            public Dizi RemoveStateless(string guid)
            {
                var dizi = GetStatelessDizi(guid);
                _stateless.Remove(guid);
                Game.MessagingManager.Send(EventString.Dizi_State_Update, guid);
                return dizi;
            }

            public Dizi GetStatelessDizi(string guid) => _stateless[guid];
            public IEnumerable<Dizi> GetStatelessDizis() => _stateless.Values;
        }
    }
}