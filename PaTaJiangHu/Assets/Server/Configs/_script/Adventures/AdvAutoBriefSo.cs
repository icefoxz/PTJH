using System;
using System.Linq;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_多文本", menuName = "事件/历练/多文本事件")]
    internal class AdvAutoBriefSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField][TextArea] private string[] 文本;

        private IAdvEvent Next => 下个事件;
        private string[] Story => 文本;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(Story.Select(m => string.Format(m, arg.DiziName)).ToArray());
            OnNextEvent?.Invoke(Next);
        }

        public override event Action<IAdvEvent> OnNextEvent;

        public override IAdvEvent[] AllEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Story;
        public override event Action<string[]> OnLogsTrigger;
    }
}