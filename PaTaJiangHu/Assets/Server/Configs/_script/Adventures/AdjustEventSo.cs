using System;
using System.Linq;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_调整事件名", menuName = "事件/调整事件")]
    internal class AdjustEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private Adjustment[] 调整;
        [SerializeField] private AdvEventSoBase 下个事件;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;
        private AdvEventSoBase NextEvent => 下个事件;
        private Adjustment[] Adjusts => 调整;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = NextEvent;
            var messages = Adjusts.Select(a => a.Set(arg.Adjustment))
                .Where(s=>!string.IsNullOrWhiteSpace(s)).ToArray();
            if (messages.Any()) OnLogsTrigger?.Invoke(messages);
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => new IAdvEvent[] { NextEvent };
        public override AdvTypes AdvType => AdvTypes.Adjust;
        public override event Action<string[]> OnLogsTrigger;

        [Serializable] public class Adjustment 
        {
            public string Set(IAdjustment adj)
            {
                throw new NotImplementedException();
            }
        }
    }
}