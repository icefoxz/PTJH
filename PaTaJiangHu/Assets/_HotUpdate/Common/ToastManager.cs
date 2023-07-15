using System;
using AOT._AOT.Core;
using AOT._AOT.Core.Systems.Messaging;
using AOT._AOT.Views;
using AOT._AOT.Views.Abstract;
using GameClient.Models;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Common
{
    internal class ToastManager : UiManagerBase
    {
        private static ToastManager Instance { get; set; }

        protected override MainUiAgent.Sections Section { get; } = MainUiAgent.Sections.Toast;
        protected override string ViewName { get; } = "common_toast_handler";
        protected override bool IsDynamicPixel { get; } = false;
        private ToastHandler Handler { get; set; }
        private View prefab_toast { get; set; }
        private Faction Faction => Game.World.Faction;
        
        public ToastManager(MainUiAgent uiAgent) : base(uiAgent)
        {
        }
        protected override void Build(IView v)
        {
            Handler = v.GetObject<ToastHandler>("toast_handler");
            prefab_toast = v.GetObject<View>("prefab_toast");
            v.Display(true);
            prefab_toast.Display(false);
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Init, b =>
            {
                SetMessage("测试门派启动!");
            });
            Game.MessagingManager.RegEvent(EventString.Faction_DiziAdd, b =>
            {
                var guid = b.Get<string>(0);
                var dizi = Faction.GetDizi(guid);
                SetMessage($"{dizi.Name}来到山门报道!");
            });
        }

        private void SetMessage(string msg)
        {
            var toast = InstanceToast();
            toast.Set(msg, null, null);
        }

        private Prefab_toast InstanceToast()
        {
            var view = Handler.CreateToast(prefab_toast);
            var toast = new Prefab_toast(view, () => Handler.DestroyToast(view), true);
            return toast;
        }

        public override void Show() { }
        public override void Hide() { }

        private class Prefab_toast : UiBase
        {
            public Text text_message { get; } // 消息文本
            public Image img_icon { get; } // 消息图标
            public Button btn_click { get; }
            public Button btn_x { get; }

            public Prefab_toast(IView v,Action onDestroyAction ,bool display) : base(v, display)
            {
                text_message = v.GetObject<Text>("text_message");
                img_icon = v.GetObject<Image>("img_ico");
                btn_click = v.GetObject<Button>("btn_click");
                btn_x = v.GetObject<Button>("btn_x");
                btn_x.OnClickAdd(onDestroyAction);
            }

            public void Set(string message, Sprite icon, Action onclickAction)
            {
                text_message.text = message;
                img_icon.sprite = icon;
                btn_click.onClick.RemoveAllListeners();
                if (onclickAction != null) btn_click.OnClickAdd(onclickAction);
                gameObject.SetActive(true);
            }
        }
    }
}