using System;
using AOT.Core.Dizi;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    //弟子到访窗口
    internal class Demo_Win_VisitorDizi : WinUiManagerBase
    {
        private Button btn_x { get; set;}
        private View_diziInfo view_diziInfo { get; set;}
        private Element_trade element_tradeBuy { get; set;}
        private Element_trade element_tradeSell { get; set; }
        private RecruitController RecruitController => Game.Controllers.Get<RecruitController>();

        public Demo_Win_VisitorDizi(MainUiAgent uiAgent) : base(uiAgent)
        {
        }

        protected override string ViewName => "demo_win_visitorDizi";
        protected override void Build(IView view)
        {
            btn_x = view.GetObject<Button>("btn_x");
            btn_x.OnClickAdd(Hide);
            view_diziInfo = new View_diziInfo(view.GetObject<View>("view_diziInfo"), true);
            element_tradeBuy = new Element_trade(view.GetObject<View>("element_tradeBuy"), BuyVisitor, true);
            element_tradeSell = new Element_trade(view.GetObject<View>("element_tradeSell"), SellVisitor, true);
        }
        protected override void RegEvents()
        {
        }

        public void Set(Dizi dizi, IVisitorDizi set)
        {
            var color = Game.GetColorFromGrade(dizi.Grade);
            Sprite icon = null;
            view_diziInfo.Set(dizi.Name, color, icon);
            element_tradeBuy.Set(icon, set.Buy.ToString());
            element_tradeSell.Set(icon, set.Sell.ToString());
            Show();
        }

        private void SellVisitor()
        {
            RecruitController.SellVisitor();
            Hide();
        }

        private void BuyVisitor()
        {
            if (RecruitController.BuyVisitor()) Hide();
        }


        private class View_diziInfo : UiBase
        {
            private Image img_diziIcon { get; }
            private Text text_diziName { get; }
            public View_diziInfo(IView v, bool display) : base(v, display)
            {
                img_diziIcon = v.GetObject<Image>("img_diziIcon");
                text_diziName = v.GetObject<Text>("text_diziName");
            }

            public void Set(string diziName, Color nameColor ,Sprite diziIcon)
            {
                text_diziName.text = diziName;
                text_diziName.color = nameColor;
                if (diziIcon != null) img_diziIcon.sprite = diziIcon;
            }
        }

        private class Element_trade : UiBase
        {
            private Button btn_trade { get; }
            private Image img_cost { get; }
            private Text text_cost { get; }
            public Element_trade(IView v,Action onclickAction ,bool display) : base(v, display)
            {
                btn_trade = v.GetObject<Button>("btn_trade");
                img_cost = v.GetObject<Image>("img_cost");
                text_cost = v.GetObject<Text>("text_cost");
                btn_trade.OnClickAdd(onclickAction);
            }

            public void Set(Sprite costIcon, string costNum)
            {
                img_cost.sprite = costIcon;
                text_cost.text = costNum;
            }
        }
    }
}