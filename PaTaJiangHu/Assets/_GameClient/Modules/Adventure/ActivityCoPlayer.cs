using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameClient.Modules.DiziM;
using UnityEngine;
using UnityEngine.Events;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动信息播放器, 基于<see cref="ActivityTempLog"/>处理成一条条信息碎片根据秒数播放.<br/>
    /// 主要透过事件注册来播放各种事件信息: <br/>
    /// 1.<see cref="PlayMessage"/> 故事文本<br/>
    /// 2.<see cref="PlayAdjustment"/> 弟子属性调整文本<br/>
    /// 3.<see cref="PlayReward"/> 弟子奖励<br/>
    /// 4.<see cref="PlayEnd"/>播放结束<br/>
    /// </summary>
    public class ActivityCoPlayer
    {
        private readonly Queue<ActivityTempLog> _storyQueue = new Queue<ActivityTempLog>();
        private float _secForAMessage = 1f;//常规一个信息秒数
        private bool _isPlayedAll;//是否已经完整播放完毕
        private ActivityTempLog _current;//当前播放信息
        public event UnityAction<string> PlayMessage;
        public event UnityAction<string> PlayAdjustment;
        public event UnityAction<IGameReward> PlayReward;
        public event UnityAction PlayEnd;

        public ActivityCoPlayer(float secForAMessage)
        {
            _secForAMessage = secForAMessage;
        }

        public void Subscribe(DiziActivityLog story)
        {
            var s = new ActivityTempLog(story);
            _storyQueue.Enqueue(s);
            //XDebug.Log($"已注册事件:{story},总信息:{s.PlayCount}");
        }

        public void FastForward(float secs)
        {
            var count = _storyQueue.Sum(s => s.PlayCount);
            if (count > 0) _secForAMessage = secs / count;
            //XDebug.Log($"Fastforward:速度改成:{_secs}秒,总信息:{count}");
        }

        public IEnumerator Play() // 播放 
        {
            while (!_isPlayedAll)
            {
                PlayLogic();
                yield return new WaitForSeconds(_secForAMessage);
            }
            PlayEnd?.Invoke();
        }

        private void PlayLogic()
        {
            if (_current == null)
            {
                if (_storyQueue.Count > 0)
                {
                    _current = _storyQueue.Dequeue();
                }
                else
                {
                    _isPlayedAll = true;
                    return;
                }
            }

            if (_current.Messages.Count > 0)
            {
                PlayMessage?.Invoke(_current.Messages.Dequeue());
                return;
            }

            if (_current.AdjustEvents.Count > 0)
            {
                PlayAdjustment?.Invoke(_current.AdjustEvents.Dequeue());
                return;
            }

            if (_current.Reward != null)
            {
                PlayReward?.Invoke(_current.Reward);
                _current.Reward = null;
                return;
            }
            _current = null;
        }

        private record ActivityTempLog
        {
            public Queue<string> Messages { get; set; }
            public string DiziGuid { get; set; }
            public long NowTicks { get; set; }
            public int LastMiles { get; set; }
            public Queue<string> AdjustEvents { get; set; }
            public IGameReward Reward { get; set; }
            public int PlayCount { get; }

            public ActivityTempLog(DiziActivityLog diziActivityLog)
            {
                Messages = diziActivityLog.Messages == null
                    ? new Queue<string>()
                    : new Queue<string>(diziActivityLog.Messages);
                AdjustEvents = diziActivityLog.AdjustEvents == null
                    ? new Queue<string>()
                    : new Queue<string>(diziActivityLog.AdjustEvents);
                DiziGuid = diziActivityLog.DiziGuid;
                NowTicks = diziActivityLog.OccurredTime;
                LastMiles = diziActivityLog.LastMiles;
                Reward = diziActivityLog.Reward;
                PlayCount = ActivityFragment.GetPlayCount(diziActivityLog);
            }
        }
    }
}