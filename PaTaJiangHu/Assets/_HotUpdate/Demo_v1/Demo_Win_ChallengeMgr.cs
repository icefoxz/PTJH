using System;
using AOT._AOT.Core.Systems.Messaging;
using AOT._AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Win_ChallengeMgr : WinUiManagerBase
    {
        private Win_challenge win_challenge { get; set; }
        private Demo_v1Agent UiAgent { get; }
        private ChallengeStageController ChallengeStageController => Game.Controllers.Get<ChallengeStageController>();

        public Demo_Win_ChallengeMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            UiAgent = uiAgent;
        }

        protected override string ViewName => "demo_win_challenge";
        protected override bool IsDynamicPixel => true;

        protected override void Build(IView view)
        {
            win_challenge = new Win_challenge(v: view, onCloseAction: () => Hide(),
                onChallengeRedirection: () =>
                {
                    UiAgent.MainPage_Show(null);
                    Hide();
                }, onChallengeAbandon: () =>
                {
                    UiAgent.Win_ChallengeAbandon();
                    Hide();
                });
        }

        protected override void RegEvents()
        {
        }

        public void ShowChallengeWindow()
        {
            Show();
            var faction = Game.World.Faction;
            if (!faction.IsChallenging)
            {
                var newChallenge = ChallengeStageController.RequestNewChallenge();
                win_challenge.UnChallengeWindow(newChallenge.Name,
                    newChallenge.Image, newChallenge.StageCount,
                    newChallenge.About, () =>
                    {
                        UiAgent.Redirect_MainPage_ChallengeSelector();
                        Hide();
                    });
                return;
            }

            var stage = faction.GetChallengeStage();

            win_challenge.ChallengingWindow(stage.Name, stage.Image, faction.ChallengeStageProgress,
                stage.StageCount, stage.About);
        }

        private class Win_challenge : UiBase
        {
            private Button btn_x { get; }
            private View_info view_info { get; }
            private View_unchallenge view_unchallenge { get; }
            private View_challenging view_challenging { get; }

            public Win_challenge(IView v, Action onCloseAction, Action onChallengeRedirection,
                Action onChallengeAbandon) : base(v, true)
            {
                btn_x = v.GetObject<Button>("btn_x");
                view_info = new View_info(v.GetObject<View>("view_info"));
                view_unchallenge = new View_unchallenge(v.GetObject<View>("view_unchallenge"), false);
                view_challenging = new View_challenging(v.GetObject<View>("view_challenging"), onChallengeRedirection,
                    onChallengeAbandon);
                btn_x.OnClickAdd(onCloseAction);
            }

            public void ChallengingWindow(string name, Sprite icon, int progress, int max, string message)
            {
                view_info.Set(name, icon, progress, max, message);
                view_unchallenge.Display(false);
                view_challenging.Display(true);
            }

            public void UnChallengeWindow(string name, Sprite icon, int stages, string message,
                Action onChallengeClick)
            {
                view_info.Set(name, icon, 0, stages, message);
                view_unchallenge.SetClickAction(onChallengeClick);
                view_unchallenge.Display(true);
                view_challenging.Display(false);
            }

            private class View_info : UiBase
            {
                private Image img_icon { get; }
                private Text text_name { get; }
                private Text text_stageValue { get; }
                private Text text_stageMax { get; }
                private Text text_message { get; }

                public View_info(IView v) : base(v, true)
                {
                    img_icon = v.GetObject<Image>("img_icon");
                    text_name = v.GetObject<Text>("text_name");
                    text_stageValue = v.GetObject<Text>("text_stageValue");
                    text_stageMax = v.GetObject<Text>("text_stageMax");
                    text_message = v.GetObject<Text>("text_message");
                }

                public void Set(string name, Sprite icon, int progress, int max, string message)
                {
                    img_icon.sprite = icon;
                    text_name.text = name;
                    text_stageValue.text = progress.ToString();
                    text_stageMax.text = max.ToString();
                    text_message.text = message;
                }
            }

            private class View_unchallenge : UiBase
            {
                private Button btn_challenge { get; }
                private Image img_cost { get; }
                private Text text_cost { get; }

                public View_unchallenge(IView v, bool display) : base(v, display)
                {
                    btn_challenge = v.GetObject<Button>("btn_challenge");
                    img_cost = v.GetObject<Image>("img_cost");
                    text_cost = v.GetObject<Text>("text_cost");
                }

                public void Set(int value, Sprite icon)
                {
                    img_cost.sprite = icon;
                    text_cost.text = value.ToString();
                }

                public void SetClickAction(Action onclickAction)
                {
                    btn_challenge.OnClickAdd(onclickAction);
                }
            }

            private class View_challenging : UiBase
            {
                private Button btn_ok { get; }
                private Button btn_giveup { get; }

                public View_challenging(IView v, Action onChallengeRedirection, Action onChallengeGiveup) : base(v,
                    false)
                {
                    btn_ok = v.GetObject<Button>("btn_ok");
                    btn_giveup = v.GetObject<Button>("btn_giveup");
                    btn_ok.OnClickAdd(onChallengeRedirection);
                    btn_giveup.OnClickAdd(onChallengeGiveup);
                }
            }
        }
    }
}