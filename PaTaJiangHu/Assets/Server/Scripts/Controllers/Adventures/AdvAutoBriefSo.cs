using System;
using System.Linq;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_文本", menuName = "事件/历练/文本事件")]
    internal class AdvAutoBriefSo : AdvAutoEventSoBase
    {
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField][TextArea] private string[] 文本;

        private IAdvEvent Next => 下个事件;
        private string[] Story => 文本;
        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent GetNextEvent(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(Story);
            return Next;
        }

        public override IAdvEvent[] AllEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Story;
        public override event Action<string[]> OnLogsTrigger;
    }
}