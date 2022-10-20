using System;
using MyBox;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "副本结束事件",menuName = "副本/结束事件")]
    internal class AdvQuitEventSo : AdvEventSoBase, IAdvEvent
    {
        //[Header("副本结束事件请保持一个")]
        //[ReadOnly][SerializeField]private int _id = 1;
        //public override int Id => _id;
        public override IAdvEvent[] PossibleEvents => Array.Empty<IAdvEvent>();
        public override AdvTypes AdvType => AdvTypes.Quit;

        protected override Action<IAdvEvent> OnResultCallback
        {
            get => throw new NotImplementedException("结束事件不允许调用结果回调！");
            set => throw new NotImplementedException("结束事件不允许调用结果回调！");
        }
    }
}