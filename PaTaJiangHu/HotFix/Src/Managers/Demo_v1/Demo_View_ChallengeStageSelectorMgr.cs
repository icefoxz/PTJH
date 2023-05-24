using System;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
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
        private ChallengeStageController ChallengeController => Game.Controllers.Get<ChallengeStageController>();

        public Demo_View_ChallengeStageSelectorMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            UiAgent = uiAgent;
        }

        protected override void Build(IView view)
        {
            ChallengeStageSelector = new View_ChallengeStageSelector(view,
                onChallengeAction: challengeIndex => UiAgent.Dizi_ChallengeStart(challengeIndex),
                onBattleFinalized: ()=> UiAgent.BattleFinalize());
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update, b => UpdateChallengeSelector());
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_BattleEnd,
                b =>
                {
                    ChallengeStageSelector.SetFinalize(b.GetBool(0));
                    UpdateChallengeSelector();
                });
        }

        public override void Show() => ChallengeStageSelector.Display(true);
        public override void Hide() => ChallengeStageSelector.Display(false);

        public void UpdateChallengeSelector()
        {
            var faction = Game.World.Faction;
            if (!faction.IsChallenging)
            {
                Hide();
                return;
            }
            ChallengeStageSelector.Set(faction.GetChallengeStage());
        }

        private class View_ChallengeStageSelector : UiBase
        {
            private ScrollRect scroll_challenge { get; }
            private ListViewUi<ChallengePrefab> ChallengeList { get; }
            private Button btn_challenge { get; }
            private Button btn_cancel { get; }
            private View_BattleFinalize view_battleFinalize { get; }
            public View_ChallengeStageSelector(IView v, Action<int> onChallengeAction,Action onBattleFinalized) : base(v, false)
            {
                scroll_challenge = v.GetObject<ScrollRect>("scroll_challenge");
                ChallengeList = new ListViewUi<ChallengePrefab>(v.GetObject<View>("prefab_challenge"), scroll_challenge);
                btn_challenge = v.GetObject<Button>("btn_challenge");
                btn_challenge.OnClickAdd(()=>onChallengeAction?.Invoke(SelectedNpcIndex));
                btn_cancel = v.GetObject<Button>("btn_cancel");
                btn_cancel.OnClickAdd(() => Display(false));
                view_battleFinalize = new View_BattleFinalize(v.GetObject<View>("view_battleFinalize"), onBattleFinalized);
            }

            private int SelectedNpcIndex { get; set; }
            public void Set(IChallengeStage stage)
            {
                ChallengeList.ClearList(c=>c.Destroy());
                if (stage == null) return;
                var faction = Game.World.Faction;
                var npcs = stage.GetChallengeNpcs(faction.ChallengeStageProgress);
                for (var i = 0; i < npcs.Length; i++)
                {
                    var index = i;
                    var n = npcs[i];
                    var ui = ChallengeList.Instance(v => new ChallengePrefab(v, () => SetSelected(index)));
                    ui.SetNpc(n.NpcName, stage.Name, n.Level, n.Icon, n.IsBoss);
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

            public void SetFinalize(bool isWin) => view_battleFinalize.Set(isWin);

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

                public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);

                public void SetNpc(string name, string faction, int level, Sprite icon, bool isBoss)
                {
                    Img_npcIco.sprite = icon;
                    Text_name.text = name;
                    Text_faction.text = faction;
                    Text_npcLevel.text = level.ToString();
                    Img_IsBoss.gameObject.SetActive(isBoss);
                    Img_select.gameObject.SetActive(false);
                }
            }
            private class View_BattleFinalize : UiBase
            {
                private Button btn_battleFinalize { get; }
                private Text text_win { get; }
                private Text text_lose { get; }
                public View_BattleFinalize(IView v,Action onclickAction) : base(v, false)
                {
                    btn_battleFinalize = v.GetObject<Button>("btn_battleFinalize");
                    text_win = v.GetObject<Text>("text_win");
                    text_lose = v.GetObject<Text>("text_lose");
                    btn_battleFinalize.OnClickAdd(() =>
                    {
                        onclickAction?.Invoke();
                        Display(false);
                    });
                }
                public void Set(bool win)
                {
                    text_win.gameObject.SetActive(win);
                    text_lose.gameObject.SetActive(!win);
                    Display(true);
                }
            }
        }
    }
}
