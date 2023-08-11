using System;
using AOT.Core.Systems.Coroutines;
using GameClient.Modules.DiziM;
using GameClient.System;
using System.Collections.Generic;
using AOT.Utls;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动的记录器, 会根据设置的播放速度添加到历史记录
    /// </summary>
    public class ActivityDelayedPlayer 
    {
        private ActivityCoPlayer _coPlayer;
        private float _speed = 1f;//暂时设为每秒播放故事信息
        private ICoroutineInstance Co { get; set; }
        private List<ActivityFragment> _activityHistory = new List<ActivityFragment>();
        public IReadOnlyList<ActivityFragment> ActivityHistory => _activityHistory;
        public string CoName { get; }
        public string CoParent { get; }
        public event Action<string> OnAdjustmentAction;
        public event Action<string> OnMessageAction;
        public event Action<IGameReward> OnRewardAction;

        public ActivityDelayedPlayer(string coName,
            string coParent,
            Func<string, ActivityFragment> messageFunc,
            Func<IGameReward, ActivityFragment> rewardFunc)
        {
            CoName = coName;
            CoParent = coParent;
            MessageFunc = messageFunc;
            RewardFunc = rewardFunc;
        }

        private event Func<string, ActivityFragment> MessageFunc;
        private event Func<IGameReward, ActivityFragment> RewardFunc;

        //注册历练事件并且播放
        public void Reg(DiziActivityLog story)
        {
            if (_coPlayer == null) // 如果没有播放器, 则生成一个
            {
                //XDebug.Log("重新生成Log Handler!");
                _coPlayer = new ActivityCoPlayer(_speed);
                _coPlayer.PlayMessage += OnHandlerOnPlayMessage;
                _coPlayer.PlayAdjustment += OnHandlerOnPlayAdjustment;
                _coPlayer.PlayReward += OnHandlerOnPlayReward;
                _coPlayer.PlayEnd += OnCoPlayerEnd;
                _coPlayer.Subscribe(story); // 注册事件并且播放
                Co = Game.CoService.RunCo(_coPlayer.Play(), CoParent, CoName);
                return;
            }
            _coPlayer.Subscribe(story); // 注册事件并且播放
        }

        private void OnHandlerOnPlayReward(IGameReward o)
        {
            XDebug.Log($"Reward({o.AllItems?.Length})");
            _activityHistory.Add(RewardFunc?.Invoke(o));
            OnRewardAction?.Invoke(o);
        }

        private void OnHandlerOnPlayAdjustment(string o)
        {
            XDebug.Log(o);
            _activityHistory.Add(MessageFunc?.Invoke(o));
            OnAdjustmentAction?.Invoke(o);
        }

        private void OnHandlerOnPlayMessage(string o)
        {
            XDebug.Log(o);
            _activityHistory.Add(MessageFunc?.Invoke(o));
            OnMessageAction?.Invoke(o);
        }

        private void OnCoPlayerEnd()
        {
            if(_coPlayer == null) return;
            _coPlayer.PlayMessage -= OnHandlerOnPlayMessage;
            _coPlayer.PlayAdjustment -= OnHandlerOnPlayAdjustment;
            _coPlayer.PlayReward -= OnHandlerOnPlayReward;
            _coPlayer.PlayEnd -= OnCoPlayerEnd;
            _coPlayer = null;
        }
        /// <summary>
        /// 快速播完所有的故事信息
        /// </summary>
        /// <param name="secs"></param>
        private void FastForward(int secs) => _coPlayer.FastForward(secs);
        public void Stop()
        {
            Co?.StopCo();
            OnCoPlayerEnd();
        }
    }
}