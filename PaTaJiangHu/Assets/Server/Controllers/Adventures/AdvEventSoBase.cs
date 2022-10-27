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
        /// <summary>
        /// 注册事件回调，并且事件只执行一次
        /// </summary>
        /// <param name="onResultCallback"></param>
        public void RegEventResult(Action<IAdvEvent> onResultCallback)
        {
            OnResultCallback = advEvent =>
            {
                onResultCallback?.Invoke(advEvent);
                OnResultCallback = null;
            };
        }

        /// <summary>
        /// 主要让之类调用回调产生后续事件
        /// </summary>
        protected abstract Action<IAdvEvent> OnResultCallback { get; set; }
    }
}