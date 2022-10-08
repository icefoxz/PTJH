using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Systems.Messaging
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class MessagingManager 
    {
        private Dictionary<string, Action<object[]>> EventMap { get; set; } = new Dictionary<string, Action<object[]>>();

        public void Invoke(string eventName, object[] args) => EventMap[eventName]?.Invoke(args);
        public void RegEvent(string eventName, Action<object[]> action)
        {
            if (!EventMap.ContainsKey(eventName))
                EventMap.Add(eventName, default);
            EventMap[eventName] += action;
        }
        public void RemoveEvent(string eventName, Action<object[]> action)
        {
            EventMap[eventName] -= action;
        }
    }

    public static class UnityButtonExtension
    {
        public static void OnClickAdd(this Button btn, Action action,bool removeAllListener = true)
        {
            if(removeAllListener) btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(new UnityAction(action));
        }
    }
}
