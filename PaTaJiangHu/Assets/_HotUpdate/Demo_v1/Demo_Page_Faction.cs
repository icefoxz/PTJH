using System;
using System.Linq;
using System.Runtime.CompilerServices;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Utls;
using AOT.Views.Abstract;
using GameClient.Models;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{

//门派
    internal class Demo_Page_Faction : PageUiManagerBase
    {
        private Faction_page FactionPage { get; set; }
        private Demo_v1Agent Agent { get; set; }
        private static Faction Faction => Game.World.Faction;
        public Demo_Page_Faction(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
        }

        protected override string ViewName => "demo_page_faction";

        protected override void Build(IView view)
        {
            FactionPage = new Faction_page(view, Agent, true,
                onShenGongPage: guid => { Agent.SkillPage_Show(guid); },
                oncXiuXingPage: guid => { Agent.MainPage_Show(guid); }
                );
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Challenge_Update,
                _ => { FactionPage.UpdateChallengeUi(); }); //更新挑战UI
            Game.MessagingManager.RegEvent(EventString.Faction_Init, _ => FactionPage.UpdateChallengeUi()); // 门派初始化
            Game.MessagingManager.RegEvent(EventString.Recruit_VisitorDizi, _ => FactionPage.ShowVisitor()); // 显示拜访者
            Game.MessagingManager.RegEvent(EventString.Recruit_VisitorRemove, _ => FactionPage.CloseVisitorUi()); // 关闭拜访者UI
            Game.MessagingManager.RegEvent(EventString.Faction_DiziListUpdate, _ => FactionPage.UpdateMapPop());
            Game.MessagingManager.RegEvent(EventString.Dizi_State_Update, _ => FactionPage.UpdateMapPop());
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, _ => FactionPage.UpdateProduction());
        }

        private class Faction_page : UiBase
        {
            private View_challenge view_challenge { get; }
            private Button btn_diziHouse { get; }
            private Button btn_treasureHouse { get; }
            private View_visitor view_visitor { get; }
            private View_diziPop view_diziPop { get; }
            private Element_production element_productionFarming { get; }
            private View_mapPop view_mapPop { get; }
            
            private Demo_v1Agent Agent { get; }

            public Faction_page(IView v,Demo_v1Agent agent, bool display, Action<string> onShenGongPage, Action<string> oncXiuXingPage) : base(v, display)
            {
                Agent = agent;
                view_challenge = new View_challenge(v.GetObject<View>("view_challenge"));
                view_visitor = new View_visitor(v.GetObject<View>("view_visitor"), Agent.ShowVisitor, false);
                btn_diziHouse = v.GetObject<Button>("btn_diziHouse");
                btn_treasureHouse = v.GetObject<Button>("btn_treasureHouse");
                btn_diziHouse.OnClickAdd(agent.ShowDiziHouse);
                btn_treasureHouse.OnClickAdd(agent.ShowTreasureHouse);
                view_diziPop = new View_diziPop(v.GetObject<View>("view_diziPop"), onShenGongPage, oncXiuXingPage);
                element_productionFarming = new Element_production(v.GetObject<View>("element_productionFarming"), guid =>
                {
                    view_diziPop.Display(guid);
                });
                view_mapPop = new View_mapPop(v.GetObject<View>("view_mapPop"));
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

            internal void UpdateMapPop()
            {
                view_mapPop.UpdateDiziList();
            }

            internal void UpdateProduction()
            {
                element_productionFarming.UpdateDiziPrductionList();
            }

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

            private class View_diziPop : UiBase
            {
                private View_diziInfo view_DiziInfo { get;}
                private Button btn_info { get; }
                private Button btn_action { get; }
                private Button btn_x { get; }
                private string SelectedDiziGuid { get; set; }
                public View_diziPop(IView v, Action<string> onShenGongPage, Action<string> onXiuXingPage) : base(v, false)
                {
                    btn_info = v.GetObject<Button>("btn_info");
                    btn_info.OnClickAdd(() =>
                    {
                        onXiuXingPage?.Invoke(SelectedDiziGuid);
                        Display(false);
                    });
                    btn_action = v.GetObject<Button>("btn_action");
                    btn_action.OnClickAdd(() =>
                    {
                        onShenGongPage?.Invoke(SelectedDiziGuid);
                        Display(false);
                    });
                    btn_x = v.GetObject<Button>("btn_x");
                    btn_x.OnClickAdd(() => Display(false));
                    view_DiziInfo = new View_diziInfo(v.GetObject<View>("view_diziInfo"));
                }
                public void Display(string guid)
                {
                    Display(true);
                    var name = Faction.GetDizi(guid).Name;
                    SetName(name);
                    SelectedDiziGuid = guid;
                }
                public void SetAvatar(Sprite avatar) => view_DiziInfo.SetAvatar(avatar);
                public void SetName(string name) => view_DiziInfo.SetName(name);

                private class View_diziInfo : UiBase
                {
                    private Image img_avatar { get; }
                    private Text text_name { get; }
                    public View_diziInfo(IView v) : base(v, true)
                    {
                        img_avatar = v.GetObject<Image>("img_avatar");
                        text_name = v.GetObject<Text>("text_name");
                    }
                    public void SetAvatar(Sprite avatar) => img_avatar.sprite = avatar;
                    public void SetName(string name)
                    {
                        text_name.text = name;
                    }
                }
            }

            private class Element_production : UiBase
            {
                private ListViewUi<Prefab_dizi> DiziList { get; }
                private RectTransform grid { get; }
                private event Action<string> OnDiziSelected;
                public Element_production(IView v, Action<string> onDiziClicked) : base(v, true)
                {
                    OnDiziSelected = onDiziClicked;
                    grid = v.GetObject<RectTransform>("grid");
                    DiziList = new ListViewUi<Prefab_dizi>(v.GetObject<View>("prefab_dizi"), grid);
                }
                public void UpdateDiziPrductionList()
                {
                    var list = Game.World.Faction.DiziList.Where(d => d.Activity == DiziActivities.Adventure).ToArray();
                    ClearList();
                    for(var i = 0;  i < list.Length; i++)
                    {
                        var dizi = list[i];
                        var guid = dizi.Guid;
                        var index = i;
                        var ui = DiziList.Instance(v => new Prefab_dizi(v, () =>
                        {
                            OnDiziSelected?.Invoke(list[index].Guid);
                        }));
                    }
                }
                private void ClearList()
                {
                    DiziList.ClearList(ui => ui.Destroy());
                }
                private class Prefab_dizi : UiBase
                {
                    private Button btn_click { get; }
                    public Prefab_dizi(IView v, Action onclickAction) : base(v, true)
                    {
                        btn_click = v.GetObject<Button>("btn_click");
                        btn_click.OnClickAdd(() => onclickAction?.Invoke());
                    }
                }
            }

            private class View_mapPop : UiBase
            {
                private ScrollRect scr_pop { get; }
                private ListViewUi<Prefab_Pop> DiziList { get; }
                private Dizi[] CurrentList { get; set; }
                public View_mapPop(IView v) : base(v, true)
                {
                    scr_pop = v.GetObject<ScrollRect>("scr_pop");
                    DiziList = new ListViewUi<Prefab_Pop>(v.GetObject<View>("prefab_pop"), scr_pop);
                }
                public void UpdateDiziList()
                {
                    var list = Game.World.Faction.DiziList.Where(d => d.Activity == DiziActivities.Adventure).ToArray();
                    //var list = Faction.DiziList.ToArray();
                    CurrentList = list;
                    ClearList();
                    for(var i = 0; i < list.Length; i++)
                    {
                        var dizi = list[i];
                        var guid = dizi.Guid;
                        var index = i;
                        var ui = DiziList.Instance(v => new Prefab_Pop(v));
                        ui.Init(dizi);
                    }
                }
                private void ClearList()
                {
                    DiziList.ClearList(ui => ui.Destroy());
                }
                private class Prefab_Pop : UiBase
                {
                    private Text text_name { get; }
                    private Image img_avatar { get; }
                    private Text text_mapName { get; }
                    public Prefab_Pop(IView v) : base(v, true)
                    {
                        text_name = v.GetObject<Text>("text_name");
                        img_avatar = v.GetObject<Image>("img_avatar");
                        text_mapName = v.GetObject<Text>("text_mapName");
                    }
                    public void SetIcon(Sprite ico) => img_avatar.sprite = ico;

                    internal void Init(Dizi dizi)
                    {
                        text_name.text = dizi.Name;
                        text_name.color = Game.GetColorFromGrade(dizi.Grade);
                        //text_mapName.text = mapName;
                    }
                }
            }
        }
    }
}