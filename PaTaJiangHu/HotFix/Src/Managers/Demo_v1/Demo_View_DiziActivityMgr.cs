using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_DiziActivityMgr : MainPageBase
    {
        private View_DiziActivity View_diziActivity { get; set; }
        private DiziAdvController DiziAdvController { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_diziActivity";
        protected override bool IsDynamicPixel => true;
        private Demo_v1Agent Agent { get; }
        public Demo_View_DiziActivityMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
            DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        }
        protected override void Build(IView view)
        {
            View_diziActivity = new View_DiziActivity(v: view,
                onRecallAction: guid => DiziAdvController.AdventureRecall(guid),
                onMapListAction: (guid,mapType) => Agent.MapSelection(guid,mapType),
                onDiziForgetAction: guid => XDebug.LogWarning("当前弟子遗忘互动"),
                onDiziBuyBackAction: guid => XDebug.LogWarning("当前弟子买回互动"),
                onDiziReturnAction: guid => DiziAdvController.AdventureFinalize(guid)
            );
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Message, b =>
            {
                var guid = b.GetString(0);
                var message = b.GetString(1);
                View_diziActivity.OnActivityUpdate(guid, message);
            });
            Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Adjust, b =>
            {
                var guid = b.GetString(0);
                var adjust = b.GetString(1);
                View_diziActivity.OnActivityUpdate(guid, adjust);
            });
            Game.MessagingManager.RegEvent(EventString.Dizi_Activity_Reward, b =>
            {
                var guid = b.GetString(0);
                View_diziActivity.OnActivityUpdate(guid, string.Empty);
            });
        }
        public override void Show() => View_diziActivity.Display(true);
        public override void Hide() => View_diziActivity.Display(false);

        public void Set(Dizi dizi) => View_diziActivity.Set(dizi.Guid);

        private class View_DiziActivity : UiBase
        {
            private ScrollContentAligner Scroll_advLog { get; }
            private View_Buttons ButtonsView { get;}
            public View_DiziActivity(IView v,
                    Action<string> onRecallAction,
                    Action<string,int> onMapListAction,
                    Action<string> onDiziForgetAction,
                    Action<string> onDiziBuyBackAction,
                    Action<string> onDiziReturnAction
                ) : base(v, true)
            {
                Scroll_advLog = v.GetObject<ScrollContentAligner>("scroll_advLog");
                Scroll_advLog.OnResetElement += OnAdvMsgReset;
                Scroll_advLog.OnSetElement += OnLogSet;
                Scroll_advLog.Init();

                ButtonsView = new View_Buttons(v.GetObject<View>("view_buttons"),
                    () => onRecallAction(SelectedDizi.Guid),
                    () => onMapListAction(SelectedDizi.Guid, 0),
                    () => onMapListAction(SelectedDizi.Guid, 1),
                    () => onDiziForgetAction(SelectedDizi.Guid),
                    () => onDiziBuyBackAction(SelectedDizi.Guid),
                    () => onDiziReturnAction(SelectedDizi.Guid)
                );
                ButtonsView.SetMode(View_Buttons.Modes.Idle);
            }

            public void Set(string diziGuid)
            {
                var dizi = Game.World.Faction.GetDizi(diziGuid);
                SelectedDizi = dizi;
                var mode = dizi.State.Current switch
                {
                    DiziStateHandler.States.Lost => View_Buttons.Modes.Lost,
                    DiziStateHandler.States.Idle => View_Buttons.Modes.Idle,
                    DiziStateHandler.States.AdvProgress => View_Buttons.Modes.Adventure,
                    DiziStateHandler.States.AdvProduction => View_Buttons.Modes.Adventure,
                    DiziStateHandler.States.AdvReturning => View_Buttons.Modes.Returning,
                    DiziStateHandler.States.AdvWaiting => View_Buttons.Modes.Waiting,
                    _ => throw new ArgumentOutOfRangeException()
                };
                ButtonsView.SetMode(mode);
            }

            public void OnActivityUpdate(string guid, string message)
            {
                if (guid != SelectedDizi.Guid) return;
                var logs = SelectedDizi.State.LogHistory;
                Scroll_advLog.SetList(logs.Count);
            }

            private IView OnLogSet(int index, View view)
            {
                var log = new Prefab_Log(view);
                var storyLog = SelectedDizi.State.LogHistory;
                var fragment = storyLog[storyLog.Count - index - 1];
                if (fragment.Reward != null)
                    log.SetReward(fragment.Reward);
                else log.SetMessage(fragment.Message);
                log.Display(true);
                return view;
            }
            private void OnAdvMsgReset(IView v) => v.GameObject.SetActive(false);

            private Dizi SelectedDizi { get; set; }
            private class View_Buttons : UiBase
            {
                public enum Modes
                {
                    None = 0,
                    Idle,
                    Adventure,
                    Returning,
                    Waiting,
                    Lost
                }
                private Button Btn_callback { get; }
                private Button Btn_selectAdvMap { get; }
                private Button Btn_selectProMap { get; }
                private Button Btn_forgetDizi { get; }
                private Button Btn_buybackDizi { get; }
                private Button Btn_returnDizi { get; }
                public View_Buttons(IView v,
                    Action onRecallAction,
                    Action onAdvMapAction,
                    Action onProductionMapAction,
                    Action onDiziForgetAction,
                    Action onDiziBuyBackAction,
                    Action onDiziReturnAction) : base(v, true)
                {
                    Btn_callback = v.GetObject<Button>("btn_callback");
                    Btn_callback.OnClickAdd(() =>
                    {
                        onRecallAction?.Invoke();
                        SetMode(Modes.Returning); //Temporary
                    });
                    Btn_selectAdvMap = v.GetObject<Button>("btn_selectAdvMap");
                    Btn_selectAdvMap.OnClickAdd(() => onAdvMapAction?.Invoke());
                    Btn_selectProMap = v.GetObject<Button>("btn_selectProMap");
                    Btn_selectProMap.OnClickAdd(() => onProductionMapAction?.Invoke());
                    Btn_forgetDizi = v.GetObject<Button>("btn_forgetDizi");
                    Btn_forgetDizi.OnClickAdd(onDiziForgetAction);
                    Btn_buybackDizi = v.GetObject<Button>("btn_buybackDizi");
                    Btn_buybackDizi.OnClickAdd(onDiziBuyBackAction);
                    Btn_returnDizi = v.GetObject<Button>("btn_returnDizi");
                    Btn_returnDizi.OnClickAdd(onDiziReturnAction);
                }

                public void SetMode(Modes mode)
                {
                    DisplayButton(Btn_callback, mode == Modes.Adventure);
                    DisplayButton(Btn_selectAdvMap, mode == Modes.Idle);
                    DisplayButton(Btn_selectProMap, mode == Modes.Idle);
                    DisplayButton(Btn_forgetDizi, mode == Modes.Lost);
                    DisplayButton(Btn_buybackDizi, mode == Modes.Lost);
                    DisplayButton(Btn_returnDizi, mode == Modes.Waiting);
                    void DisplayButton(Button button, bool display) => button.gameObject.SetActive(display);
                }

            }

            private class Prefab_Log : UiBase
            {
                private View_TextHandler TextHandlerView { get; }
                private View_Reward RewardView { get; }
                public Prefab_Log(IView v) : base(v, true)
                {
                    TextHandlerView = new View_TextHandler(v.GetObject<View>("view_textHandler"));
                    RewardView = new View_Reward(v.GetObject<View>("view_reward"));
                }
                public void SetMessage(string message)
                {
                    TextHandlerView.LogMessage(message);
                    TextHandlerView.Display(true);
                    RewardView.Display(false);
                }

                public void SetReward(IGameReward reward)
                {
                    TextHandlerView.Display(false);
                    var list = new List<(string, int)>();
                    for (var i = 0; i < reward.AllItems.Length; i++)
                    {
                        var item = reward.AllItems[i];
                        list.Add((item.Item.Name, item.Amount));
                    }
                    list.Add(("包裹", reward.Packages.Length));
                    RewardView.SetViewReward(0, list.ToArray());
                    RewardView.Display(true);
                }

                private class View_TextHandler : UiBase
                {
                    private const int OneLine = 24;
                    private Text Text_time { get; }
                    private Text Text_message { get; }
                    public View_TextHandler(IView v) : base(v, true)
                    {
                        Text_message = v.GetObject<Text>("text_message");
                    }
                    public void LogMessage(string message)
                    {
                        var line = message.Length / OneLine;
                        Text_message.text = message;
                        View.RectTransform.rect.size.SetY(line * 20);
                    }
                    public void ResetUi() => Text_message.text = string.Empty;
                }

                private class View_Reward : UiBase
                {
                    private Element_Prefab ExpElement { get; }
                    private Element_Prefab RewardElement1 { get; }
                    private Element_Prefab RewardElement2 { get; }
                    private Element_Prefab RewardElement3 { get; }
                    private Element_Prefab RewardElement4 { get; }
                    private Element_Prefab RewardElement5 { get; }
                    private Element_Prefab RewardElement6 { get; }
                    private Element_Prefab RewardElement7 { get; }
                    private Element_Prefab RewardElement8 { get; }
                    private Element_Prefab RewardElement9 { get; }
                    private Element_Prefab RewardElement10 { get; }
                    private Element_Prefab RewardElement11 { get; }
                    private Element_Prefab RewardElement12 { get; }
                    private Element_Prefab RewardElement13 { get; }
                    private List<Element_Prefab> AllRewardElement { get; }
                    public View_Reward(IView v) : base(v, true)
                    {
                        ExpElement = new Element_Prefab(v.GetObject<View>("element_exp"));
                        RewardElement1 = new Element_Prefab(v.GetObject<View>("element_reward1"));
                        RewardElement2=new Element_Prefab(v.GetObject<View>("element_reward2"));
                        RewardElement3=new Element_Prefab(v.GetObject<View>("element_reward3"));
                        RewardElement4=new Element_Prefab(v.GetObject<View>("element_reward4"));
                        RewardElement5=new Element_Prefab(v.GetObject<View>("element_reward5"));
                        RewardElement6=new Element_Prefab(v.GetObject<View>("element_reward6"));
                        RewardElement7=new Element_Prefab(v.GetObject<View>("element_reward7"));
                        RewardElement8=new Element_Prefab(v.GetObject<View>("element_reward8"));
                        RewardElement9=new Element_Prefab(v.GetObject<View>("element_reward9"));
                        RewardElement10=new Element_Prefab(v.GetObject<View>("element_reward10"));
                        RewardElement11=new Element_Prefab(v.GetObject<View>("element_reward11"));
                        RewardElement12=new Element_Prefab(v.GetObject<View>("element_reward12"));
                        RewardElement13=new Element_Prefab(v.GetObject<View>("element_reward13"));
                        AllRewardElement = new List<Element_Prefab>()
                        {
                            RewardElement1,
                            RewardElement2,
                            RewardElement3,
                            RewardElement4,
                            RewardElement5,
                            RewardElement6,
                            RewardElement7,
                            RewardElement8,
                            RewardElement9,
                            RewardElement10,
                            RewardElement11,
                            RewardElement12,
                            RewardElement13
                        };
                    }
                    public void SetViewReward(int exp, (string itemName, int value)[] items)
                    {
                        ExpElement.Set(exp);
                        var totalElements = items.Length;
                        totalElements += exp > 0 ? 1 : 0;
                        var rows = totalElements switch 
                        {
                            > 9 => 3,
                            > 4 => 2,
                            _ => 1 
                        };
                        SetPrefabSize(rows);
                        for (var i=0; i <AllRewardElement.Count; i++)
                        {
                            var element = AllRewardElement[i];
                            if(i >= items.Length)
                            {
                                element.Hide();
                                continue;
                            }
                            var (title, value) = items[i];
                            element.Set(value, title);
                        }
                    }
                    private void SetPrefabSize(int rows)
                    {
                        const int RowHeight = 60;
                        View.RectTransform.rect.size.SetY(rows * RowHeight);
                    }
                    private class Element_Prefab : UiBase
                    {
                        private Text Text_title { get; }
                        private Text Text_value { get; }
                        public Element_Prefab(IView v) : base(v, true)
                        {
                            Text_title = v.GetObject<Text>("text_title");
                            Text_value = v.GetObject<Text>("text_value");
                        }
                        public void Set(int value, string title = null)
                        {
                            Display(value > 0);
                            Text_value.text = value.ToString();
                            if (title != null) Text_title.text = title;
                        }
                        public void Hide() => Display(false);
                    }
                }

            }
        }
    }
}
