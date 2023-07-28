using System;
using System.Runtime.CompilerServices;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Utls;
using AOT.Views.Abstract;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{

//门派
    internal class Demo_Page_Faction : PageUiManagerBase
    {
        private Faction_page FactionPage { get; set; }
        private Demo_v1Agent Agent { get; }

        public Demo_Page_Faction(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
        }

        protected override string ViewName => "demo_page_faction";

        protected override void Build(IView view)
        {
            FactionPage = new Faction_page(view ,Agent, true);
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update,
                _ => { FactionPage.UpdateChallengeUi(); }); //更新挑战UI
            Game.MessagingManager.RegEvent(EventString.Faction_Init, _ => FactionPage.UpdateChallengeUi()); // 门派初始化
            Game.MessagingManager.RegEvent(EventString.Recruit_VisitorDizi, _ => FactionPage.ShowVisitor()); // 显示拜访者
            Game.MessagingManager.RegEvent(EventString.Recruit_VisitorRemove, _ => FactionPage.CloseVisitorUi()); // 关闭拜访者UI
        }

        private class Faction_page : UiBase
        {
            private View_challenge view_challenge { get; }
            private Button btn_diziHouse { get; }
            private Button btn_treasureHouse { get; }
            private View_visitor view_visitor { get; }
            
            private Demo_v1Agent Agent { get; }

            public Faction_page(IView v,Demo_v1Agent agent, bool display) : base(v, display)
            {
                Agent = agent;
                view_challenge = new View_challenge(v.GetObject<View>("view_challenge"));
                view_visitor = new View_visitor(v.GetObject<View>("view_visitor"), Agent.ShowVisitor, false);
                btn_diziHouse = v.GetObject<Button>("btn_diziHouse");
                btn_treasureHouse = v.GetObject<Button>("btn_treasureHouse");
                btn_diziHouse.OnClickAdd(agent.ShowDiziHouse);
                btn_treasureHouse.OnClickAdd(agent.ShowTreasureHouse);
            }

            /** 根据不同的状态,点击效果会不一样.
         *  1. 未解锁挑战,点击无效果, 并且不显示UI
         *  2. 已解锁挑战,点击弹出挑战窗口并且调用控制器请求生成一个新挑战,
         * 注意: 生成后无论点击挑战与否都不影响挑战已经启动了.
         *  3. 已经有挑战,点击弹出挑战窗口,可选择转换挑战页或请求控制器放弃挑战
         *  4. 有宝箱状态, 点击并领取宝箱.
         */
            public void UpdateChallengeUi([CallerMemberName] string methodName = null)
            {
                XDebug.Log($"{methodName}.{nameof(UpdateChallengeUi)}");
                var faction = Game.World.Faction;
                Action action = NewChallenge;
                if (!faction.IsChallenging)
                {
                    //挑战中
                    action = DoChallenge;

                }

                if (faction.ChallengeChests.Count > 0)
                {
                    //领取宝箱
                    action = OpenChest;
                }

                view_challenge.UpdateStage(action);
            }
            private void NewChallenge() => Agent.Win_ChallengeWindow();
            private void DoChallenge() => Agent.Win_ChallengeWindow();
            private void OpenChest()
            {
                Agent.Win_ChallengeReward();
                XDebug.Log("领取宝箱");
            }

            private class View_challenge : UiBase
            {
                private Image img_icon { get; }
                private Text text_stageName { get; }
                private GameObject obj_chest_0 { get; }
                private GameObject obj_chest_1 { get; }
                private GameObject obj_chest_2 { get; }
                private GameObject obj_chest_3 { get; }
                private GameObject obj_challenge { get; }
                private Text text_stageValue { get; }
                private Text text_stageMax { get; }
                private Text text_level { get; }
                private Button btn_action { get; }

                private GameObject[] Chests { get; }

                public View_challenge(IView v) : base(v, true)
                {
                    img_icon = v.GetObject<Image>("img_icon");
                    text_stageName = v.GetObject<Text>("text_stageName");
                    obj_chest_0 = v.GetObject("obj_chest_0");
                    obj_chest_1 = v.GetObject("obj_chest_1");
                    obj_chest_2 = v.GetObject("obj_chest_2");
                    obj_chest_3 = v.GetObject("obj_chest_3");
                    obj_challenge = v.GetObject("obj_challenge");
                    text_stageValue = v.GetObject<Text>("text_stageValue");
                    text_stageMax = v.GetObject<Text>("text_stageMax");
                    text_level = v.GetObject<Text>("text_level");
                    btn_action = v.GetObject<Button>("btn_action");

                    Chests = new[] { obj_chest_0, obj_chest_1, obj_chest_2, obj_chest_3 };
                }

                public void UpdateStage(Action onClickAction)
                {
                    var faction = Game.World.Faction;
                    btn_action.OnClickAdd(() => onClickAction?.Invoke());
                    if (faction.IsChallenging || faction.ChallengeChests.Count > 0)
                    {
                        var stage = faction.GetChallengeStage();
                        SetChest(faction.ChallengeChests.Count);
                        SetStage(stage.Name, faction.ChallengeStageProgress, stage.StageCount, faction.ChallengeLevel,
                            stage.Image);
                        obj_challenge.gameObject.SetActive(true);
                        return;
                    }

                    SetChest(0);
                    SetStage("未解锁", 0, 0, faction.ChallengeLevel, null);
                    obj_challenge.gameObject.SetActive(false);
                }

                private void SetChest(int chests)
                {
                    for (var i = 0; i < Chests.Length; i++) Chests[i].SetActive(i < chests);
                }

                private void SetStage(string stageName, int progress, int stageMax, int level, Sprite icon)
                {
                    text_stageName.text = stageName;
                    text_stageValue.text = progress.ToString();
                    text_stageMax.text = stageMax.ToString();
                    text_level.text = level.ToString();
                    img_icon.sprite = icon;
                }
            }

            public void ShowVisitor() => view_visitor.Display(true);
            public void CloseVisitorUi() => view_visitor.Display(false);

            // 弟子到访, 点击弹出弟子到访窗口, 暂时仅作为一个按键操作
            private class View_visitor : UiBase
            {
                private Button btn_visitor { get; }
                public View_visitor(IView v, Action onVisitorAction, bool display) : base(v, display)
                {
                    btn_visitor = v.GetObject<Button>("btn_visitor");
                    btn_visitor.OnClickAdd(onVisitorAction);
                }
            }
        }
    }
}