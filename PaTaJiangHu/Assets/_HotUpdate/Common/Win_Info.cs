using System;
using AOT._AOT.Core.Systems.Messaging;
using AOT._AOT.Views.Abstract;
using HotUpdate._HotUpdate.Demo_v1;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Common
{
    internal class Win_Info : WinUiManagerBase
    {
        private static Win_Info Instance { get; set; }
        private Demo_v1Agent Demo_v1Agent { get; }
        private Common_win_info _commonWinInfo;
        public Win_Info(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Demo_v1Agent = uiAgent;
            Instance = this;
        }

        protected override string ViewName => "common_win_info";
        public static void Show(string title, string content)
        {
            Instance._commonWinInfo.Set(title, content);
            Instance.Show();
        }
        protected override void Build(IView view) => _commonWinInfo = new Common_win_info(view, Hide);
        protected override void RegEvents()
        {
        }
        private class Common_win_info : UiBase 
        {
            private Text text_title { get; }
            private Text text_message { get; }
            private Button btn_confirm { get; }
            private Button btn_x { get; }
            public Common_win_info(IView v, Action onCloseAction) : base(v, true)
            {
                text_title = v.GetObject<Text>("text_title");
                text_message = v.GetObject<Text>("text_message");
                btn_confirm = v.GetObject<Button>("btn_confirm");
                btn_x = v.GetObject<Button>("btn_x");
                btn_x.OnClickAdd(onCloseAction);
                btn_confirm.OnClickAdd(onCloseAction);
            }
            public void Set(string title, string content)
            {
                text_title.text = title;
                text_message.text = content;
            }
        }
    }

    internal class Win_Confirm : WinUiManagerBase
    {
        private static Win_Confirm Instance { get; set; }
        private Demo_v1Agent Demo_v1Agent { get; }
        private Common_win_confirm _commonWinConfirm;

        public Win_Confirm(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Demo_v1Agent = uiAgent;
            Instance = this;
        }

        protected override string ViewName => "common_win_confirm";

        public static void Show(string title, string content, Action onConfirmAction)
        {
            Instance._commonWinConfirm.Set(title, content, onConfirmAction);
            Instance.Show();
        }

        protected override void Build(IView view) => _commonWinConfirm = new Common_win_confirm(view, Hide);

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
