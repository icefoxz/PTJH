﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_对话事件名", menuName = "状态玩法/事件/对话事件")]
    internal class AdvDialogEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private DialogField[] _dialogs;

        public override AdvTypes AdvType => AdvTypes.Dialog;
        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override string Name => 事件名;

        protected override IAdvEvent OnEventInvoke(IAdvEventArg arg)
        {
            ProcessLogs(Dialogs.Select(d => d.GetMessage(arg.DiziName)).ToArray());
            return NextEvent;
        }

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