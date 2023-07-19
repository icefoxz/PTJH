using System;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.System;
using HotUpdate._HotUpdate.Common;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Win_DiziHouse : WinUiManagerBase
    {
        private Button btn_x { get; set; }
        private Button btn_dismiss { get; set; }
        private Button btn_info { get; set; }
        private ListViewUi<Prefab_dizi> DiziList { get; set; }
        private Faction Faction => Game.World.Faction;
        private FactionController FactionController => Game.Controllers.Get<FactionController>();
        private Demo_v1Agent Agent { get; }
        public Demo_Win_DiziHouse(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
        }
        protected override string ViewName => "demo_win_diziHouse";

        protected override void Build(IView v)
        {
            btn_x = v.GetObject<Button>("btn_x");
            btn_dismiss = v.GetObject<Button>("btn_dismiss");
            btn_info = v.GetObject<Button>("btn_info");
            DiziList = new ListViewUi<Prefab_dizi>(v, "prefab_dizi", "scr_dizi");
            btn_x.OnClickAdd(Hide);
            btn_dismiss.OnClickAdd(OnDismiss);
            btn_info.OnClickAdd(() =>
            {
                var dizi = Faction.DiziList[SelectedDizi];
                Agent.SkillPage_Show(dizi.Guid);
                Hide();
            });
        }
        private int SelectedDizi { get; set; }
        // 驱逐弟子
        private void OnDismiss()
        {
            var dizi = Faction.DiziList[SelectedDizi];
            Win_Confirm.Show("弟子", $"是否驱逐弟子{dizi.Name}?", () => FactionController.DismissDizi(dizi.Guid));
        }

        protected override void RegEvents()
        {
        }

        public override void Show()
        {
            base.Show();
            ShowDiziList();
        }

        private void ShowDiziList()
        {
            DiziList.ClearList(d=>d.Destroy());
            for (var i = 0; i < Faction.DiziList.Count; i++)
            {
                var index = i;
                var dizi = Faction.DiziList[i];
                var ui = DiziList.Instance(v => new Prefab_dizi(v, ()=>SetSelected(index), true));
                Sprite diziIcon = null;
                var color = Game.GetColorFromGrade(dizi.Grade);
                ui.Set(dizi.Name, color, diziIcon);
            }
            SetSelected(0);
        }

        private void SetSelected(int index)
        {
            SelectedDizi = index;
            for (var i = 0; i < DiziList.List.Count; i++)
            {
                var dizi = DiziList.List[i];
                dizi.SetSelected(i == index);
            }
        }

        private class Prefab_dizi : UiBase
        {
            private Text text_diziName { get; }
            private Image img_diziIcon { get; }
            private Image img_selected { get; }
            private Button btn_dizi { get; }
            public Prefab_dizi(IView v,Action onclickAction ,bool display) : base(v, display)
            {
                text_diziName = v.GetObject<Text>("text_diziName");
                img_diziIcon = v.GetObject<Image>("img_diziIcon");
                img_selected = v.GetObject<Image>("img_selected");
                btn_dizi = v.GetObject<Button>("btn_dizi");
                btn_dizi.OnClickAdd(() => onclickAction?.Invoke());
            }

            public void Set(string name, Color nameColor, Sprite icon)
            {
                text_diziName.text = name;
                text_diziName.color = nameColor;
                if (icon != null) img_diziIcon.sprite = icon;
            }

            public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);
        }
    }
}