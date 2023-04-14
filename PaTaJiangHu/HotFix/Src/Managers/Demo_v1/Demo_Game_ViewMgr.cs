using System;
using System.Collections;
using System.Collections.Generic;
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
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Game;
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
            var map = state.DiziState.CurrentMapName;
            var stateText = state.DiziState.StateLabel;
            var mile = state.CurrentMile == 0 ? string.Empty
                : state.CurrentMile == -1 ? "未知"
                : state.CurrentMile.ToString();
            var occasion = dizi.State.DiziState.CurrentOccasion;
            GameView.Set(map, occasion, mile, stateText, GetTimeText(state.DiziState.CurrentProgressTime));
            GameView.UpdateBag(dizi);
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
            private View_bagCount BagCount { get; }
            private Text Text_status { get; }
            private Text Text_time { get; }

            public Game_View(IView v) : base(v, true)
            {
                Text_mapTitle = v.GetObject<Text>("text_mapTitle");
                Text_placeTitle = v.GetObject<Text>("text_placeTitle");
                Text_mile = v.GetObject<Text>("text_mile");
                DiziBagView = new View_diziBag(v.GetObject<View>("view_diziBag"));
                BagCount = new View_bagCount(v.GetObject<View>("view_bagCount"));
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

            public void UpdateBag(Dizi dizi)
            {
                var indexes = GetIndexes(dizi.Capable.Bag);
                DiziBagView.Set(View_diziBag.BagStates.Empty, View_diziBag.BagStates.None, indexes);
                var contents = new List<int>();
                foreach (var reward in dizi.State.StateBags)
                {
                    var i = 0;
                    for (; i < reward.AllItems.Length; i++) contents.Add(i);
                    for (; i < reward.Packages.Length; i++) contents.Add(i);
                }
                foreach (var itemIndex in contents) DiziBagView.Set(itemIndex, View_diziBag.BagStates.Content);
                BagCount.Set(contents.Count, dizi.Capable.Bag);
            }

            private static int[] GetIndexes(int length)
            {
                var indexes = new int[length];
                for (var i = 0; i < indexes.Length; i++) indexes[i] = i;
                return indexes;
            }

            private class View_diziBag : UiBase
            {
                public enum BagStates
                    {
                        None,
                        Empty,
                        Content,
                        Disable
                    }
                #region 60_Bags
                private BagElement Bag_0 { get; }
                private BagElement Bag_1 { get; }
                private BagElement Bag_2 { get; }
                private BagElement Bag_3 { get; }
                private BagElement Bag_4 { get; }
                private BagElement Bag_5 { get; }
                private BagElement Bag_6 { get; }
                private BagElement Bag_7 { get; }
                private BagElement Bag_8 { get; }
                private BagElement Bag_9 { get; }
                private BagElement Bag_10 { get; }
                private BagElement Bag_11 { get; }
                private BagElement Bag_12 { get; }
                private BagElement Bag_13 { get; }
                private BagElement Bag_14 { get; }
                private BagElement Bag_15 { get; }
                private BagElement Bag_16 { get; }
                private BagElement Bag_17 { get; }
                private BagElement Bag_18 { get; }
                private BagElement Bag_19 { get; }
                private BagElement Bag_20 { get; }
                private BagElement Bag_21 { get; }
                private BagElement Bag_22 { get; }
                private BagElement Bag_23 { get; }
                private BagElement Bag_24 { get; }
                private BagElement Bag_25 { get; }
                private BagElement Bag_26 { get; }
                private BagElement Bag_27 { get; }
                private BagElement Bag_28 { get; }
                private BagElement Bag_29 { get; }                
                private BagElement Bag_30 { get; }
                private BagElement Bag_31 { get; }
                private BagElement Bag_32 { get; }
                private BagElement Bag_33 { get; }
                private BagElement Bag_34 { get; }
                private BagElement Bag_35 { get; }
                private BagElement Bag_36 { get; }
                private BagElement Bag_37 { get; }
                private BagElement Bag_38 { get; }
                private BagElement Bag_39 { get; }                
                private BagElement Bag_40 { get; }
                private BagElement Bag_41 { get; }
                private BagElement Bag_42 { get; }
                private BagElement Bag_43 { get; }
                private BagElement Bag_44 { get; }
                private BagElement Bag_45 { get; }
                private BagElement Bag_46 { get; }
                private BagElement Bag_47 { get; }
                private BagElement Bag_48 { get; }
                private BagElement Bag_49 { get; }
                private BagElement Bag_50 { get; }
                private BagElement Bag_51 { get; }
                private BagElement Bag_52 { get; }
                private BagElement Bag_53 { get; }
                private BagElement Bag_54 { get; }
                private BagElement Bag_55 { get; }
                private BagElement Bag_56 { get; }
                private BagElement Bag_57 { get; }
                private BagElement Bag_58 { get; }
                private BagElement Bag_59 { get; }
                #endregion
                private BagElement[] Bags { get; set; }

                public View_diziBag(IView v) : base(v, true)
                {
                    #region 60_Bags
                    Bag_0 = new BagElement(v.GetObject<View>("element_0"));
                    Bag_1 = new BagElement(v.GetObject<View>("element_1"));
                    Bag_2 = new BagElement(v.GetObject<View>("element_2"));
                    Bag_3 = new BagElement(v.GetObject<View>("element_3"));
                    Bag_4 = new BagElement(v.GetObject<View>("element_4"));
                    Bag_5 = new BagElement(v.GetObject<View>("element_5"));
                    Bag_6 = new BagElement(v.GetObject<View>("element_6"));
                    Bag_7 = new BagElement(v.GetObject<View>("element_7"));
                    Bag_8 = new BagElement(v.GetObject<View>("element_8"));
                    Bag_9 = new BagElement(v.GetObject<View>("element_9"));
                    Bag_10 = new BagElement(v.GetObject<View>("element_10"));
                    Bag_11 = new BagElement(v.GetObject<View>("element_11"));
                    Bag_12 = new BagElement(v.GetObject<View>("element_12"));
                    Bag_13 = new BagElement(v.GetObject<View>("element_13"));
                    Bag_14 = new BagElement(v.GetObject<View>("element_14"));
                    Bag_15 = new BagElement(v.GetObject<View>("element_15"));
                    Bag_16 = new BagElement(v.GetObject<View>("element_16"));
                    Bag_17 = new BagElement(v.GetObject<View>("element_17"));
                    Bag_18 = new BagElement(v.GetObject<View>("element_18"));
                    Bag_19 = new BagElement(v.GetObject<View>("element_19"));
                    Bag_20 = new BagElement(v.GetObject<View>("element_20"));
                    Bag_21 = new BagElement(v.GetObject<View>("element_21"));
                    Bag_22 = new BagElement(v.GetObject<View>("element_22"));
                    Bag_23 = new BagElement(v.GetObject<View>("element_23"));
                    Bag_24 = new BagElement(v.GetObject<View>("element_24"));
                    Bag_25 = new BagElement(v.GetObject<View>("element_25"));
                    Bag_26 = new BagElement(v.GetObject<View>("element_26"));
                    Bag_27 = new BagElement(v.GetObject<View>("element_27"));
                    Bag_28 = new BagElement(v.GetObject<View>("element_28"));
                    Bag_29 = new BagElement(v.GetObject<View>("element_29"));
                    Bag_30 = new BagElement(v.GetObject<View>("element_30"));
                    Bag_31 = new BagElement(v.GetObject<View>("element_31"));
                    Bag_32 = new BagElement(v.GetObject<View>("element_32"));
                    Bag_33 = new BagElement(v.GetObject<View>("element_33"));
                    Bag_34 = new BagElement(v.GetObject<View>("element_34"));
                    Bag_35 = new BagElement(v.GetObject<View>("element_35"));
                    Bag_36 = new BagElement(v.GetObject<View>("element_36"));
                    Bag_37 = new BagElement(v.GetObject<View>("element_37"));
                    Bag_38 = new BagElement(v.GetObject<View>("element_38"));
                    Bag_39 = new BagElement(v.GetObject<View>("element_39"));
                    Bag_40 = new BagElement(v.GetObject<View>("element_40"));
                    Bag_41 = new BagElement(v.GetObject<View>("element_41"));
                    Bag_42 = new BagElement(v.GetObject<View>("element_42"));
                    Bag_43 = new BagElement(v.GetObject<View>("element_43"));
                    Bag_44 = new BagElement(v.GetObject<View>("element_44"));
                    Bag_45 = new BagElement(v.GetObject<View>("element_45"));
                    Bag_46 = new BagElement(v.GetObject<View>("element_46"));
                    Bag_47 = new BagElement(v.GetObject<View>("element_47"));
                    Bag_48 = new BagElement(v.GetObject<View>("element_48"));
                    Bag_49 = new BagElement(v.GetObject<View>("element_49"));
                    Bag_50 = new BagElement(v.GetObject<View>("element_50"));
                    Bag_51 = new BagElement(v.GetObject<View>("element_51"));
                    Bag_52 = new BagElement(v.GetObject<View>("element_52"));
                    Bag_53 = new BagElement(v.GetObject<View>("element_53"));
                    Bag_54 = new BagElement(v.GetObject<View>("element_54"));
                    Bag_55 = new BagElement(v.GetObject<View>("element_55"));
                    Bag_56 = new BagElement(v.GetObject<View>("element_56"));
                    Bag_57 = new BagElement(v.GetObject<View>("element_57"));
                    Bag_58 = new BagElement(v.GetObject<View>("element_58"));
                    Bag_59 = new BagElement(v.GetObject<View>("element_59"));
                    #endregion
                    Bags = new BagElement[]
                    {
                        Bag_0, Bag_1, Bag_2, Bag_3, Bag_4, Bag_5, Bag_6, Bag_7, Bag_8, Bag_9, Bag_10,
                        Bag_11, Bag_12, Bag_13, Bag_14, Bag_15, Bag_16, Bag_17, Bag_18, Bag_19, Bag_20,
                        Bag_21, Bag_22, Bag_23, Bag_24, Bag_25, Bag_26, Bag_27, Bag_28, Bag_29, Bag_30,
                        Bag_31, Bag_32, Bag_33, Bag_34, Bag_35, Bag_36, Bag_37, Bag_38, Bag_39, Bag_40,
                        Bag_41, Bag_42, Bag_43, Bag_44, Bag_45, Bag_46, Bag_47, Bag_48, Bag_49, Bag_50,
                        Bag_51, Bag_52, Bag_53, Bag_54, Bag_55, Bag_56, Bag_57, Bag_58, Bag_59,
                    };
                }

                public void SetAll(BagStates state)
                {
                    foreach (var bag in Bags) bag.Set(state);
                }

                public void Set(int index, BagStates state) => Bags[index].Set(state);
                public void Set(BagStates inIndexState, BagStates elseState, params int[] indexes)
                {
                    SetAll(elseState);
                    Set(inIndexState, indexes);
                }
                public void Set(BagStates state, params int[] indexes)
                {
                    for (int i = 0; i < indexes.Length; i++)
                    {
                        var index = indexes[i];
                        Bags[index].Set(state);
                    }
                }
                private class BagElement : UiBase
                {
                    private Image Img_empty { get; }
                    private Image Img_content { get; }
                    private Image Img_disable { get; }
                    public BagElement(IView v) : base(v, true)
                    {
                        Img_empty = v.GetObject<Image>("img_empty");
                        Img_content = v.GetObject<Image>("img_content");
                        Img_disable = v.GetObject<Image>("img_disable");
                    }

                    public void Set(BagStates state)
                    {
                        Img_empty.gameObject.SetActive(state == BagStates.Empty);
                        Img_content.gameObject.SetActive(state == BagStates.Content);
                        Img_disable.gameObject.SetActive(state == BagStates.Disable);
                    }
                }
            }
            private class View_bagCount : UiBase
            {
                private Text Text_bagValue { get; }
                private Text Text_bagMax { get; }

                public View_bagCount(IView v) : base(v, true)
                {
                    Text_bagValue= v.GetObject<Text>("text_bagValue");
                    Text_bagMax = v.GetObject<Text>("text_bagMax");
                }

                public void Set(int value, int max)
                {
                    if (value <= 0 && max <= 0)
                    {
                        Display(false);
                        return;
                    }
                    Text_bagValue.text = value.ToString();
                    Text_bagMax.text = max.ToString();
                }
            }
        }
    }
}
