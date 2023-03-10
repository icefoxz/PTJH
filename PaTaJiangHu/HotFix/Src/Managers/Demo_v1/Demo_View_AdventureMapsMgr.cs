﻿using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_View_AdventureMapsMgr : MainPageBase
    {
        private View_AdventureMaps AdventureMaps { get; set; }
        private DiziAdvController DiziAdvController { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_adventureMaps";
        protected override bool IsDynamicPixel => true;
        public Demo_View_AdventureMapsMgr(MainUiAgent uiAgent) : base(uiAgent)
        {
            DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        }
        protected override void Build(IView view)
        {
            AdventureMaps = new View_AdventureMaps(view,
                () => XDebug.LogWarning("更新历练地图"),
                onAdvStartAction: (guid, index) => DiziAdvController.AdventureStart(guid, index)
                );
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag =>  //Temporary
            {
                AdventureMaps.ListMap(DiziAdvController.AutoAdvMaps());
                Show();
            });
        }
        public override void Show() => AdventureMaps.Display(true);
        public override void Hide() => AdventureMaps.Display(false);

        private class View_AdventureMaps : UiBase
        {
            private ScrollRect Scroll_mapSelector { get; }
            private ListViewUi<Prefab_map> MapView { get; }
            private Button Btn_refersh { get; }
            private Button Btn_action { get; }
            private Button Btn_cancel { get; }
            public View_AdventureMaps(IView v,
                Action onRefreshAction,
                Action<string, int> onAdvStartAction
                ) : base(v, false)
            {
                Scroll_mapSelector = v.GetObject<ScrollRect>("scroll_mapSelector");
                MapView = new ListViewUi<Prefab_map>(v.GetObject<View>("prefab"), Scroll_mapSelector);
                Btn_refersh = v.GetObject<Button>("btn_refresh");
                Btn_refersh.OnClickAdd(onRefreshAction);
                Btn_action = v.GetObject<Button>("btn_action");
                Btn_action.OnClickAdd(() => onAdvStartAction?.Invoke(SelectedDizi?.Guid, SelectedMap.Id));
                Btn_cancel = v.GetObject<Button>("btn_cancel");
                Btn_cancel.OnClickAdd(() =>
                {
                    Display(false);
                });
            }
            private Dizi SelectedDizi { get; set; }
            private IAutoAdvMap SelectedMap { get; set; }
            public void ListMap(IAutoAdvMap[] maps)
            {
                for(var i = 0; i < maps.Length; i++)
                {
                    var map = maps[i];
                    var index = i;
                    var ui = MapView.Instance(v => new Prefab_map(v, map));
                    ui.Set(map.Name, map.About, map.ActionLingCost);
                    ui.SetMapImage(map.Image);
                }
            }

            private class Prefab_map : UiBase
            {
                private Image Img_mapIco { get; }
                private Text Text_mapTitle { get; }
                private Text Text_about { get; }
                private Image Img_costIco { get; }
                private Text Text_cost { get; }
                public IAutoAdvMap Map { get; }
                public Prefab_map(IView v, IAutoAdvMap map) : base(v, true)
                {
                    Map = map;
                    Img_mapIco = v.GetObject<Image>("img_mapIco");
                    Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                    Text_about = v.GetObject<Text>("text_about");
                    Img_costIco = v.GetObject<Image>("img_costIco");
                    Text_cost = v.GetObject<Text>("text_cost");
                }
                public void SetMapImage(Sprite mapImg) => Img_mapIco.sprite = mapImg;
                public void SetCostIco(Sprite costImg) => Img_costIco.sprite = costImg;
                public void Set(string title, string about, int advCost)
                {
                    Text_mapTitle.text = title;
                    Text_about.text = about;
                    Text_cost.text = advCost.ToString();
                }
            }
        }
    }
}
