using System;
using GameClient.Modules.DiziM;
using MyBox;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    /// <summary>
    /// 事件So父类, <see cref="OnLogsTrigger"/>提供文本信息
    /// </summary>
    internal abstract class AdvEventSoBase : ScriptableObject, IAdvEvent
    {
        protected bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private AdvEventSoBase So;

        public abstract string Name { get; }
        public abstract void EventInvoke(IAdvEventArg arg);
        public abstract event Action<IAdvEvent> OnNextEvent;
        public virtual event Action<string[]> OnAdjustmentEvent;
        public virtual event Action<IGameReward> OnRewardEvent;
        public abstract IAdvEvent[] AllEvents { get; }
        public abstract AdvTypes AdvType { get; }
        public abstract event Action<string[]> OnLogsTrigger;

        protected void InvokeAdjustmentEvent(string[] adjustments) =>
            OnAdjustmentEvent?.Invoke(adjustments);
        protected void InvokeRewardEvent(IGameReward reward)=> OnRewardEvent?.Invoke(reward);
    }

}