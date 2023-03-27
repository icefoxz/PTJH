using System;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.ChallengeStages;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_ChallengeStageSelectorMgr : MainPageBase
    {
        private View_ChallengeStageSelector ChallengeStageSelector { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_challengeStageSelector";
        protected override bool IsDynamicPixel => true;
        private Demo_v1Agent UiAgent { get; }

        public Demo_View_ChallengeStageSelectorMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            UiAgent = uiAgent;
        }

        protected override void Build(IView view)
        {
            ChallengeStageSelector = new View_ChallengeStageSelector(view,
                onChallengeAction: challengeIndex => UiAgent.StartChallenge(challengeIndex));
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update, b => UiAgent.ShowChallengeState());
        }

        public override void Show() => ChallengeStageSelector.Display(true);
        public override void Hide() => ChallengeStageSelector.Display(false);

        public void SetChallenges(IChallengeStageNpc[] challengeStages) => ChallengeStageSelector.Set(challengeStages);

        private class View_ChallengeStageSelector : UiBase
        {
            private ScrollRect Scroll_challenge { get; }
            private ListViewUi<ChallengePrefab> ChallengeList { get; }
            private Button Btn_challenge { get; }
            private Button Btn_cancel { get; }
            public View_ChallengeStageSelector(IView v, Action<int> onChallengeAction) : base(v, false)
            {
                Scroll_challenge = v.GetObject<ScrollRect>("scroll_challenge");
                ChallengeList = new ListViewUi<ChallengePrefab>(v.GetObject<View>("prefab_challenge"), Scroll_challenge);
                Btn_challenge = v.GetObject<Button>("btn_challenge");
                Btn_challenge.OnClickAdd(()=>onChallengeAction?.Invoke(SelectedNpcIndex));
                Btn_cancel = v.GetObject<Button>("btn_cancel");
                Btn_cancel.OnClickAdd(() => Display(false));
            }

            private int SelectedNpcIndex { get; set; }
            public void Set(IChallengeStageNpc[] npcs)
            {
                ChallengeList.ClearList(c=>c.Destroy());
                for (var i = 0; i < npcs.Length; i++)
                {
                    var index = i;
                    var n = npcs[i];
                    var ui = ChallengeList.Instance(v => new ChallengePrefab(v, () => SetSelected(index)));
                    ui.SetNpc(n.NpcName, n.Faction, n.Level, n.IsBoss);
                    if (i == 0)
                    {
                        //预选第一个
                        SelectedNpcIndex = index;
                        ui.SetSelected(true);
                    }
                }
            }

            private void SetSelected(int index)
            {
                SelectedNpcIndex = index;
                for (var i = 0; i < ChallengeList.List.Count; i++)
                {
                    var ui = ChallengeList.List[i];
                    ui.SetSelected(index == i);
                }
            }

            private class ChallengePrefab : UiBase
            {
                private Image Img_select { get; }
                private Image Img_npcIco { get; }
                private Image Img_IsBoss { get; }
                private Text Text_npcLevel { get; }
                private Button Btn_npc { get; }
                private Text Text_name { get; }
                private Text Text_faction { get; }
                public ChallengePrefab(IView v,Action onSelectAction) : base(v, true)
                {
                    Img_select = v.GetObject<Image>("img_select");
                    Img_npcIco = v.GetObject<Image>("img_npcIco");
                    Img_IsBoss = v.GetObject<Image>("img_isBoss");
                    Text_npcLevel = v.GetObject<Text>("text_npcLevel");
                    Text_name = v.GetObject<Text>("text_name");
                    Text_faction = v.GetObject<Text>("text_faction");
                    Btn_npc = v.GetObject<Button>("btn_npc");
                    Btn_npc.OnClickAdd(onSelectAction);
                }

                public void SetIcon(Sprite ico) => Img_npcIco.sprite = ico;
                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
                public void SetNpc(string name, string faction, int level,bool isBoss)
                {
                    Text_name.text = name;
                    Text_faction.text = faction;
                    Text_npcLevel.text = level.ToString();
                    Img_IsBoss.gameObject.SetActive(isBoss);
                    Img_select.gameObject.SetActive(false);
                }
            }
        }
    }
}
