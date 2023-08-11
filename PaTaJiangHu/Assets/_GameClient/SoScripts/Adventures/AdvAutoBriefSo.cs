using System;
using System.Linq;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_多文本", menuName = "状态玩法/事件/多文本事件")]
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

        protected override IAdvEvent OnEventInvoke(IAdvEventArg arg)
        {
            ProcessLogs(Story.Select(m => string.Format(m, arg.DiziName)).ToArray());
            return Next;
        }

        public override IAdvEvent[] AllEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Story;
    }
}