using System;
using System.Linq;
using MyBox;
using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "id_对话事件名", menuName = "副本/对话事件")]
    internal class DialogEventField : AdvEventSoBase
    {
        [SerializeField] private string _name;
        [SerializeField] private AdvEventSoBase 下个事件;
        [SerializeField] private DialogField[] _dialogs;

        //[SerializeField] private int _id;
        //public override int Id => _id;
        public override IAdvEvent[] PossibleEvents => new[] { Next };
        public override AdvTypes AdvType => AdvTypes.Dialog;
        protected override Action<IAdvEvent> OnResultCallback { get; set; }
        private IAdvEvent Next => 下个事件;

        private DialogField[] Dialogs => _dialogs;

        public (int id, string name, string message)[] GetDialogue =>
            Dialogs.Select(d => (d.Id, d.Name, d.Message)).ToArray();

        public void NextEvent() => OnResultCallback(Next);
        [Serializable] private class DialogField
        {
            [ConditionalField(true,nameof(IsCustomizeName))][SerializeField]private string _name;
            [Header("id 0是玩家执行单位,如果设置为-1为自定义名字")][SerializeField]private int _id;
            [SerializeField] [TextArea] private string _message;
            public int Id => _id;
            public string Name => _name;
            public string Message => _message;
            private bool IsCustomizeName()
            {
                if (_id < 0) return true;
                _name = _id.ToString();
                return false;
            }
        }
    }
}