using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_对话事件名", menuName = "事件/对话事件")]
    internal class AdvDialogEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private DialogField[] _dialogs;

        public override AdvTypes AdvType => AdvTypes.Dialog;
        public override event Action<string[]> OnLogsTrigger;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            OnLogsTrigger?.Invoke(Dialogs.Select(d => d.GetMessage(arg.DiziName)).ToArray());
            OnNextEvent?.Invoke(NextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;
        public override IAdvEvent[] AllEvents => new[] { NextEvent };
        private IAdvEvent NextEvent => 下个事件;
        private DialogField[] Dialogs => _dialogs;
        public IEnumerable<(string name, string message)> GetDialogue => Dialogs.Select(d => (d.NpcName, d.Message));

        [Serializable] private class DialogField
        {
            //[ConditionalField(true,nameof(IsCustomizeName))]
            [Header("id {0}是玩家执行单位,{1}是npc名字")]
            [SerializeField]private string _npcName;
            //[Header("id 0是玩家执行单位,如果设置为-1为自定义名字")][SerializeField]private int _id;
            [SerializeField] [TextArea] private string _message;

            private const char Paragraph = '\n';
            //public int Id => _id;
            public string NpcName => _npcName;
            public string Message => _message;
            //private bool IsCustomizeName()
            //{
            //    if (_id < 0) return true;
            //    _name = _id.ToString();
            //    return false;
            //}
            public string GetMessage(string charName) => string.Format(_message, charName + Paragraph , NpcName + Paragraph);
        }
    }
}