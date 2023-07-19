using System;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
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

        public override void Hide()
        {
            View.Hide();
            HidePanel();
            // 信息窗口不需要关闭其它窗口(实现窗口叠加), 所以不调用base.Hide()
        }

        protected override void Build(IView view) => _commonWinInfo = new Common_win_info(view, Hide);
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Init, _ => Show("测试", "测试门派初始化!"));
            Game.MessagingManager.RegEvent(EventString.Info_Trade_Failed_Silver, _ => Show("交易失败", "银两不足!"));
            Game.MessagingManager.RegEvent(EventString.Info_Trade_Failed_YuanBao, _ => Show("交易失败", "元宝不足!"));
            Game.MessagingManager.RegEvent(EventString.Info_DiziAdd_LimitReached, _ => Show("招募失败", "当前弟子已达上限!"));
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
