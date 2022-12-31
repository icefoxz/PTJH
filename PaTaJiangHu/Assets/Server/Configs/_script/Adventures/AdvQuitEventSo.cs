using System;
using UnityEngine;

namespace Server.Configs._script.Adventures
{
    [CreateAssetMenu(fileName = "结束事件",menuName = "事件/结束事件")]
    internal class AdvQuitEventSo :  AdvEventSoBase
    {
        //[Header("副本结束事件请保持一个")]
        //[ReadOnly][SerializeField]private int _id = 1;
        //public override int Id => _id;
        public override IAdvEvent GetNextEvent(IAdvEventArg arg) => null;
        public override IAdvEvent[] AllEvents => Array.Empty<IAdvEvent>();
        public override AdvTypes AdvType => AdvTypes.Quit;
    }
}