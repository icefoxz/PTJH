using System;
using System.Linq;
using AOT.Utls;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_池事件名", menuName = "状态玩法/事件/池事件")]
    internal class PoolEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private WeightField[] _options;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var nextEvent = _options.WeightPick().Event;
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => _options.Select(o => o.Event).ToArray();
        public override AdvTypes AdvType => AdvTypes.Pool;
        public override event Action<string[]> OnLogsTrigger;//池事件不触发log

        [Serializable] public class WeightField : IWeightElement
        {
            [SerializeField] private int 权重;
            [SerializeField] private AdvEventSoBase 事件;
            public IAdvEvent Event => 事件;
            public int Weight => 权重;
        }
    }
}