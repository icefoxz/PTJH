using System;
using System.Linq;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_选择事件名", menuName = "事件/副本/选择事件")]
    internal class OptionEventSo : AdvInterEventSoBase
    {
        [SerializeField][TextArea] private string 文本;
        [SerializeField]private OptionField[] _options;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent GetNextEvent(IAdvEventArg arg) => _options[arg.Result].Event;
        public override IAdvEvent[] AllEvents => _options.Select(o => o.Event).ToArray();
        public override AdvTypes AdvType => AdvTypes.Option;
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