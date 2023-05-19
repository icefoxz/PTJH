using System;
using _GameClient.Models;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

//门派
internal class Demo_Page_Faction : UiManagerBase
{
    private Faction_page FactionPage { get; set; }

    public Demo_Page_Faction(Demo_v1Agent uiAgent) : base(uiAgent)
    {
    }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Page;
    protected override string ViewName => "demo_page_faction";
    protected override bool IsDynamicPixel => true;

    protected override void Build(IView view)
    {
        FactionPage = new Faction_page(view, true);
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update, _ =>
        {
            FactionPage.UpdateChallengeUi();
        });
    }

    public override void Show() => FactionPage.Display(true);

    public override void Hide() => FactionPage.Display(false);

    private class Faction_page : UiBase
    {
        private View_challenge view_challenge { get; }
        public Faction_page(IView v, bool display) : base(v, display)
        {
            view_challenge = new View_challenge(v.GetObject<View>("view_challenge"));
        }

        /** 根据不同的状态,点击效果会不一样.
         *  1. 未解锁挑战,点击无效果, 并且不显示UI
         *  2. 已解锁挑战,点击弹出挑战窗口并且调用控制器请求生成一个新挑战,
         * 注意: 生成后无论点击挑战与否都不影响挑战已经启动了.
         *  3. 已经有挑战,点击弹出挑战窗口,可选择转换挑战页或请求控制器放弃挑战
         *  4. 有宝箱状态, 点击并领取宝箱.
         */
        public void UpdateChallengeUi()
        {
            var faction = Game.World.Faction;
            var challenge = faction.Challenge;
            if (challenge == null)
            {
                //未解锁挑战
                ChallengeNoneUi();
                return;
            }

            var isChallenging = challenge.Progress < challenge.CurrentStage.MaxCheckPoint;
            var hasChest = challenge.Chests.Count > 0;
            if (hasChest)
            {
                //领取宝箱
                view_challenge.UpdateStage(challenge, faction.ChallengeLevel, () => { XDebug.Log("领取宝箱"); });
                return;
            }

            if (isChallenging)
            {
                //挑战中
                view_challenge.UpdateStage(challenge, faction.ChallengeLevel, () => { XDebug.Log("跳转挑战页面!"); });
                return;
            }
            //代码会跑到这里估计是已完成并且领取了所有宝箱, 所以跟无挑战状态一样处理
            ChallengeNoneUi();

            void ChallengeNoneUi() => view_challenge.UpdateStage(null, -1, () =>
            {
                XDebug.Log("请求解锁挑战新挑战!");
            });
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

            public View_challenge(IView v) : base(v, false)
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

                Chests = new[] {obj_chest_0, obj_chest_1, obj_chest_2, obj_chest_3};
            }

            public void UpdateStage(Faction.ChallengeStage challenge, int level, Action onClickAction)
            {
                btn_action.OnClickAdd(() => onClickAction?.Invoke());
                if (challenge == null)
                {
                    SetChest(0);
                    SetStage("未解锁", 0, 0, level, null);
                    obj_challenge.gameObject.SetActive(false);
                    return;
                }

                var stage = challenge.CurrentStage;
                SetChest(challenge.Chests.Count);
                SetStage(stage.Name, challenge.Progress, challenge.CurrentStage.MaxCheckPoint, level, stage.Image);
                obj_challenge.gameObject.SetActive(true);
            }

            private  void SetChest(int chests)
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
    }
}