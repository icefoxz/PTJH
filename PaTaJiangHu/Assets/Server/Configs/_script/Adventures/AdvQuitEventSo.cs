using System;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "结束事件",menuName = "事件/结束事件")]
    internal class AdvQuitEventSo :  AdvEventSoBase
    {
        //[Header("副本结束事件请保持一个")]
        //[ReadOnly][SerializeField]private int _id = 1;
        //public override int Id => _id;
        public override string Name { get; } = "结束";
        public override void EventInvoke(IAdvEventArg arg) => OnNextEvent?.Invoke(null);
        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => Array.Empty<IAdvEvent>();
        public override AdvTypes AdvType => AdvTypes.Quit;
        public override event Action<string[]> OnLogsTrigger;
    }
}