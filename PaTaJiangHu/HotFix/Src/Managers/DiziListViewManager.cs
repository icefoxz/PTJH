using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

internal class DiziListViewManager : MainPageBase
{
    private DiziListView DiziList { get; set; }
    private DiziController DiziInteraction { get; } = Game.Controllers.Get<DiziController>();

    public DiziListViewManager(UiManager uiManager) : base(uiManager)
    {
    }

    protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
    protected override string ViewName => "view_diziListView";
    protected override bool IsFixPixel => true;

    protected override void Build(IView view)
    {
        DiziList = new DiziListView(view, key => DiziInteraction.SelectDizi(key));
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, bag => DiziList.UpdateList());
        Game.MessagingManager.RegEvent(EventString.Dizi_Params_StateUpdate, bag => DiziList.UpdateDiziState());
    }

    public override void Show() => DiziList.Display(true);

    public override void Hide() => DiziList.Display(false);

    private class DiziListView : UiBase
    {
        private event Action<string> OnDiziSelectedAction;
        private ListViewUi<DiziPrefab> DiziList { get; }
        private TopRightView TopRight { get; set; }
        private int MaxDizi = 10;

        public DiziListView(IView v, Action<string> onDiziSelectedAction) : base(v.GameObject, true)
        {
            OnDiziSelectedAction = onDiziSelectedAction;
            DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"),
                v.GetObject<ScrollRect>("scroll_diziListView").content.gameObject);
            TopRight = new TopRightView(v.GetObject<View>("view_topRight"));
            TopRight.Set(0, MaxDizi);
        }

        public void UpdateList()
        {
            var list = Game.World.Faction.DiziList.ToList();
            SetList(list);
            TopRight.Set(list.Count, MaxDizi);
        }

        public void UpdateDiziState()//更新弟子状态
        {
            var faction = Game.World.Faction;
            foreach (var ui in DiziList.List) 
                ui.SetStateTitle(faction.GetDizi(ui.DiziGuid).State.ShortTitle);
        }

        private void ClearList() => DiziList.ClearList(ui => ui.Destroy());

        private void SetList(List<Dizi> arg)
        {
            ClearList();
            for (var i = 0; i < arg.Count; i++)
            {
                var dizi = arg[i];
                var ui = DiziList.Instance(v => new DiziPrefab(v, dizi.Guid));
                ui.Init(dizi.Name, () => OnDiziSelectedAction?.Invoke(dizi.Guid));
                ui.SetStateTitle(dizi.State.ShortTitle);
            }
        }

        private class DiziPrefab : UiBase
        {
            private Button Btn_dizi { get; }
            private Text Text_diziName { get; }
            private Text Text_stateTitle { get; }

            public string DiziGuid { get; }

            public DiziPrefab(IView v, string diziGuid) : base(v.GameObject, true)
            {
                DiziGuid = diziGuid;
                Btn_dizi = v.GetObject<Button>("btn_dizi");
                Text_diziName = v.GetObject<Text>("text_diziName");
                Text_stateTitle = v.GetObject<Text>("text_stateTitle");
            }

            public void Init(string name, Action onClickAction)
            {
                Text_diziName.text = name;
                Btn_dizi.OnClickAdd(onClickAction);
                Display(true);
            }

            public void SetStateTitle(string stateText)=> Text_stateTitle.text = stateText;
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
                Text_value.text = value.ToString("00");
                Text_max.text = max.ToString("00");
            }
        }
    }
}