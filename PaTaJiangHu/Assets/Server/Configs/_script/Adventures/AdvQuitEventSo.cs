using System;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "结束事件",menuName = "事件/结束事件")]
    internal class AdvQuitEventSo :  AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "结束";

        //[Header("副本结束事件请保持一个")]
        //[ReadOnly][SerializeField]private int _id = 1;
        //public override int Id => _id;
        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg) => OnNextEvent?.Invoke(null);
        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => Array.Empty<IAdvEvent>();
        public override AdvTypes AdvType => AdvTypes.Quit;
        public override event Action<string[]> OnLogsTrigger;
    }
}