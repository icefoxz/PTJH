using System;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_池事件名", menuName = "事件/池事件")]
    internal class PoolEventSo : AdvEventSoBase
    {
        [SerializeField] private WeightField[] _options;
        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent GetNextEvent(IAdvEventArg arg) => _options.WeightPick().Event;
        public override IAdvEvent[] AllEvents => _options.Select(o => o.Event).ToArray();
        public override AdvTypes AdvType => AdvTypes.Pool;

        [Serializable] public class WeightField : IWeightElement
        {
            [SerializeField] private int 权重;
            [SerializeField] private AdvEventSoBase 事件;
            public IAdvEvent Event => 事件;
            public int Weight => 权重;
        }
    }
}