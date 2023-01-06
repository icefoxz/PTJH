using System;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_文本事件名", menuName = "事件/文本事件")]
    internal class AdvTextEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] [TextArea] private string 文本;
        public override event Action<string[]> OnLogsTrigger;

        public override AdvTypes AdvType => AdvTypes.Story;
        private IAdvEvent Next => 下个事件;
        public string Text => 文本;
        public override IAdvEvent[] AllEvents => new[] { Next };
        //[SerializeField] private int _id;
        //public override int Id => _id;

        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(new[] { Text });
            OnNextEvent?.Invoke(Next);
        }

        public override event Action<IAdvEvent> OnNextEvent;
    }
}