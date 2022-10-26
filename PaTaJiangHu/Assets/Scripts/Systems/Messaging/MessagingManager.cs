using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using Utls;

namespace Systems.Messaging
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class MessagingManager 
    {
        private Dictionary<string, Action<string>> EventMap { get; set; } = new Dictionary<string, Action<string>>();

        public void Invoke(string eventName, object obj)
        {
            if (EventMap.ContainsKey(eventName))
            {
                var method = EventMap[eventName];
                if (obj == null)
                    method?.Invoke(null);
                else method?.Invoke(Json.Serialize(obj));
            }
        }

        public void Invoke(string eventName, string args) =>
            EventMap[eventName]?.Invoke(string.IsNullOrEmpty(args) ? args : null);

        public void RegEvent(string eventName, Action<string> action)
        {
            if (!EventMap.ContainsKey(eventName))
                EventMap.Add(eventName, default);
            EventMap[eventName] += action;
        }
        public void RemoveEvent(string eventName, Action<string> action)
        {
            EventMap[eventName] -= action;
        }
    }

    public static class UnityButtonExtension
    {
        public static void OnClickAdd(this Button btn, Action action,bool removeAllListener = true)
        {
            if(removeAllListener) btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(action == null ? default : new UnityAction(action));
        }
    }
}
