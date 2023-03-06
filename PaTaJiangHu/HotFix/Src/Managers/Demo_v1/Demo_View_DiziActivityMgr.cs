using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_View_DiziActivityMgr : MainPageBase
    {
        private View_DiziActivity View_diziActivity { get; set; }
        private DiziAdvController DiziAdvController { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;
        protected override string ViewName => "demo_view_diziActivity";
        protected override bool IsDynamicPixel => false;
        public Demo_View_DiziActivityMgr(MainUiAgent uiAgent) : base(uiAgent)
        {
            DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        }
        protected override void Build(IView view)
        {
            View_diziActivity = new View_DiziActivity(view);
        }
        protected override void RegEvents() { }
        public override void Show() => View_diziActivity.Display(true);
        public override void Hide() => View_diziActivity.Display(false);

        private class View_DiziActivity : UiBase
        {
            private ScrollContentAligner Scroll_advLog { get; }
            private View_Buttons ButtonsView { get;}
            public View_DiziActivity(IView v) : base(v, true)
            {
                Scroll_advLog = v.GetObject<ScrollContentAligner>("scroll_advLog");
                Scroll_advLog.OnResetElement += OnAdvMsgReset;
                Scroll_advLog.OnSetElement += OnLogSet;
                Scroll_advLog.Init();

                ButtonsView = new View_Buttons(v.GetObject<View>("view_buttons"),
                    onRecallAction: () => XDebug.LogWarning("当前弟子被召回门派"),
                    onAdvMapSelectAction: () => XDebug.LogWarning("当前弟子选择历练地图"),
                    onAdvStartAction: () => XDebug.LogWarning("当前弟子历练开始"),
                    onDiziForgetAction: () => XDebug.LogWarning("当前弟子遗忘互动"),
                    onDiziBuyBackAction: () => XDebug.LogWarning("当前弟子买回互动"),
                    onDiziReturnAction: () => XDebug.LogWarning("当前弟子回门派互动")
                    );
                ButtonsView.SetModes(View_Buttons.Modes.SelectMap);
            }

            private IView OnLogSet(int index, View view)
            {
                var log = new Prefab(view);
                var storyLog = SelectedDizi.Adventure.StoryLog;
                var message = storyLog[storyLog.Count - index - 1];
                log.Display(true);
                log.LogMessage(message);
                return view;
            }
            private void OnAdvMsgReset(IView v) => v.GameObject.SetActive(false);

            private Dizi SelectedDizi { get; set; }
            private class View_Buttons : UiBase
            {
                public enum Modes
                {
                    None = 0,
                    Prepare,
                    SelectMap,
                    Adventure,
                    Returning,
                    Waiting,
                    Failed
                }
                private Button Btn_callback { get; }
                private Button Btn_selectAdvMap { get; }
                private Button Btn_startAdv { get; }
                private Button Btn_forgetDizi { get; }
                private Button Btn_buybackDizi { get; }
                private Button Btn_returnDizi { get; }
                public View_Buttons(IView v,
                    Action onRecallAction,
                    Action onAdvMapSelectAction,
                    Action onAdvStartAction,
                    Action onDiziForgetAction,
                    Action onDiziBuyBackAction,
                    Action onDiziReturnAction) : base(v, true)
                {
                    Btn_callback = v.GetObject<Button>("btn_callback");
                    Btn_callback.OnClickAdd(() =>
                    {
                        onRecallAction?.Invoke();
                        SetModes(Modes.Returning); //Temporary
                    });
                    Btn_selectAdvMap = v.GetObject<Button>("btn_selectAdvMap");
                    Btn_selectAdvMap.OnClickAdd(() =>
                    {
                        onAdvMapSelectAction?.Invoke();
                        SetModes(Modes.SelectMap);
                    });
                    Btn_startAdv = v.GetObject<Button>("btn_startAdv");
                    Btn_startAdv.OnClickAdd(() =>
                    {
                        onAdvStartAction?.Invoke();
                        SetModes(Modes.Adventure);
                    });
                    Btn_forgetDizi = v.GetObject<Button>("btn_forgetDizi");
                    Btn_forgetDizi.OnClickAdd(onDiziForgetAction);
                    Btn_buybackDizi = v.GetObject<Button>("btn_buybackDizi");
                    Btn_buybackDizi.OnClickAdd(onDiziBuyBackAction);
                    Btn_returnDizi = v.GetObject<Button>("btn_returnDizi");
                    Btn_returnDizi.OnClickAdd(onDiziReturnAction);
                }

                public void SetModes(Modes mode)
                {
                    DisplayButton(Btn_callback, mode == Modes.Adventure);
                    DisplayButton(Btn_selectAdvMap, mode == Modes.SelectMap);
                    DisplayButton(Btn_startAdv, mode == Modes.Prepare);
                    DisplayButton(Btn_forgetDizi, mode == Modes.Failed);
                    DisplayButton(Btn_buybackDizi, mode == Modes.Failed);
                    DisplayButton(Btn_returnDizi, mode == Modes.Waiting);
                    void DisplayButton(Button button, bool display) => button.gameObject.SetActive(display);
                }

            }

            private class Prefab : UiBase
            {
                private View_TextHandler TextHandlerView { get; }
                private View_Reward RewardView { get; }
                public Prefab(IView v) : base(v, true)
                {
                    TextHandlerView = new View_TextHandler(v.GetObject<View>("view_textHandler"));
                    RewardView = new View_Reward(v.GetObject<View>("view_reward"));
                }
                public void LogMessage(string message)
                {
                    TextHandlerView.LogMessage(message);
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
                        View.RectTransform.SetSize(line * 20, RectTransform.Axis.Vertical);
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
                        ExpElement = new Element_Prefab(v.GetObject<View>(""));
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
                    private void SetViewReward(int exp, (string itemName, int value)[] items)
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
                        var rect = View.RectTransform.rect;
                        rect.size = new Vector2(rect.x, rows * RowHeight);
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
