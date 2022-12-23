using System;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    /// <summary>
    /// 事件So父类,透过ID获取下一个事件
    /// </summary>
    internal abstract class AdvEventSoBase : ScriptableObject, IAdvEvent
    {
        public abstract IAdvEvent GetNextEvent(IAdvEventArg arg);
        public abstract IAdvEvent[] AllEvents { get; }
        public abstract AdvTypes AdvType { get; }
    }

    /// <summary>
    /// 副本事件的父类, 其中事件模式是透过交互式获得事件Id
    /// </summary>
    internal abstract class AdvInterEventSoBase : AdvEventSoBase
    {
    }

    /// <summary>
    /// 自动事件的父类, 自动获取下个事件Id, 并且<see cref="OnLogsTrigger"/>提供文本信息
    /// </summary>
    internal abstract class AdvAutoEventSoBase : AdvEventSoBase, IAdvAutoEvent
    {
        public abstract event Action<string[]> OnLogsTrigger;
    }
}