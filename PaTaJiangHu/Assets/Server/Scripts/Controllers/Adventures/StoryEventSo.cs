using System;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_文本事件名", menuName = "副本/文本事件")]
    internal class StoryEventSo : AdvEventSoBase
    {
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField][TextArea] private string 文本;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent[] PossibleEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Story;
        protected override Action<IAdvEvent> OnResultCallback { get; set; }
        private IAdvEvent Next => 下个事件;
        public string Story => 文本;
        public void NextEvent() => OnResultCallback(Next);
    }
}