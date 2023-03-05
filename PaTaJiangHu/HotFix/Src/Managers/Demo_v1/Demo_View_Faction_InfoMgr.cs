using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using System;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_View_Faction_InfoMgr : UiManagerBase
    {
        private View_Faction_Info View_faction_info { get; set; }
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Top;
        protected override string ViewName => "demo_view_faction_info";
        protected override bool IsDynamicPixel => true;

        public Demo_View_Faction_InfoMgr(MainUiAgent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            View_faction_info = new View_Faction_Info(view);
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Init,
                bag =>
                {
                    View_faction_info.SetFaction();
                    Show();
                });
            Game.MessagingManager.RegEvent(EventString.Faction_SilverUpdate,
                bag => View_faction_info.SetSilver(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_YuanBaoUpdate,
                bag => View_faction_info.SetYuanBao(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_FoodUpdate,
                bag => View_faction_info.SetFood(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_WineUpdate,
                bag => View_faction_info.SetWine(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_PillUpdate,
                bag => View_faction_info.SetPill(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_HerbUpdate,
                bag => View_faction_info.SetHerb(bag.GetInt(0)));
            Game.MessagingManager.RegEvent(EventString.Faction_Params_ActionLingUpdate,
                bag => View_faction_info.SetActionToken(bag.GetInt(0), bag.GetInt(1), bag.GetInt(2), bag.GetInt(3)));
        }

        public override void Show() => View_faction_info.Display(true);
        public override void Hide() => View_faction_info.Display(false);

        private class View_Faction_Info : UiBase
        {
            private Text Text_faction { get; }
            private Image Img_faction { get; }
            private Element Element_Food { get; }
            private Element Element_Wine { get; }
            private Element Element_Silver { get; }
            private Element Element_Pill { get; }
            private Element Element_Herb { get; }
            private Element Element_YuanBao { get; }
            private View_Ling FactionLing { get; }
            private View_Time LingTime { get; }
            public View_Faction_Info(IView v) : base(v, true)
            {
                Text_faction = v.GetObject<Text>("text_faction");
                Img_faction = v.GetObject<Image>("img_faction");
                Element_Food = new Element(v.GetObject<View>("element_food"));
                Element_Wine = new Element(v.GetObject<View>("element_wine"));
                Element_Silver = new Element(v.GetObject<View>("element_silver"));
                Element_Pill = new Element(v.GetObject<View>("element_pill"));
                Element_Herb = new Element(v.GetObject<View>("element_herb"));
                Element_YuanBao = new Element(v.GetObject<View>("element_yuanBao"));
                FactionLing = new View_Ling(v.GetObject<View>("view_ling"));
                LingTime = new View_Time(v.GetObject<View>("view_time"));
            }
            public void SetFaction()
            {
                var f = Game.World.Faction;
                Element_Food.SetText(f.Food);
                Element_Wine.SetText(f.Wine);
                Element_Silver.SetText(f.Silver);
                Element_Pill.SetText(f.Pill);
                Element_Herb.SetText(f.Herb);
                Element_YuanBao.SetText(f.YuanBao);
                FactionLing.SetToken(f.ActionLing, f.ActionLing);
                LingTime.SetTimer(0, 0);
            }
            public void SetText(string fName) => Text_faction.text = fName;
            public void SetImage(Sprite fImg) => Img_faction.sprite = fImg;
            public void SetFood(int food) => Element_Food.SetText(food);
            public void SetWine(int wine) => Element_Wine.SetText(wine);
            public void SetSilver(int silver) => Element_Silver.SetText(silver);
            public void SetPill(int pill) => Element_Pill.SetText(pill);
            public void SetHerb(int herb) => Element_Herb.SetText(herb);
            public void SetYuanBao(int yuanbao) => Element_YuanBao.SetText(yuanbao);
            public void SetActionToken(int value, int max, int min, int sec)
            {
                FactionLing.SetToken(value, max);
                LingTime.SetTimer(min, sec);
            }

            private class Element : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_value { get; }
                public Element(IView v) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_value = v.GetObject<Text>("text_value");
                }
                public void SetIcon(Sprite ico) => Img_ico.sprite = ico;
                public void SetText(int value) => Text_value.text = value.ToString();
            }

            private class View_Ling : UiBase
            {
                private Text Text_value { get; }
                private Text Text_max { get; }
                public View_Ling(IView v) : base(v, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                }
                public void SetToken(int value, int max)
                {
                    Text_value.text = value.ToString();
                    Text_max.text = max.ToString();
                }
            }

            private class View_Time : UiBase
            {
                private Text Text_mins { get; }
                private Text Text_secs { get; }
                public View_Time(IView v) : base(v, true)
                {
                    Text_mins = v.GetObject<Text>("text_mins");
                    Text_secs = v.GetObject<Text>("text_secs");
                }
                public void SetTimer(int min, int sec)
                {
                    Text_mins.text = EmptyIfZero(min);
                    Text_secs.text = EmptyIfZero(sec);
                }
                private string EmptyIfZero(int value) => value==0 ? string.Empty : value.ToString();
            }
        }
    }
}
