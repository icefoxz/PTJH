using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers;

internal class DiziListViewManager 
{
    private DiziListView DiziList { get; set; }
    private DiziController DiziInteraction { get; } = Game.Controllers.Get<DiziController>();
    public void Init()
    {
        InitUi();
        RegEvents();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, bag => DiziList.UpdateList(bag));
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziListView", v =>
        {
            Game.MainUi.MainPage.Set(v, MainPageLayout.Sections.Btm, true);
            DiziList = new DiziListView(v, key => DiziInteraction.SelectDizi(key));
        });
    }

    private class DiziListView : UiBase
    {
        private event Action<string> OnDiziSelectedAction;
        private ListViewUi<DiziPrefab> DiziList { get; }
        private TopRightView TopRight { get; set; }

        public DiziListView(IView v, Action<string> onDiziSelectedAction) : base(v.GameObject, true)
        {
            OnDiziSelectedAction = onDiziSelectedAction;
            DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"),
                v.GetObject<ScrollRect>("scroll_diziListView").content.gameObject);
            TopRight = new TopRightView(v.GetObject<View>("view_topRight"));
        }


        public void UpdateList(ObjectBag bag)
        {
            var list = Game.World.Faction.DiziList.ToList();
            SetList(list);
        }

        private void ClearList() => DiziList.ClearList(ui => ui.Destroy());

        private void SetList(List<Dizi> arg)
        {
            ClearList();
            for (var i = 0; i < arg.Count; i++)
            {
                var info = arg[i];
                var ui = DiziList.Instance(v => new DiziPrefab(v));
                ui.Init(info.Name, () => OnDiziSelectedAction?.Invoke(info.Guid));
            }
        }

        private class DiziPrefab : UiBase
        {
            private Button Btn_dizi { get; }
            private Text Text_diziName { get; }

            public DiziPrefab(IView v) : base(v.GameObject, true)
            {
                Btn_dizi = v.GetObject<Button>("btn_dizi");
                Text_diziName = v.GetObject<Text>("text_diziName");
            }

            public void Init(string name, Action onClickAction)
            {
                Text_diziName.text = name;
                Btn_dizi.OnClickAdd(onClickAction);
                Display(true);
            }
        }

        private class TopRightView : UiBase
        {
            private Text Text_value { get; }
            private Text Text_max { get; }

            public TopRightView(IView v) : base(v.GameObject, true)
            {
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
            }

            public void Set(int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
            }
        }
    }
}