using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameClient.Controllers;
using GameClient.Modules.DiziM;
using UnityEngine;
using UnityEngine.Events;

namespace GameClient.Modules.Adventure
{
    public class DiziActivityLogHandler
    {
        private readonly Queue<ActivityLogWrapper> _storyQueue = new Queue<ActivityLogWrapper>();
        private float _secs = 1f;//常规一个信息秒数
        private bool _isPlayedAll;//是否已经完整播放完毕
        private ActivityLogWrapper _current;//当前播放信息
        public event UnityAction<string> PlayMessage;
        public event UnityAction<string> PlayAdjustment;
        public event UnityAction<IGameReward> PlayReward;
        public event UnityAction PlayEnd;

        public DiziActivityLogHandler(float secs)
        {
            _secs = secs;
        }


        public void Subscribe(DiziActivityLog story)
        {
            var s = new ActivityLogWrapper(story);
            _storyQueue.Enqueue(s);
            //XDebug.Log($"已注册事件:{story},总信息:{s.PlayCount}");
        }

        public void FastForward(float secs)
        {
            var count = _storyQueue.Sum(s => s.PlayCount);
            if (count > 0) _secs = secs / count;
            //XDebug.Log($"Fastforward:速度改成:{_secs}秒,总信息:{count}");
        }

        public IEnumerator Play()
        {
            while (!_isPlayedAll)
            {
                PlayLogic();
                yield return new WaitForSeconds(_secs);
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


    }
}