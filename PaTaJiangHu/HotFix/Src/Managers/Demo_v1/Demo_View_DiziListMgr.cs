using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_DiziListMgr : MainPageBase
    {
        private View_DiziList DiziList { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_diziList";
        protected override bool IsDynamicPixel => true;
        private Demo_v1Agent DemoAgent { get; }
        public Demo_View_DiziListMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            DemoAgent = uiAgent;
        }
        protected override void Build(IView view)
        {
            DiziList = new View_DiziList(view, diziIndex =>
            {
                var dizi = Game.World.Faction.DiziList[diziIndex];
                DemoAgent.SetDiziView(dizi.Guid);
            });
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
            private event Action<int> OnDiziSelected;
            public View_DiziList(IView v,Action<int> onDiziClicked) : base(v, true)
            {
                OnDiziSelected = onDiziClicked;
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
                    var index = i;
                    var ui = DiziList.Instance(v => new DiziPrefab(v, () => OnDiziSelected?.Invoke(index)));
                    ui.Init(dizi.Name, dizi.Grade);
                }
            }

            private void ClearList() => DiziList.ClearList(ui => ui.Destroy());

            private class DiziPrefab : UiBase
            {
                private Image Img_avatar { get; }
                private Text Text_name { get; }
                private Button Btn_dizi { get; }
                public DiziPrefab(IView v,Action onClickAction) : base(v, true)
                {
                    Img_avatar = v.GetObject<Image>("img_avatar");
                    Text_name = v.GetObject<Text>("text_name");
                    Btn_dizi = v.GetObject<Button>("btn_dizi");
                    Btn_dizi.OnClickAdd(onClickAction);
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
