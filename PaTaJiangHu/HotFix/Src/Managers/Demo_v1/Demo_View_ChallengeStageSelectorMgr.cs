using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
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
    internal class Demo_View_ChallengeStageSelectorMgr : MainPageBase
    {
        private View_ChallengeStageSelector ChallengeStageSelector { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_challengeStageSelector";
        protected override bool IsDynamicPixel => true;
        public Demo_View_ChallengeStageSelectorMgr(MainUiAgent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            ChallengeStageSelector = new View_ChallengeStageSelector(view,
                onClickAction: () => XDebug.LogWarning("挑战开始！"));
        }
        protected override void RegEvents()
        {

        }

        public override void Show() => ChallengeStageSelector.Display(true);
        public override void Hide() => ChallengeStageSelector.Display(false);
        private class View_ChallengeStageSelector : UiBase
        {
            private ScrollRect Scroll_challenge { get; }
            private ListViewUi<ChallengePrefab> ChallengeList { get; }
            private Button Btn_challenge { get; }
            private Button Btn_cancel { get; }
            public View_ChallengeStageSelector(IView v, Action onClickAction) : base(v, false)
            {
                Scroll_challenge = v.GetObject<ScrollRect>("scroll_challenge");
                ChallengeList = new ListViewUi<ChallengePrefab>(v.GetObject<View>("prefab_challenge"), Scroll_challenge);
                Btn_challenge = v.GetObject<Button>("btn_challenge");
                Btn_challenge.OnClickAdd(onClickAction);
                Btn_cancel = v.GetObject<Button>("btn_cancel");
                Btn_cancel.OnClickAdd(() =>
                {
                    Display(false);
                });
            }

            private class ChallengePrefab : UiBase
            {
                private Image Img_select { get; }
                private Image Img_npcIco { get; }
                private Text Text_npcLevel { get; }
                private Text Text_name { get; }
                private Text Text_faction { get; }
                public ChallengePrefab(IView v) : base(v, true)
                {
                    Img_select = v.GetObject<Image>("img_select");
                    Img_npcIco = v.GetObject<Image>("img_npcIco");
                    Text_npcLevel = v.GetObject<Text>("text_npcLevel");
                    Text_name = v.GetObject<Text>("text_name");
                    Text_faction = v.GetObject<Text>("text_faction");
                }

                public void SetIcon(Sprite ico) => Img_npcIco.sprite = ico;
                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
                public void SetNpc(string name, string faction, int level)
                {
                    Text_name.text = name;
                    Text_faction.text = faction;
                    Text_npcLevel.text = level.ToString();
                }
            }
        }
    }
}
