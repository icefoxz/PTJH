﻿using System;
using System.Collections.Generic;
using Server.Configs.Adventures;
using Server.Controllers;
using UnityEngine.Events;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 弟子状态处理器
    /// </summary>
    public class DiziStateHandler
    {
        public enum States
        {
            /// <summary>
            /// 失踪状态
            /// </summary>
            Lost,
            /// <summary>
            /// 发呆状态
            /// </summary>
            Idle,
            /// <summary>
            /// 历练中
            /// </summary>
            AdvProgress,
            /// <summary>
            /// 生产中
            /// </summary>
            AdvProduction,
            /// <summary>
            /// 历练回程中
            /// </summary>
            AdvReturning,
            /// <summary>
            /// 历练等待中
            /// </summary>
            AdvWaiting,
        }
        
        public States Current
        {
            get
            {
                //闲置状态会延迟,所以用IsActive来检查是否活跃
                var result = CheckNull(LostState) + (Idle.IsActive ? 1 : 0) + CheckNull(Adventure);
                if (result > 1)
                    XDebug.LogError(
                        $"状态异常!活跃状态: Lost:{LostState != null}, Idle:{Idle?.IsActive}, Adventure:{Adventure != null}");
                if (LostState != null) return States.Lost;
                if (Idle != null) return States.Idle;
                if (Adventure != null)
                {
                    return Adventure.State switch
                    {
                        AutoAdventure.States.Progress => States.AdvProgress,
                        AutoAdventure.States.Recall => States.AdvReturning,
                        AutoAdventure.States.End => States.AdvWaiting,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                throw new NotImplementedException();
                int CheckNull(object obj) => obj != null ? 1 : 0;
            }
        }
        public IdleState Idle { get; private set; }
        public LostState LostState { get; private set; }
        public AutoAdventure Adventure { get; private set; }

        public string ShortTitle { get; private set; } = "闲";
        public string Description { get; private set; }
        public long LastUpdate { get; private set; }
        /// <summary>
        /// 上一个获取的奖励
        /// </summary>
        public IGameReward LastReward { get; private set; }
        private Dizi Dizi { get; }
        private DiziActivityPlayer ActivityPlayer { get; }
        public IReadOnlyList<ActivityFragment> LogHistory => ActivityPlayer.LogHistory;

        public DiziStateHandler(Dizi dizi, UnityAction<string> messageAction, UnityAction<string> adjustAction,
            UnityAction rewardAction)
        {
            Dizi = dizi;
            ActivityPlayer = new DiziActivityPlayer(dizi);
            ActivityPlayer.PlayMessage += messageAction;
            ActivityPlayer.PlayAdjustment += adjustAction;
            ActivityPlayer.PlayReward += r =>
            {
                RewardMethod(r);
                rewardAction?.Invoke();
            };
        }
        private void RewardMethod(IGameReward reward) => LastReward = reward;

        // 设定弟子状态短文本
        private void Set(string shortTitle, string description, long lastUpdate)
        {
            ShortTitle = shortTitle;
            Description = description;
            LastUpdate = lastUpdate;
        }

        public override string ToString() => string.Join('|', ShortTitle, Description, LastUpdate);

        public void StartIdle(long startTime)
        {
            Idle = new IdleState(Dizi, startTime, ActivityPlayer);
            Set("闲", "闲置中...", startTime);
        }

        public void StartLost(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            LostState = new LostState(dizi, startTime, lastActivityLog);
            Set("失", "失踪了...", startTime);
        }

        public void RestoreFromLost()
        {
            LostState = null;
            Set("归", "失踪回归...", SysTime.UnixNow);
        }

        public void RecallFromAdventure(long now, int lastMile,long reachingTime)
        {
            Set("回", "回程中...", now);
            Adventure.Recall(now, lastMile, reachingTime, () => Set("待", "宗门外等待", 0));
        }

        public void StartAdventure(IAutoAdvMap map, long startTime, int messageSecs, bool isProduction)
        {
            Adventure = new AutoAdventure(map, startTime, messageSecs, Dizi, isProduction, ActivityPlayer);
            Adventure.UpdateStoryService.AddListener(() => Set("历", "历练中...", startTime));
        }

        public void FinalizeAdventure()
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Set("归", "历练回归...", SysTime.UnixNow);
            Adventure = null;
        }

        public void Terminate(long terminateTime,int lastMile)
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Set("断", "历练中断...", terminateTime);
            Adventure.Terminate(terminateTime, lastMile);
            Adventure = null;
        }
    }
}