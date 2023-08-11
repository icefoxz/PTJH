using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Utls;
using GameClient.Modules.Adventure;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Adventures;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子历练(实时)活动信息
    /// </summary>
    public record AdventureActivity : IDiziState
    {
        public enum AdvTypes
        {
            Adventure,
            Production,
        }

        public enum States
        {
            Progress,
            Returning,
            Waiting,
        }

        #region IDiziState

        public DiziActivities Activity { get; } = DiziActivities.Adventure;
        public long StartTime { get; private set; }
        public long LastUpdate { get; private set; }

        public string ShortTitle => AdvType switch
        {
            AdvTypes.Adventure => State switch
            {
                States.Progress => "历",
                States.Returning => "回",
                States.Waiting => "待",
                _ => throw new ArgumentOutOfRangeException()
            },
            AdvTypes.Production => "产",
            _ => throw new ArgumentOutOfRangeException()
        };

        public string Description => AdvType switch
        {
            AdvTypes.Adventure => State switch
            {
                States.Progress => "历练中...",
                States.Returning => "回程中...",
                States.Waiting => "宗门等待...",
                _ => throw new ArgumentOutOfRangeException()
            },
            AdvTypes.Production => "生产中...",
            _ => throw new ArgumentOutOfRangeException()
        };

        public string StateLabel => AdvType switch
        {
            AdvTypes.Adventure => "历练",
            AdvTypes.Production => "生产",
            _ => throw new ArgumentOutOfRangeException()
        };

        public string CurrentMapName => Map.Name;
        public TimeSpan CurrentProgressTime => SysTime.CompareUnixNow(LastUpdate);

        #endregion

        public string CurrentOccasion { get; private set; }
        public int LastMiles { get; private set; }
        public long EndTime { get; private set; }

        public IAutoAdvMap Map { get; private set; }
        public Dizi Dizi { get; private set; }
        public AdvTypes AdvType { get; private set; }

        public States State
        {
            get
            {
                if (EndTime > 0)//设定了结束时间代表至少已经回程
                {
                    return LastUpdate >= EndTime ? 
                        States.Waiting : //最新更新时间大于结束时间代表活动结束
                        States.Returning; //否则代表回程中
                }
                return States.Progress;
            }
        }
        public IReadOnlyList<DiziActivityLog> Logs => _logs;
        public IReadOnlyList<ActivityFragment> History => _history;
        public IEnumerable<IGameReward> Rewards => Logs.Select(l => l.Reward).Where(r => r != null);
        private readonly List<DiziActivityLog> _logs = new List<DiziActivityLog>();
        private readonly List<ActivityFragment> _history = new List<ActivityFragment>();

        public AdventureActivity(IAutoAdvMap map, long startTime, Dizi dizi, bool isProduction)
        {
            Map = map;
            StartTime = startTime;
            LastUpdate = startTime;
            Dizi = dizi;
            AdvType = isProduction ? AdvTypes.Production : AdvTypes.Adventure;
        }

        public void Set(long lastUpdate, int lasMile, string occasionName, long endTime = 0)
        {
            LastUpdate = lastUpdate;
            LastMiles = lasMile;
            EndTime = endTime;
            CurrentOccasion = occasionName;
        }

        public void SetLastUpdate(long lastUpdate) => LastUpdate = lastUpdate;

        public void AddLog(DiziActivityLog log)
        {
            LastMiles = log.LastMiles;
            LastUpdate = log.OccurredTime;
            CurrentOccasion = log.Occasion;
            _logs.Add(log);
            var messages = log.Messages?.ToList() ?? new List<string>();
            messages.AddRange(log.AdjustEvents);
            _history.Add(new ActivityFragment(string.Join(' ', messages.Where(m => !string.IsNullOrEmpty(m))), log.Reward));
        }
    }
}