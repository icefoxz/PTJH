using System;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "结束事件",menuName = "状态玩法/事件/结束事件")]
    internal class AdvQuitEventSo :  AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "结束";
        [SerializeField] private bool 强制历练结束;
        [SerializeField] private bool 标记历练失败;
        [SerializeField][TextArea] private string 文本;
        [SerializeField] private bool 标记失踪;

        //[Header("副本结束事件请保持一个")]
        //[ReadOnly][SerializeField]private int _id = 1;
        //public override int Id => _id;
        public override string Name => 事件名;

        protected override IAdvEvent OnEventInvoke(IAdvEventArg arg)
        {
            if (!string.IsNullOrWhiteSpace(Message))
                ProcessLogs(new[] { string.Format(Message, arg.DiziName) });
            return null;
        }

        public override IAdvEvent[] AllEvents => Array.Empty<IAdvEvent>();
        public override AdvTypes AdvType => AdvTypes.Quit;

        public bool IsForceQuit => 强制历练结束;
        public bool IsAdvFailed => 标记历练失败;
        public bool IsLost => 标记失踪;

        private string Message => 文本;
    }
}