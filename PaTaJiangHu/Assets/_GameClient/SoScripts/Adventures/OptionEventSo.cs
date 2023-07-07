using System;
using System.Linq;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_选择事件名", menuName = "状态玩法/副本/选择事件")]
    internal class OptionEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "选择";
        [SerializeField][TextArea] private string 文本;
        [SerializeField] private OptionField[] _options;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var nexEvent = _options[arg.InteractionResult].Event;
            OnNextEvent?.Invoke(nexEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => _options.Select(o => o.Event).ToArray();
        public override AdvTypes AdvType => AdvTypes.Option;
        public override event Action<string[]> OnLogsTrigger;//选择事件不触发log
        public string Story => 文本;
        public string[] GetOptions => _options.Select(o => o.Title).ToArray();

        [Serializable]private class OptionField
        {
            [SerializeField] private string 描述;
            [SerializeField] private AdvEventSoBase 事件;
            public string Title => 描述;
            public IAdvEvent Event => 事件;
        }
    }
}