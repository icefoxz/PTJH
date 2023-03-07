using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_View_DiziListMgr : MainPageBase
    {
        private View_DiziList DiziList { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_diziList";
        protected override bool IsDynamicPixel => true;
        public Demo_View_DiziListMgr(MainUiAgent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            DiziList = new View_DiziList(view);
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Init, bag => DiziList.SetElement());
            Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, bag =>
            {
                DiziList.UpdateList();
                DiziList.SetElement();
            });
        }
        public override void Show() => DiziList.Display(true);
        public override void Hide() => DiziList.Display(false);

        private class View_DiziList : UiBase
        {
            private ScrollRect Scroll_dizi { get; }
            private ListViewUi<DiziPrefab> DiziList { get; }
            private Element Elm_all { get; }
            private Element Elm_idle { get; }
            private Element Elm_production { get; }
            private Element Elm_adventure { get; }
            public View_DiziList(IView v) : base(v, true)
            {
                Scroll_dizi = v.GetObject<ScrollRect>("scroll_dizi");
                DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"), Scroll_dizi);
                Elm_all = new Element(v.GetObject<View>("element_all"));
                Elm_idle = new Element(v.GetObject<View>("element_idle"));
                Elm_production = new Element(v.GetObject<View>("element_production"));
                Elm_adventure = new Element(v.GetObject<View>("element_adventure"));
            }
            public void SetElement()
            {
                var faction = Game.World.Faction;
                var list = faction.DiziList.ToList();
                Elm_all.SetValue(list.Count, faction.MaxDizi);
                Elm_idle.SetValue(0, faction.MaxDizi);
                Elm_production.SetValue(0, faction.MaxDizi);
                Elm_adventure.SetValue(0, faction.MaxDizi);
            }
            public void UpdateList()
            {
                var list = Game.World.Faction.DiziList.ToList();
                SetList(list);
            }

            private void SetList(List<Dizi> arg)
            {
                ClearList();
                for(var i = 0; i< arg.Count; i++)
                {
                    var dizi = arg[i];
                    var ui = DiziList.Instance(v => new DiziPrefab(v));
                    ui.Init(dizi.Name, dizi.Grade);
                }
            }

            private void ClearList() => DiziList.ClearList(ui => ui.Destroy());

            private class DiziPrefab : UiBase
            {
                private Image Img_avatar { get; }
                private Text Text_name { get; }
                public DiziPrefab(IView v) : base(v, true)
                {
                    Img_avatar = v.GetObject<Image>("img_avatar");
                    Text_name = v.GetObject<Text>("text_name");
                }
                public void SetIcon(Sprite ico) => Img_avatar.sprite = ico;
                public void Init(string name, int grade)
                {
                    Text_name.text = name;
                    Text_name.color = Game.GetColorFromGrade(grade);
                    Display(true);
                } 
            }

            private class Element : UiBase
            {
                private Text Text_value { get; }
                private Text Text_max { get; }
                public Element(IView v) : base(v, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                }
                public void SetValue(int value, int max)
                {
                    Text_value.text = value.ToString();
                    Text_max.text = max.ToString();
                }
            }
        }
    }
}
