using System;
using System.Linq;
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
        protected abstract IAdvEvent OnEventInvoke(IAdvEventArg arg);
        public void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = OnEventInvoke(arg);
            if (arg.ExtraMessages.Count > 0)
                OnLogsTrigger?.Invoke(arg.ExtraMessages.ToArray());
            OnNextEvent?.Invoke(nextEvent);
        }
        public event Action<IAdvEvent> OnNextEvent;
        public event Action<string[]> OnAdjustmentEvent;
        public event Action<IGameReward> OnRewardEvent;
        public event Action<string[]> OnLogsTrigger;
        public abstract IAdvEvent[] AllEvents { get; }
        public abstract AdvTypes AdvType { get; }

        protected void ProcessAdjustment(string[] adjustments) => OnAdjustmentEvent?.Invoke(adjustments);
        protected void ProcessReward(IGameReward reward)=> OnRewardEvent?.Invoke(reward);
        protected void ProcessLogs(string[] logs) => OnLogsTrigger?.Invoke(logs);
    }

}