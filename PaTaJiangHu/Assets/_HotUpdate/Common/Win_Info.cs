using System;
using AOT._AOT.Core;
using AOT._AOT.Core.Systems.Messaging;
using AOT._AOT.Views.Abstract;
using GameClient.System;
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
            Game.MessagingManager.RegEvent(EventString.Faction_Init, _ => Show("测试", "测试门派初始化!"));
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
}
