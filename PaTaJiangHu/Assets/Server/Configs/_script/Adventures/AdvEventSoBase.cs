using System;
using UnityEngine;

namespace Server.Configs.Adventures
{
    /// <summary>
    /// 事件So父类, <see cref="OnLogsTrigger"/>提供文本信息
    /// </summary>
    internal abstract class AdvEventSoBase : ScriptableObject, IAdvEvent
    {
        public abstract string Name { get; }
        public abstract void EventInvoke(IAdvEventArg arg);
        public abstract event Action<IAdvEvent> OnNextEvent;
        public abstract IAdvEvent[] AllEvents { get; }
        public abstract AdvTypes AdvType { get; }
        public abstract event Action<string[]> OnLogsTrigger;
    }

}