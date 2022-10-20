using System;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    /// <summary>
    /// 副本事件的父类
    /// </summary>
    internal abstract class AdvEventSoBase : ScriptableObject, IAdvEvent
    {
        public int TypeId => (int)AdvType;
        public abstract IAdvEvent[] PossibleEvents { get; }
        public abstract AdvTypes AdvType { get; }
        public void RegEventResult(Action<IAdvEvent> onResultCallback) => OnResultCallback = onResultCallback;
        /// <summary>
        /// 主要让之类调用回调产生后续事件
        /// </summary>
        protected abstract Action<IAdvEvent> OnResultCallback { get; set; }
    }
}