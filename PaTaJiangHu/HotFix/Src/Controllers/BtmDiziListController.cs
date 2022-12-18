using System;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Controllers;

internal class BtmDiziListController 
{
    private DiziListView DiziList { get; set; }
    private TopRightView TopRight { get; set; }

    public BtmDiziListController()
    {
        Game.UiBuilder.Build("view_diziListView", v =>
        {
            DiziList = new DiziListView(v);
            TopRight = new TopRightView(v);
        });

    }
    private class DiziListView : UiBase
    {
        private ListViewUi<DiziPrefab> DiziList { get; }
        public DiziListView(IView v) : base(v.GameObject, true)
        {
            DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"),
                v.GetObject<ScrollRect>("scroll_diziListView").content.gameObject);
        }

        public void ClearList() => DiziList.ClearList(ui => ui.Destroy());

        public void SetList((string name, Action onDiziSelected)[] arg)
        {
            ClearList();
            foreach (var (name, onDiziSelected) in arg)
            {
                var ui = DiziList.Instance(v => new DiziPrefab(v));
                ui.Init(name, onDiziSelected);
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