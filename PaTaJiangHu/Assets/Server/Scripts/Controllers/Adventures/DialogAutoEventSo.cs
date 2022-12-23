using System;
using System.Linq;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_对话事件名", menuName = "事件/历练/对话事件")]
    internal class DialogAutoEventSo : AdvAutoEventSoBase
    {
        [SerializeField] private string _name;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private DialogField[] _dialogs;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override event Action<string[]> OnLogsTrigger;
        public override IAdvEvent[] AllEvents => new[] { NextEvent };
        public int TypeId => (int)AdvType;
        public override AdvTypes AdvType => AdvTypes.Dialog;
        private IAdvEvent NextEvent => 下个事件;
        private DialogField[] Dialogs => _dialogs;

        public override IAdvEvent GetNextEvent(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(Dialogs.Select(d => d.GetMessage()).ToArray());
            return NextEvent;
        }

        [Serializable] private class DialogField
        {
            //[ConditionalField(true,nameof(IsCustomizeName))]
            [Header("id {0}是玩家执行单位,{1}是npc名字")]
            [SerializeField]private string _name;
            //[Header("id 0是玩家执行单位,如果设置为-1为自定义名字")][SerializeField]private int _id;
            [SerializeField] [TextArea] private string _message;
            //public int Id => _id;
            public string Name => _name;
            public string Message => _message;
            //private bool IsCustomizeName()
            //{
            //    if (_id < 0) return true;
            //    _name = _id.ToString();
            //    return false;
            //}
            public string GetMessage() => string.Format(_message, "{0}", Name);
        }

    }
}