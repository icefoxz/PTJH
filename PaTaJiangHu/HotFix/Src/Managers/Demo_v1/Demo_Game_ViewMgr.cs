using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_Game_ViewMgr : MainPageBase
    {
        private Game_View GameView { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Mid;
        protected override string ViewName => "demo_game_view";
        protected override bool IsDynamicPixel => true;
        public Demo_Game_ViewMgr(MainUiAgent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            GameView = new Game_View(view);
        }
        protected override void RegEvents()
        {
        }
        public override void Show() => GameView.Display(true);
        public override void Hide() => GameView.Display(false);

        private class Game_View : UiBase
        {
            private Text Text_mapTitle { get; }
            private Text Text_placeTitle { get; }
            private Text Text_mile { get; }
            private View_diziBag DiziBagView { get; }
            private Text Text_status { get; }
            private Text Text_time { get; }
            public Game_View(IView v) : base(v, false)
            {
                Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                Text_placeTitle = v.GetObject<Text>("text_placeTitle");
                Text_mile = v.GetObject<Text>("text_mile");
                DiziBagView = new View_diziBag(v.GetObject<View>("view_diziBag"));
                Text_status = v.GetObject<Text>("text_status");
                Text_time = v.GetObject<Text>("text_time");
            }
            public void Set(string map, string place, string mile, string status, string time)
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
