using System;
using System.Collections.Generic;
using _GameClient.Models.States;
using Models;
using Server.Configs.Adventures;
using Server.Controllers;
using UnityEngine.Events;
using Utls;

namespace _GameClient.Models
{
    public interface IDiziState
    {
        /// <summary>
        /// 状态开始时间
        /// </summary>
        long StartTime { get; }
        /// <summary>
        /// 最后一次更新
        /// </summary>
        long LastUpdate { get; }
        /// <summary>
        /// 状态标签
        /// </summary>
        string ShortTitle { get; }
        /// <summary>
        /// 事件描述
        /// </summary>
        string Description { get; }
        /// <summary>
        /// 事件场景
        /// </summary>
        string CurrentOccasion { get; }
        /// <summary>
        /// 当前地图
        /// </summary>
        string CurrentMapName { get; }
        /// <summary>
        /// 时间描述
        /// </summary>
        string StateLabel { get; }
        /// <summary>
        /// 事件经过时间
        /// </summary>
        TimeSpan CurrentProgressTime { get; }
        DiziStateHandler Handler { get; }
    }
    /// <summary>
    /// 弟子状态处理器
    /// </summary>
    public class DiziStateHandler
    {
        public enum States
        {
            /// <summary>
            /// 自动状态，初始状态, 并且会自动转换发呆状态
            /// </summary>
            Auto,
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
            /// <summary>
            /// 战斗中
            /// </summary>
            Battle,
        }
        
        public States Current
        {
            get
            {
                if (DiziState is IdleState) return States.Idle;
                if (DiziState is AutoAdventure a)
                {
                    return a.State switch
                    {
                        AutoAdventure.States.Progress => a.AdvType switch
                        {
                            AutoAdventure.AdvTypes.Adventure => States.AdvProgress,
                            AutoAdventure.AdvTypes.Production => States.AdvProduction,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        AutoAdventure.States.Recall => States.AdvReturning,
                        AutoAdventure.States.End => States.AdvWaiting,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                if (DiziState is LostState) return States.Lost;
                if (DiziState is BattleState) return States.Battle;
                return States.Auto;
            }
        }
        /// <summary>
        /// 当前的闲置状态,如果没有会返回null
        /// </summary>
        public IdleState Idle { get; private set; }
        /// <summary>
        /// 当前历练状态(如果没有会返回null)
        /// </summary>
        public AutoAdventure Adventure { get; private set; }
        
        public IDiziState DiziState { get; private set; } 

        public string ShortTitle => DiziState.ShortTitle;
        public string Description => DiziState.Description;
        public long LastUpdate => DiziState.LastUpdate;
        /// <summary>
        /// 上一个获取的奖励
        /// </summary>
        public IGameReward LastReward { get; private set; }
        private Dizi Dizi { get; }
        public IReadOnlyList<IGameReward> StateBags
        {
            get
            {
                if (DiziState is AutoAdventure)
                    return Adventure.Rewards;
                return Array.Empty<IGameReward>();
            }
        }

        private DiziActivityPlayer ActivityPlayer { get; }
        public IReadOnlyList<ActivityFragment> LogHistory => ActivityPlayer.LogHistory;

        public int CurrentMile
        {
            get
            {
                if (DiziState is AutoAdventure a)
                    return a.State == AutoAdventure.States.Progress 
                        ? a.LastMile 
                        : -1;
                return 0;
            }
        }

        /// <summary>
        /// 当前状态事件经过(多少秒)
        /// </summary>
        public int CurrentStateProgressInSecs =>
            (int)TimeSpan.FromMilliseconds(SysTime.UnixNow - LastStateTick).TotalSeconds;
        /// <summary>
        /// 上一个状态开始时间
        /// </summary>
        public long LastStateTick { get; private set; }

        public string DiziName => Dizi.Name;

        public IRewardHandler RewardHandler
        {
            get
            {
                if(DiziState is AutoAdventure)return Adventure;
                if(DiziState is IdleState)return Idle;
                return null;
            }
        }

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
            DiziState = new AutoState(this);
        }
        private void RewardMethod(IGameReward reward) => LastReward = reward;

        public override string ToString() => string.Join('|', ShortTitle, Description, LastUpdate);

        public void StartIdle(long startTime)
        {
            LastStateTick = startTime;
            Idle = new IdleState(Dizi, startTime, ActivityPlayer, this);
            DiziState = Idle;
        }

        public void StartLost(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            LastStateTick = startTime;
            DiziState = new LostState(dizi, startTime, lastActivityLog, this);
        }

        public void RecallFromAdventure(long now, int lastMile,long reachingTime)
        {
            Adventure.Recall(now, lastMile, reachingTime);
        }

        public void StartAdventure(IAutoAdvMap map, long startTime, int messageSecs, bool isProduction)
        {
            LastStateTick = startTime;
            Adventure = new AutoAdventure(map, startTime, messageSecs, Dizi, isProduction, ActivityPlayer, this);
            DiziState = Adventure;
        }

        public void FinalizeAdventure()
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Adventure = null;
            DiziState = null;
        }

        public void Terminate(long terminateTime,int lastMile)
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Adventure.Terminate(terminateTime, lastMile);
            Adventure = null;
            DiziState = null;
        }

        public void StopIdleState()
        {
            Idle.StopIdleState();
            Idle = null;
            DiziState = null;
        }

        public void RegIdleStory(DiziActivityLog log) => Idle.RegStory(log);

        public void RegAutoAdvStory(DiziActivityLog log) => Adventure.RegStory(log);
    }
}