using System;
using System.Collections;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Systems.Coroutines;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_Game_ViewMgr : MainPageBase
    {
        private Game_View GameView { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Mid;
        protected override string ViewName => "demo_game_view";
        protected override bool IsDynamicPixel => true;
        public Demo_Game_ViewMgr(MainUiAgent uiAgent) : base(uiAgent) { }
        private ICoroutineInstance UpdateCo { get; set; }
        private Dizi SelectedDizi { get; set; }

        protected override void Build(IView view)
        {
            GameView = new Game_View(view);
        }
        protected override void RegEvents()
        {
        }
        public override void Show() => GameView.Display(true);
        public override void Hide() => GameView.Display(false);

        public void Set(Dizi dizi)
        {
            UpdateCo ??= Game.CoService.RunCo(UpdateTime(), () => UpdateCo = null, nameof(Demo_Game_ViewMgr));
            SelectedDizi = dizi;
            UpdateState(dizi);
        }

        private void UpdateState(Dizi dizi)
        {
            if (SelectedDizi == null) return;
            var state = dizi.State;
            var map = state.CurrentMap;
            var stateText = state.StateText;
            var mile = state.CurrentMile == 0 ? string.Empty
                : state.CurrentMile == -1 ? "未知"
                : state.CurrentMile.ToString();
            var occasion = dizi.State.CurrentOccasion;
            GameView.Set(map, occasion, mile, stateText, GetTimeText(state.CurrentProgressTime));
        }


        private IEnumerator UpdateTime()
        {
            while (SelectedDizi != null)
            {
                yield return new WaitForSeconds(1);
                UpdateState(SelectedDizi);
            }
        }

        private string GetTimeText(TimeSpan ts)
        {
            var totalHrs = (int)ts.TotalHours;
            var hrText = totalHrs > 0 ? $"{totalHrs}小时" : string.Empty;
            var minText = ts.Minutes > 0 ? $"{ts.Minutes}分钟" : string.Empty;
            var secText = ts.Seconds > 0 ? $"{ts.Seconds}秒" : string.Empty;
            return hrText + minText + secText;
        }

        private class Game_View : UiBase
        {
            private Text Text_mapTitle { get; }
            private Text Text_placeTitle { get; }
            private Text Text_mile { get; }
            private View_diziBag DiziBagView { get; }
            private Text Text_status { get; }
            private Text Text_time { get; }
            public Game_View(IView v) : base(v, true)
            {
                Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                Text_placeTitle = v.GetObject<Text>("text_placeTitle");
                Text_mile = v.GetObject<Text>("text_mile");
                DiziBagView = new View_diziBag(v.GetObject<View>("view_diziBag"));
                Text_status = v.GetObject<Text>("text_status");
                Text_time = v.GetObject<Text>("text_time");
            }
            public void Set(string map, string place, string mile, string status,string time)
            {
                Text_mapTitle.text = map;
                Text_placeTitle.text = place;
                Text_mile.text = mile;
                Text_status.text = status;
                Text_time.text = time;
            }
            private class View_diziBag : UiBase
            {
                public View_diziBag(IView v) : base(v, true)
                {
                }
            }
        }
    }
}
