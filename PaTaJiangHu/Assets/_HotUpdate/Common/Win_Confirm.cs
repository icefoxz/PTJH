using System;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using HotUpdate._HotUpdate.Demo_v1;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Common
{
    internal class Win_Confirm : WinUiManagerBase
    {
        private static Win_Confirm Instance { get; set; }
        private Demo_v1Agent Demo_v1Agent { get; }
        private Common_win_confirm _commonWinConfirm;
        private static event Action OnCancelAction;

        public Win_Confirm(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Demo_v1Agent = uiAgent;
            Instance = this;
        }

        protected override string ViewName => "common_win_confirm";

        public static void Show(string title, string content, Action onConfirmAction) =>
            Show(title, content, onConfirmAction, null);
        public static void Show(string title, string content, Action onConfirmAction,Action onCancelAction)
        {
            OnCancelAction = onCancelAction;
            Instance._commonWinConfirm.Set(title, content, () =>
            {
                onConfirmAction?.Invoke();
                Instance.Hide();
            });
            Instance.Show();
        }

        protected override void Build(IView view) => _commonWinConfirm = new Common_win_confirm(view, OnCancelClick);

        private void OnCancelClick()
        {
            OnCancelAction?.Invoke();
            Hide();
        }

        protected override void RegEvents()
        {
        }

        private class Common_win_confirm : UiBase
        {
            private Text text_title { get; }
            private Text text_message { get; }
            private Button btn_confirm { get; }
            private Button btn_cancel { get; }

            public Common_win_confirm(IView v, Action onCloseAction) : base(v, true)
            {
                text_title = v.GetObject<Text>("text_title");
                text_message = v.GetObject<Text>("text_message");
                btn_confirm = v.GetObject<Button>("btn_confirm");
                btn_cancel = v.GetObject<Button>("btn_cancel");
                btn_cancel.OnClickAdd(onCloseAction);
            }

            public void Set(string title, string content, Action onConfirmAction)
            {
                text_title.text = title;
                text_message.text = content;
                btn_confirm.OnClickAdd(onConfirmAction);
            }
        }
    }
}