using System;
using System.Collections.Generic;
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
    }
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
            /// <summary>
            /// 战斗中
            /// </summary>
            Battle
        }
        
        public States Current
        {
            get
            {
                if (DiziState is IdleState) return States.Idle;
                if (DiziState is AutoAdventure a)
                {
                    return a.AdvType switch
                    {
                        AutoAdventure.AdvTypes.Adventure => a.State switch
                        {
                            AutoAdventure.States.Progress => States.AdvProgress,
                            AutoAdventure.States.Recall => States.AdvReturning,
                            AutoAdventure.States.End => States.AdvWaiting,
                            _ => throw new ArgumentOutOfRangeException()
                        },
                        AutoAdventure.AdvTypes.Production => States.AdvProduction,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
                if (DiziState is LostState) return States.Lost;
                if (DiziState is BattleState) return States.Battle;
                throw new NotImplementedException();
            }
        }
        public IdleState Idle { get; private set; }
        public LostState LostState { get; private set; }
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
                if (Current is States.AdvProgress or States.AdvReturning or States.AdvProduction or States.AdvWaiting)
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
        /// <summary>
        /// 是否可能失踪
        /// </summary>
        public bool IsPossibleLost => Current switch
        {
            States.Lost => true,
            States.Idle => false,
            States.AdvProgress => Adventure.Map.PossibleLost(Dizi),
            States.AdvProduction => Adventure.Map.PossibleLost(Dizi),
            States.AdvReturning => Adventure.Map.PossibleLost(Dizi),
            States.AdvWaiting => Adventure.Map.PossibleLost(Dizi),
            _ => throw new ArgumentOutOfRangeException()
        };

        public IRewardHandler RewardHandler => Current switch
        {
            States.Lost => null,
            States.Idle => Idle,
            States.AdvProgress => Adventure,
            States.AdvProduction => Adventure,
            States.AdvReturning => Adventure,
            States.AdvWaiting => Adventure,
            _ => throw new ArgumentOutOfRangeException()
        };

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

        public override string ToString() => string.Join('|', ShortTitle, Description, LastUpdate);

        public void StartIdle(long startTime)
        {
            LastStateTick = startTime;
            Idle = new IdleState(Dizi, startTime, ActivityPlayer);
        }

        public void StartLost(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            LastStateTick = startTime;
            LostState = new LostState(dizi, startTime, lastActivityLog);
        }

        public void RestoreFromLost()
        {
            LostState = null;
        }

        public void RecallFromAdventure(long now, int lastMile,long reachingTime)
        {
            Adventure.Recall(now, lastMile, reachingTime);
        }

        public void StartAdventure(IAutoAdvMap map, long startTime, int messageSecs, bool isProduction)
        {
            LastStateTick = startTime;
            Adventure = new AutoAdventure(map, startTime, messageSecs, Dizi, isProduction, ActivityPlayer);
        }

        public void FinalizeAdventure()
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Adventure = null;
        }

        public void Terminate(long terminateTime,int lastMile)
        {
            Adventure.UpdateStoryService.RemoveAllListeners();
            Adventure.Terminate(terminateTime, lastMile);
            Adventure = null;
        }

        public void StopIdleState()
        {
            Idle.StopIdleState();
            Idle = null;
        }

        public void RegIdleStory(DiziActivityLog log) => Idle.RegStory(log);

        public void RegAutoAdvStory(DiziActivityLog log) => Adventure.RegStory(log);
    }
}