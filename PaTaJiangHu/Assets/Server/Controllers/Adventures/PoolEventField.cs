using System;
using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_池事件名", menuName = "副本/池事件")]
    internal class PoolEventField : AdvEventSoBase
    {
        [SerializeField] private WeightField[] _options;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent[] PossibleEvents => _options.Select(o => o.Event).ToArray();
        public override AdvTypes AdvType => AdvTypes.Pool;
        protected override Action<IAdvEvent> OnResultCallback { get; set; }
        public void NextEvent() => OnResultCallback(_options.WeightPick().Event);

        [Serializable] public class WeightField : IWeightElement
        {
            [SerializeField] private int 权重;
            [SerializeField] private AdvEventSoBase 事件;
            public IAdvEvent Event => 事件;
            public int Weight => 权重;
        }
    }
}