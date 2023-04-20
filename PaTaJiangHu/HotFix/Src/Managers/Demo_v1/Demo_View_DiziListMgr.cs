using System;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Models;
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
            DiziList = new View_DiziList(view, dizi =>
            {
                DemoAgent.SetDiziView(dizi.Guid);
            });
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Init, bag => DiziList.SetElements());
            Game.MessagingManager.RegEvent(EventString.Dizi_Params_StateUpdate, b => DiziList.SetElements());
            Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, bag =>
            {
                DiziList.UpdateList();
                DiziList.SetElements();
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
            private Element Elm_lost { get; }
            private event Action<Dizi> OnDiziSelected;
            private string SelectedDiziGuid { get; set; }
            private Element[] AllFilter { get; }

            //cache 当前弟子列表
            private Dizi[] CurrentList { get; set; }

            public View_DiziList(IView v,Action<Dizi> onDiziClicked) : base(v, true)
            {
                OnDiziSelected = onDiziClicked;
                Scroll_dizi = v.GetObject<ScrollRect>("scroll_dizi");
                DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"), Scroll_dizi);
                Elm_all = new Element(v.GetObject<View>("element_all"), () =>
                {
                    SetFilterSelected(Elm_all);
                    UpdateList(GetAllDizi());
                });
                Elm_idle = new Element(v.GetObject<View>("element_idle"), () =>
                {
                    SetFilterSelected(Elm_idle);
                    UpdateList(GetIdleDizi());
                });
                Elm_production = new Element(v.GetObject<View>("element_production"), () =>
                {
                    SetFilterSelected(Elm_production);
                    UpdateList(GetProductionDizi());
                });
                Elm_adventure = new Element(v.GetObject<View>("element_adventure"), () =>
                {
                    SetFilterSelected(Elm_adventure);
                    UpdateList(GetAdventureDizi());
                });
                Elm_lost = new Element(v.GetObject<View>("element_lost"), () =>
                {
                    SetFilterSelected(Elm_lost);
                    UpdateList(GetLostDizi());
                });
                AllFilter = new[]
                {
                    Elm_all,
                    Elm_idle,
                    Elm_adventure,
                    Elm_production,
                    Elm_lost
                };
            }

            private Dizi[] GetLostDizi() => Game.World.Faction.DiziList
                .Where(d=>d.State.Current == DiziStateHandler.States.Lost)
                .ToArray();
            private Dizi[] GetProductionDizi()=> Game.World.Faction.DiziList
                .Where(d=>d.State.Current == DiziStateHandler.States.AdvProduction)
                .ToArray();
            private Dizi[] GetAdventureDizi() => Game.World.Faction.DiziList
                .Where(d => d.State.Current == DiziStateHandler.States.AdvProgress ||
                            d.State.Current == DiziStateHandler.States.AdvReturning)
                .ToArray();
            private Dizi[] GetAllDizi() => Game.World.Faction.DiziList.ToArray();

            private Dizi[] GetIdleDizi() => Game.World.Faction.DiziList
                .Where(d => d.State.Current == DiziStateHandler.States.AdvWaiting ||
                            d.State.Current == DiziStateHandler.States.Idle)
                .ToArray();

            private void SetFilterSelected(Element element)
            {
                foreach (var e in AllFilter) e.SetSelected(e == element);
            }

            public void SetElements()
            {
                var faction = Game.World.Faction;
                var list = faction.DiziList.ToList();
                Elm_all.SetValue(list.Count, faction.MaxDizi);
                Elm_idle.SetValue(GetIdleDizi().Length, faction.MaxDizi);
                Elm_production.SetValue(GetProductionDizi().Length, faction.MaxDizi);
                Elm_adventure.SetValue(GetAdventureDizi().Length, faction.MaxDizi);
                Elm_lost.SetValue(GetLostDizi().Length, faction.MaxDizi);
            }

            public void UpdateList(Dizi[] list = null)
            {
                if(list == null)
                    list = Game.World.Faction.DiziList.ToArray();
                CurrentList = list;
                ClearList();
                for(var i = 0; i< list.Length; i++)
                {
                    var dizi = list[i];
                    var guid = dizi.Guid;
                    var index = i;
                    var ui = DiziList.Instance(v => new DiziPrefab(v, () =>
                    {
                        OnDiziSelected?.Invoke(CurrentList[index]);
                        SetSelected(guid);
                    }));
                    ui.Init(dizi);
                }
            }
            private void SetSelected(string diziGuid)
            {
                SelectedDiziGuid = Game.World.Faction.GetDizi(diziGuid)?.Guid 
                                   ?? string.Empty;//如果弟子不在了,是不可选中的
                foreach (var ui in DiziList.List) 
                    ui.SetSelected(ui.DiziGuid == SelectedDiziGuid);
            }

            private void ClearList()
            {
                DiziList.ClearList(ui => ui.Destroy());
                SelectedDiziGuid = null;
            }

            private class DiziPrefab : UiBase
            {
                private Image Img_avatar { get; }
                private Text Text_name { get; }
                private Button Btn_dizi { get; }
                private Image Img_select { get; }
                public string DiziGuid { get; private set; }

                public DiziPrefab(IView v,Action onClickAction) : base(v, true)
                {
                    Img_avatar = v.GetObject<Image>("img_avatar");
                    Text_name = v.GetObject<Text>("text_name");
                    Btn_dizi = v.GetObject<Button>("btn_dizi");
                    Img_select = v.GetObject<Image>("img_select");
                    Btn_dizi.OnClickAdd(onClickAction);
                }
                public void SetIcon(Sprite ico) => Img_avatar.sprite = ico;
                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
                public void Init(Dizi dizi)
                {
                    Text_name.text = dizi.Name;
                    DiziGuid = dizi.Guid;
                    Text_name.color = Game.GetColorFromGrade(dizi.Grade);
                    SetSelected(false);
                    Display(true);
                } 
            }

            private class Element : UiBase
            {
                private Text Text_value { get; }
                private Text Text_max { get; }
                private Button Btn_filter { get; }
                private Image Img_select { get; }

                public Element(IView v, Action onFilterAction) : base(v, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                    Btn_filter = v.GetObject<Button>("btn_filter");
                    Btn_filter.OnClickAdd(onFilterAction);
                    Img_select = v.GetObject<Image>("img_select");
                }

                public void SetValue(int value, int max)
                {
                    Text_value.text = value.ToString();
                    Text_max.text = max.ToString();
                }

                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
            }
        }
    }
}
