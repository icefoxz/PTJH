using System;
using System.Collections.Generic;
using Utls;

namespace Systems.Messaging
{
    /// <summary>
    /// 事件消息系统
    /// </summary>
    public class MessagingManager 
    {
        private Dictionary<string, Action<string>> EventMap { get; set; } = new Dictionary<string, Action<string>>();

        public void Invoke<T>(string eventName, T obj) where T : class, new()
        {
            if (EventMap.ContainsKey(eventName))
            {
                var method = EventMap[eventName];
                if (obj == null)
                    method?.Invoke(null);
                else method?.Invoke(Json.Serialize(obj));
            }
        }
        public void Invoke<T>(string eventName, T[] obj) where T : class, new()
        {
            if (EventMap.ContainsKey(eventName))
            {
                var method = EventMap[eventName];
                if (obj == null)
                    method?.Invoke(null);
                else method?.Invoke(Json.Serialize(obj));
            }
        }

        public void Invoke(string eventName, string args)
        {
            if (EventMap.ContainsKey(eventName)) EventMap[eventName]?.Invoke(string.IsNullOrEmpty(args) ? null : args);
        }

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
}
