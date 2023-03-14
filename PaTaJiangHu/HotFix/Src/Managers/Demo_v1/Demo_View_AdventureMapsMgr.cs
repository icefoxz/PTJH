using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1
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
                onAdvStartAction: index => DiziAdvController.AdventureStart(SelectedDizi.Guid, index)
            );
        }
        protected override void RegEvents()
        {
            //Game.MessagingManager.RegEvent(EventString.Dizi_AdvManagement, bag =>
            //{
            //    AdventureMaps.Set();
            //});
            //Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag =>  //Temporary
            //{
            //    AdventureMaps.ListMap(DiziAdvController.AutoAdvMaps(0));
            //    Show();
            //});
        }
        public override void Show() => AdventureMaps.Display(true);
        public override void Hide() => AdventureMaps.Display(false);

        private Dizi SelectedDizi { get; set; }
        /// <summary>
        /// 0 = 历练, 1 = production
        /// </summary>
        public void Set(string guid,int mapType)
        {
            SelectedDizi = Game.World.Faction.GetDizi(guid);
            var maps = DiziAdvController.AutoAdvMaps(mapType);
            AdventureMaps.ListMap(maps);
        }

        private class View_AdventureMaps : UiBase
        {
            private ScrollRect Scroll_mapSelector { get; }
            private ListViewUi<Prefab_map> MapView { get; }
            private Button Btn_refersh { get; }
            private Button Btn_action { get; }
            private Button Btn_cancel { get; }

            public View_AdventureMaps(IView v,
                Action onRefreshAction,
                Action<int> onAdvStartAction
            ) : base(v, false)
            {
                Scroll_mapSelector = v.GetObject<ScrollRect>("scroll_mapSelector");
                MapView = new ListViewUi<Prefab_map>(v.GetObject<View>("prefab_map"), Scroll_mapSelector);
                Btn_refersh = v.GetObject<Button>("btn_refresh");
                Btn_refersh.OnClickAdd(onRefreshAction);
                Btn_action = v.GetObject<Button>("btn_action");
                Btn_action.OnClickAdd(() =>
                {
                    onAdvStartAction?.Invoke(SelectedIndex);
                    Display(false);
                });
                Btn_cancel = v.GetObject<Button>("btn_cancel");
                Btn_cancel.OnClickAdd(() => Display(false));
            }

            private int SelectedIndex { get; set; }
            public void ListMap(IAutoAdvMap[] maps)
            {
                for (var i = 0; i < maps.Length; i++)
                {
                    var map = maps[i];
                    var ui = MapView.Instance(v => new Prefab_map(v, map, SetSelected));
                    ui.Set(map.Name, map.About, map.ActionLingCost);
                    ui.SetMapImage(map.Image);
                }
            }

            private void SetSelected(int mapId)
            {
                for (var i = 0; i < MapView.List.Count; i++)
                {
                    var ui = MapView.List[i];
                    var selected = ui.Map.Id == mapId;
                    ui.SetSelected(selected);
                    if (selected) SelectedIndex = mapId;
                }
            }

            private class Prefab_map : UiBase
            {
                private Image Img_mapIco { get; }
                private Text Text_mapTitle { get; }
                private Text Text_about { get; }
                private Image Img_costIco { get; }
                private Text Text_cost { get; }
                private Image Img_selected { get; }
                private Button Btn_Map { get; }
                public IAutoAdvMap Map { get; }

                public Prefab_map(IView v, IAutoAdvMap map, Action<int> onClickAction) : base(v, true)
                {
                    Map = map;
                    Img_mapIco = v.GetObject<Image>("img_mapIco");
                    Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                    Text_about = v.GetObject<Text>("text_about");
                    Img_costIco = v.GetObject<Image>("img_costIco");
                    Text_cost = v.GetObject<Text>("text_cost");
                    Img_selected = v.GetObject<Image>("img_selected");
                    Btn_Map = v.GetObject<Button>("btn_map");
                    Btn_Map.OnClickAdd(() => onClickAction(map.Id));
                }

                public void SetSelected(bool isSelected) => Img_selected.gameObject.SetActive(isSelected);
                public void SetMapImage(Sprite mapImg) => Img_mapIco.sprite = mapImg;
                public void SetCost(Sprite costImg) => Img_costIco.sprite = costImg;

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
