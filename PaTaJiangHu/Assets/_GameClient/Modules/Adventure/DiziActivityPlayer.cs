using System.Collections.Generic;
using AOT.Core.Systems.Coroutines;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.System;
using UnityEngine.Events;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 弟子活动的播放器, 主要用来延迟展示模型的状态, 让玩家会感觉历练是实时交互的.
    /// </summary>
    public class DiziActivityPlayer //: MonoBehaviour
    {
        private DiziActivityLogHandler _handler;
        private float _speed = 1f;//暂时设为每秒播放故事信息
        private readonly List<ActivityFragment> _logHistory = new List<ActivityFragment>();
        public event UnityAction<string> PlayMessage;
        public event UnityAction<string> PlayAdjustment;
        public event UnityAction<IGameReward> PlayReward;
        private ICoroutineInstance Co { get; set; }
        private Dizi Dizi { get; }

        public IReadOnlyList<ActivityFragment> LogHistory => _logHistory;

        public DiziActivityPlayer(Dizi dizi)
        {
            Dizi = dizi;
        }
   
        //注册历练事件并且播放
        public void Reg(DiziActivityLog story)
        {
            if (_handler == null)
            {
                //XDebug.Log("重新生成Log Handler!");
                _handler = new DiziActivityLogHandler(_speed);
                _handler.PlayMessage += OnHandlerOnPlayMessage;
                _handler.PlayAdjustment += OnHandlerOnPlayAdjustment;
                _handler.PlayReward += OnHandlerOnPlayReward;
                _handler.PlayEnd += OnPlayerEnd;
                _handler.Subscribe(story);
                Co = Game.CoService.RunCo(_handler.Play(), null, Dizi.Name);
                //StopAllCoroutines();
                //StartCoroutine(_handler.Play());
                return;
            }
            _handler.Subscribe(story);
        }

        private void OnHandlerOnPlayReward(IGameReward o)
        {
            _logHistory.Add(ActivityFragment.InstanceFragment(o));
            PlayReward?.Invoke(o);
        }

        private void OnHandlerOnPlayAdjustment(string o)
        {
            _logHistory.Add(ActivityFragment.InstanceFragment(o));
            PlayAdjustment?.Invoke(o);
        }

        private void OnHandlerOnPlayMessage(string o)
        {
            _logHistory.Add(ActivityFragment.InstanceFragment(o));
            PlayMessage?.Invoke(o);
        }

        private void OnPlayerEnd()
        {
            _handler.PlayMessage -= OnHandlerOnPlayMessage;
            _handler.PlayAdjustment -= OnHandlerOnPlayAdjustment;
            _handler.PlayReward -= OnHandlerOnPlayReward;
            _handler.PlayEnd -= OnPlayerEnd;
            _handler = null;
        }
        public void ClearHistory() => _logHistory.Clear();
        public void FastForward(int secs) => _handler.FastForward(secs);
    }
}