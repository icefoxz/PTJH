using System;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    /// <summary>
    /// 故事
    /// </summary>
    [CreateAssetMenu(fileName = "id_故事名", menuName = "副本/故事")]
    internal class AdvInterStorySo : AdvStorySoBase, IAdvInterStory
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;

        [MustBeAssigned] [ConditionalField(true, nameof(RefreshAllEvent))] [SerializeField] private AdvEventSoBase 事件;

        public int Id => _id;
        public string Name => _name;
        public IAdvEvent StartAdvEvent => 事件;
        public IAdvEvent[] AllAdvEvents
        {
            get
            {
                var interEvents = AllEvents.ToArray();
                if (interEvents.Length != AllEvents.Length)
                    throw new InvalidCastException(
                        $"[{name}]为交互故事, 其中却包涵非交互式事件, 请确保所有事件继承交互事件接口:{nameof(IAdvEvent)}!");
                return interEvents;
            }
        }

        protected override IAdvEvent BeginEvent => StartAdvEvent;
    }
}