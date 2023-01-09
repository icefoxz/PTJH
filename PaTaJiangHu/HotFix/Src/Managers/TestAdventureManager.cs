using System;
using System.Collections.Generic;
using System.Linq;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;
using Object = UnityEngine.Object;

namespace HotFix_Project.Managers
{
    /// <summary>
    /// 冒险任务管理器，主要管理UI与本地数据的交互
    /// </summary>
    internal class TestAdventureManager 
    {
        private TestStory AdvStory { get; set; }
        private TestEvent EventWindow { get; set; }
        private ITestAdventureController Controller { get; set; }

        public void Init()
        {
            Controller = TestCaller.Instance.InstanceAdventureController();
            InitUi();
            EventReg();
        }

        private void InitUi()
        {
            Game.UiBuilder.Build("test_testEvent",
                v => EventWindow = new TestEvent(v, Controller.OnEventInvoke));
            Game.UiBuilder.Build("test_testAdvStory", v => AdvStory = new TestStory(v, Controller.OnStartMapEvent));
        }

        private void EventReg()
        {
            Game.MessagingManager.RegEvent(EventString.Test_AdventureMap, bag =>
            {
                var list = bag.Get<TestAdventureController.Story[]>(0);
                AdvStory.UpdateStory(list);
                AdvStory.Display(true);
            });
            Game.MessagingManager.RegEvent(EventString.Test_AdvEventInvoke, bag =>
            {
                var advEvent = bag.Get<TestAdventureController.AdvEvent>(0);
                EventWindow.PromptEvent(advEvent.AdvType, bag);
            });
        }

        private class TestStory : UiBase
        {
            private enum Modes
            {
                None,
                Preview,
                Process
            }
            private ScrollRect Scroll_map { get; }
            private View Prefab_map { get; }
            private Button Btn_startEvent { get; }

            private EventProcess Process { get; }
            private EventPreview Preview { get; }
            private ListViewUi<MapUi> Maps { get; }
            private event Action<int> OnStartMapEvent;
            
            private void SetMode(Modes mode)
            {
                Preview.Display(mode == Modes.Preview);
                Process.Display(mode == Modes.Process);
            }
            private void SetNextEvent(Action onStartEventAction) => Btn_startEvent.OnClickAdd(onStartEventAction);
            private void UpdateEventPreview(string[] events)
            {
                Preview.RefreshEvents(events);
                SetMode(Modes.Preview);
            }
            private void OnMapSelected(MapUi selectedUi)
            {
                foreach (var ui in Maps.List) 
                    ui.SetSelected(ui == selectedUi);
            }

            public TestStory(IView v, Action<int> onStartMapEvent) : base(v.GameObject, false)
            {
                Scroll_map = v.GetObject<ScrollRect>("scroll_map");
                Prefab_map = v.GetObject<View>("prefab_map");
                Btn_startEvent = v.GetObject<Button>("btn_startEvent");
                Preview = new EventPreview(v.GetObject<View>("view_eventPreview"));
                Process = new EventProcess(v.GetObject<View>("view_eventProcess"));
                Maps = new ListViewUi<MapUi>(Prefab_map, Scroll_map.content.gameObject);
                OnStartMapEvent = onStartMapEvent;
            }

            //public void SetEvent(AdventureController.AdvEvent advEvent, string arg)
            //{
            //    Process.ClearEvents();
            //    Process.AddProcessEvent($"{advEvent.Name}", () => EventWindow.PromptEvent(advEvent.AdvType, arg));
            //    SetMode(Modes.Process);
            //}
            public void UpdateStory(TestAdventureController.Story[] list)
            {
                Maps.ClearList(m => m.Destroy());
                foreach (var m in list)
                {
                    var events = m.AllEvents;
                    Maps.Instance(v => new MapUi(v, m.Name, selectedUi =>
                    {
                        UpdateEventPreview(events);
                        OnMapSelected(selectedUi);
                        SetNextEvent(()=>OnStartMapEvent?.Invoke(m.Id));
                    }));
                }
                SetMode(Modes.None);
            }

            private class EventPreview : UiBase
            {
                private ScrollRect Next_event { get; }
                private View Prefab_nextEvent { get; }
                private ListViewUi<EventBtnUi> EventBtnList { get; }

                public EventPreview(IView v) : base(v.GameObject,true)
                {
                    Next_event = v.GetObject<ScrollRect>("scroll_nextEvent");
                    Prefab_nextEvent = v.GetObject<View>("prefab_nextEvent");
                    EventBtnList = new ListViewUi<EventBtnUi>(Prefab_nextEvent, Next_event.content.gameObject);
                }

                public void RefreshEvents(string[] events)
                {
                    EventBtnList.ClearList(e => e.Destroy());
                    foreach (var title in events) 
                        EventBtnList.Instance(v => new EventBtnUi(v, title, null));

                }
                private class EventBtnUi : UiBase
                {
                    private Text Text_title { get; }
                    private Button Btn { get; }
                    public EventBtnUi(IView v,string title,Action onclickAction) : base(v.GameObject, true)
                    {
                        Text_title = v.GetObject<Text>("text_eventTitle");
                        Btn = v.GameObject.GetComponent<Button>();
                        Text_title.text = title;
                        Btn.OnClickAdd(onclickAction);
                    }

                    public void Destroy()
                    {
                        Display(false);
                        Object.Destroy(gameObject);
                    }
                }
            }
            private class EventProcess : UiBase
            {
                private Transform Trans_processContent { get; }
                private View Prefab_interaction { get; }

                private ListViewUi<InteractionUi> InteractListView { get; }
                public EventProcess(IView v) : base(v.GameObject, true)
                {
                    Trans_processContent = v.GetObject<Transform>("trans_processContent");
                    Prefab_interaction = v.GetObject<View>("prefab_interaction");
                    InteractListView = new ListViewUi<InteractionUi>(Prefab_interaction, Trans_processContent.gameObject);
                }

                public void AddProcessEvent(string title, Action onclickAction) =>
                    InteractListView.Instance(v => new InteractionUi(v, title, onclickAction));

                private class InteractionUi : UiBase
                {
                    private Button Btn_interaction { get; }
                    private Text Text_interaction { get; }

                    public InteractionUi(IView v, string title, Action onclickAction) : base(
                        v.GameObject, true)
                    {
                        Btn_interaction = v.GetObject<Button>("btn_interaction");
                        Text_interaction = v.GetObject<Text>("text_interaction");
                        Text_interaction.text = title;
                        Btn_interaction.OnClickAdd(onclickAction);
                    }

                    public void Destroy()
                    {
                        Display(false);
                        Object.Destroy(gameObject);
                    }
                }

                public void ClearEvents() => InteractListView.ClearList(ui => ui.Destroy());
            }
            private class MapUi : UiBase
            {
                private Text Text_title { get; }
                private Image Img_selected { get; }
                private Button Button { get; }
                public MapUi(IView v,string title,Action<MapUi> onClickAction) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Img_selected = v.GetObject<Image>("img_selected");
                    Button = v.GameObject.GetComponent<Button>();
                    Text_title.text = title;
                    Button.OnClickAdd(() => onClickAction(this));
                }

                public void Destroy()
                {
                    Display(false);
                    Object.Destroy(gameObject);
                }

                public void SetSelected(bool selected) => Img_selected.gameObject.SetActive(selected);
            }
        }
        private class TestEvent : UiBase
        {
            private StoryEvent StoryEventView { get; }
            private OptionEvent OptionEventView { get; }
            private QuitEvent QuitEventView { get; }
            private DialogEvent DialogEventView { get; }
            private BattleEvent BattleEventView { get; }
            private RewardEvent RewardEventView { get; }

            private event Action<int, int> OnNextEvent;
            private void WindowDisplay(AdvTypes advType)
            {
                StoryEventView.Display(advType == AdvTypes.Story);
                OptionEventView.Display(advType is AdvTypes.Option or AdvTypes.Term or AdvTypes.Pool);
                QuitEventView.Display(advType == AdvTypes.Quit);
                DialogEventView.Display(advType == AdvTypes.Dialog);
                BattleEventView.Display(advType == AdvTypes.Battle);
                RewardEventView.Display(advType == AdvTypes.Reward);
            }

            public TestEvent(IView v,Action<int,int> nextEventAction) : base(v.GameObject, false)
            {
                StoryEventView = new StoryEvent(v.GetObject<View>("view_story"));
                OptionEventView = new OptionEvent(v.GetObject<View>("view_options"));
                QuitEventView = new QuitEvent(v.GetObject<View>("view_quit"));
                DialogEventView = new DialogEvent(v.GetObject<View>("view_dialog"));
                BattleEventView = new BattleEvent(v.GetObject<View>("view_battle"));
                RewardEventView = new RewardEvent(v.GetObject<View>("view_reward"));
                OnNextEvent = nextEventAction;
            }

            public void PromptEvent(AdvTypes advType, ObjectBag bag)
            {
                Display(true);
                switch (advType)
                {
                    case AdvTypes.Story:
                    {
                        var storyEvent = bag.Get<TestAdventureController.BriefEvent>(0);
                        var nextIndex = storyEvent.NextIndexes.First();
                        StoryEventView.Set(storyEvent.Name, storyEvent.Text,
                            () => OnNextEvent?.Invoke(storyEvent.StoryId, nextIndex));
                        break;
                    }
                    case AdvTypes.Option:
                    case AdvTypes.Term:
                    case AdvTypes.Pool:
                    {
                        var optionEvent = bag.Get<TestAdventureController.OptionEvent>(0);
                        OptionEventView.Set(optionEvent.Name, optionEvent.Story);
                        for (var i = 0; i < optionEvent.Options.Length; i++)
                        {
                            var title = optionEvent.Options[i];
                            var nextId = optionEvent.NextIndexes[i];
                            OptionEventView.AddOption(title, () => OnNextEvent?.Invoke(optionEvent.StoryId, nextId));
                        }
                        break;
                    }
                    case AdvTypes.Quit:
                    {
                        var advEvent = bag.Get<TestAdventureController.AdvEvent>(0);
                        QuitEventView.Set(advEvent.Name, () => Display(false));
                        break;
                    }
                    case AdvTypes.Dialog:
                    {
                        var dialogEvent = bag.Get<TestAdventureController.DialogEvent>(0);
                        var nextIndex = dialogEvent.NextIndexes.First();
                        DialogEventView.Set(dialogEvent, () => OnNextEvent?.Invoke(dialogEvent.StoryId, nextIndex));
                        break;
                    }
                    case AdvTypes.Battle:
                    {
                        var battleEvent = bag.Get<TestAdventureController.BattleEvent>(0);
                        BattleEventView.Set(battleEvent.Name, battleEvent.ResultEvents, battleEvent.NextIndexes,
                            eventIndex => OnNextEvent?.Invoke(battleEvent.StoryId, eventIndex));
                        break;
                    }
                    case AdvTypes.Reward:
                    {
                        var rewardEvent = bag.Get<TestAdventureController.RewardEvent>(0);
                        RewardEventView.Set(rewardEvent.Name, rewardEvent.Rewards,
                            () => OnNextEvent?.Invoke(rewardEvent.StoryId, rewardEvent.NextIndexes.First()));
                        break;
                    }
                    case AdvTypes.Adjust:
                    case AdvTypes.Simulation:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(advType), advType.ToString(), null);
                }
                WindowDisplay(advType);
            }

            private class StoryEvent : UiBase
            {
                private Text Text_story { get; }
                private Text Text_title { get; }
                private Button Btn_nextEvent { get; }

                public StoryEvent(IView v) : base(v.GameObject, false)
                {
                    Text_story = v.GetObject<Text>("text_story");
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_nextEvent = v.GetObject<Button>("btn_nextEvent");
                }

                public void Set(string title,string story,Action nextEventAction)
                {
                    Text_title.text = title;
                    Text_story.text = story;
                    Btn_nextEvent.OnClickAdd(() =>
                    {
                        nextEventAction();
                        ResetUi();
                    });
                }

                private void ResetUi()
                {
                    Text_title.text = string.Empty;
                    Text_story.text = string.Empty;
                }
            }
            private class OptionEvent : UiBase
            {
                private Text Text_title { get; }
                private Text Text_story { get; }
                private Transform Trans_optionContent { get; }
                private View Prefab_option { get; }

                private ListViewUi<OptionUi> OptionListView { get; }
                public OptionEvent(IView v) : base(v.GameObject, false)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_story = v.GetObject<Text>("text_story");
                    Trans_optionContent = v.GetObject<Transform>("trans_optionContent");
                    Prefab_option = v.GetObject<View>("prefab_option");
                    OptionListView = new ListViewUi<OptionUi>(Prefab_option, Trans_optionContent.gameObject);
                }

                public void Set(string title, string story)
                {
                    Text_title.text = title;
                    Text_story.text = story;
                }

                public void AddOption(string title, Action callbackAction)
                {
                    OptionListView.Instance(v => new OptionUi(v, title, () =>
                    {
                        OptionListView.ClearList(ui => ui.Destroy());
                        callbackAction();
                    }));
                }

                private class OptionUi : UiBase
                {
                    private Button Button_option { get; }
                    private Text Text_title { get; }

                    public OptionUi(View v, string text, Action onclickAction) : base(v.GameObject, true)
                    {
                        Text_title = v.GetObject<Text>("text_title");
                        Button_option = v.GetObject<Button>("btn_option");
                        Text_title.text = text;
                        Button_option.OnClickAdd(onclickAction);
                    }

                    public void Destroy()
                    {
                        Display(false);
                        Object.Destroy(gameObject);
                    }
                }
            }
            private class QuitEvent : UiBase
            {
                private Text Text_title { get; }
                private Button Btn_exit { get; }
                public QuitEvent(IView v) : base(v.GameObject, false)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_exit = v.GetObject<Button>("btn_exit");
                }

                public void Set(string title, Action onClickAction)
                {
                    Text_title.text = title;
                    Btn_exit.OnClickAdd(onClickAction);
                }
            }
            private class DialogEvent : UiBase
            {
                private Text Text_title { get; }
                private Text Text_charName { get; }
                private Text Text_dialog { get; }
                private Button Btn_nextEvent { get; }
                private Button Btn_nextDialog { get; }

                private List<(int Id, string Name, string Message)> Dialog { get; set; } =
                    new List<(int Id, string Name, string Message)>();
                private void NextDialog()
                {
                    var d = Dialog[0];
                    var name = d.Id switch
                    {
                        0 => "玩家",
                        -1 => d.Name,
                        _ => d.Id.ToString()
                    };
                    Text_charName.text = name;
                    Text_dialog.text = d.Message;
                    Dialog.RemoveAt(0);
                    CheckDialog();
                }
                private void CheckDialog()
                {
                    var noDialog = Dialog.Count == 0;
                    Btn_nextEvent.gameObject.SetActive(noDialog);
                    Btn_nextDialog.gameObject.SetActive(!noDialog);
                }

                public DialogEvent(IView v) : base(v.GameObject, false)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_charName = v.GetObject<Text>("text_charName");
                    Text_dialog = v.GetObject<Text>("text_dialog");
                    Btn_nextEvent = v.GetObject<Button>("btn_nextEvent");
                    Btn_nextDialog = v.GetObject<Button>("btn_nextDialog");
                    Btn_nextDialog.OnClickAdd(NextDialog);
                }

                public void Set(TestAdventureController.DialogEvent dialogEvent,Action onDialogComplete)
                {
                    Text_title.text = dialogEvent.Name;
                    var array = new(int Id, string Name, string Message)[dialogEvent.Names.Length];
                    for (var i = 0; i < dialogEvent.Names.Length; i++)
                    {
                        array[i].Id = i;
                        array[i].Name = dialogEvent.Names[i];
                        array[i].Message = dialogEvent.Messages[i];
                    }
                    Dialog = array.ToList();
                    Btn_nextEvent.OnClickAdd(() =>
                    {
                        onDialogComplete();
                        ResetUi();
                    });
                    NextDialog();
                }

                private void ResetUi()
                {
                    Text_charName.text = string.Empty;
                    Text_dialog.text = string.Empty;
                    Text_dialog.text = string.Empty;
                    Dialog.Clear();
                }
            }
            private class BattleEvent : UiBase
            {
                private Text Text_brief { get; }
                private Text Text_win { get; }
                private Text Text_lose { get; }
                private Text Text_kill { get; }
                private Text Text_escape { get; }
                private GameObject Obj_duel { get; }
                private GameObject Obj_fight { get; }
                private Button Btn_win { get; }
                private Button Btn_lose { get; }
                private Button Btn_kill { get; }
                private Button Btn_escape { get; }

                public BattleEvent(IView v) : base(v.GameObject, false)
                {
                    Text_brief = v.GetObject<Text>("text_brief");
                    Obj_duel = v.GetObject("obj_duel");
                    Obj_fight = v.GetObject("obj_fight");
                    Btn_win = v.GetObject<Button>("btn_win");
                    Btn_lose = v.GetObject<Button>("btn_lose");
                    Btn_kill = v.GetObject<Button>("btn_kill");
                    Btn_escape = v.GetObject<Button>("btn_escape");
                    Text_win = v.GetObject<Text>("text_win");
                    Text_lose = v.GetObject<Text>("text_lose");
                    Text_kill = v.GetObject<Text>("text_kill");
                    Text_escape = v.GetObject<Text>("text_escape");
                }

                public void Set(string brief, string[] nextEvents, int[] nextIds, Action<int> onNextEventAction)
                {
                    var isDuel = nextEvents.Length == 2;
                    Text_brief.text = brief;
                    Text_win.text = nextEvents[0];
                    Text_lose.text = nextEvents[1];
                    Btn_win.OnClickAdd(()=>onNextEventAction(nextIds[0]));
                    Btn_lose.OnClickAdd(() => onNextEventAction(nextIds[1]));
                    Obj_fight.gameObject.SetActive(!isDuel);
                    if (isDuel) return;
                    Text_kill.text = nextEvents[2];
                    Text_escape.text = nextEvents[3];
                    Btn_kill.OnClickAdd(() => onNextEventAction(nextIds[2]));
                    Btn_escape.OnClickAdd(() => onNextEventAction(nextIds[3]));
                }
            }
            private class RewardEvent : UiBase
            {
                private ScrollRect Scroll_reward { get; }
                private Button Btn_nextEvent { get; }
                private Text Text_title { get; }

                private ListViewUi<ItemUi> ItemListView { get; }
                public RewardEvent(IView v) : base(v.GameObject, false)
                {
                    Scroll_reward = v.GetObject<ScrollRect>("scroll_reward");
                    Btn_nextEvent = v.GetObject<Button>("btn_nextEvent");
                    Text_title = v.GetObject<Text>("text_title");
                    var prefab = v.GetObject<View>("prefab_item");
                    ItemListView = new ListViewUi<ItemUi>(prefab, Scroll_reward.content.gameObject);
                }

                private class ItemUi : UiBase
                {
                    private Text Text_message { get; }
                    public ItemUi(IView v,string title) : base(v.GameObject, true)
                    {
                        Text_message = v.GetObject<Text>("text_message");
                        Text_message.text = title;
                    }

                    public void Destroy()
                    {
                        Display(false);
                        Object.Destroy(gameObject);
                    }
                }

                public void Set(string title, string[] rewards, Action onclickAction)
                {
                    Text_title.text = title;
                    foreach (var reward in rewards)
                        ItemListView.Instance(v => new ItemUi(v, reward));
                    Btn_nextEvent.OnClickAdd(() =>
                    {
                        onclickAction();
                        ItemListView.ClearList(ui => ui.Destroy());
                    });
                }
            }
        }
    }
}