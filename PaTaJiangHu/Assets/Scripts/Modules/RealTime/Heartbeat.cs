using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;

namespace RealTime
{
    public interface IHeartbeat
    {
        void RegHeartbeat(long id, Action action);
    }

    /// <summary>
    /// 心跳
    /// </summary>
    public class Heartbeat : MonoBehaviour, IHeartbeat
    {
        private Dictionary<long,Action> Triggers { get; } = new Dictionary<long,Action>();
        private bool _isInit;

        public void Init()
        {
            if (_isInit) return;
            _isInit = true;
            Game.CoService.RunCo(HalfSecondRoutine());
        }

        public void RegHeartbeat(long id, Action action)
        {
            if (Triggers.ContainsKey(id))
            {
                Triggers[id] += action;
                return;
            }
            Triggers.Add(id, action);
        }

        void OnApplicationFocus(bool focus)
        {
            if (!_isInit) return;
            if (focus) HeartbeatUpdate();
        }

        private IEnumerator HalfSecondRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            HeartbeatUpdate();
        }

        private void HeartbeatUpdate()
        {
            var now = SysTime.UnixNow;
            foreach (var (key, action) in Triggers.Where(t => t.Key > now).ToArray())
            {
                Triggers.Remove(key);
                action.Invoke();
            }
        }
    }
}