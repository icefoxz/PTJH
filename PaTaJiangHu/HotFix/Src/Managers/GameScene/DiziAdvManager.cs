using System;
using _GameClient.Models;
using DiziM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class DiziAdvManager : MainPageBase
{
    private View_diziAdv DiziAdv { get; set; }

    private DiziController DiziController { get; set; }
    private DiziAdvController DiziAdvController { get; set; }
    private FactionController FactionController { get; set; }

    protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Game;
    protected override string ViewName => "view_diziAdv";
    protected override bool IsDynamicPixel => true;

    public DiziAdvManager(GameSceneAgent uiAgent) : base(uiAgent)
    {
        DiziController = Game.Controllers.Get<DiziController>();
        DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        FactionController = Game.Controllers.Get<FactionController>();
    }

    protected override void Build(IView view)
    {
        DiziAdv = new View_diziAdv(v: view, DiziAdvController,
            onItemSelectAction: (guid, itemType) => MainUiAgent.Show<WinEquipmentManager>(mgr=>mgr.Set(guid,itemType,0),true),
            onAdvStartAction: (guid, index) => DiziAdvController.AdventureStart(guid, index),
            onAdvRecallAction: guid => DiziAdvController.AdventureRecall(guid),
            onDiziFinalizeAction: guid => DiziAdvController.AdventureFinalize(guid),
            onDiziForgetAction: guid => XDebug.LogWarning("弟子遗忘功能未完!"),
            onDiziBuyBackAction: guid => XDebug.LogWarning("弟子买回功能未完!"),
            onEquipSlotAction: (guid, slot) =>
                Game.MessagingManager.SendParams(EventString.Dizi_Adv_SlotManagement, guid, slot),
            onSwitchAction: () => XDebug.LogWarning("切换弟子管理页面!, 功能未完!"),
            onConditionClick: (guid, con) => FactionController.ConsumeResourceByStep(guid, con)
        );
    }

    protected override void RegEvents()
    {
        //todo: 方法被弃用, 现在是使用UiAgent来管理Ui与Ui的交互
        //Game.MessagingManager.RegEvent(EventString.Dizi_AdvManagement, bag =>
        //{
        //    DiziAdv.Set(bag);
        //    //MainUi.MainPage.HideAll(MainPageLayout.Sections.Mid);
        //    MainUiAgent.Show(this);
        //});
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_EventMessage, bag =>
        {
            var diziGuid = bag.Get<string>(0);
            DiziAdv.AdvMsgUpdate(diziGuid);
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Recall, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_End, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Finalize, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Page_DiziList, bag =>
        {
            //MainUi.MainPage.HideAll(MainPageLayout.Sections.Mid);
            MainUiAgent.Show(this, true);
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate, bag => DiziAdv.Update(bag.GetString(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_SlotUpdate, bag => DiziAdv.SlotUpdate(bag.Get<string>(0)));
    }

    public override void Show() => DiziAdv.Display(true);

    public override void Hide() => DiziAdv.Display(false);

    private class View_diziAdv : UiBase
    {
        public enum Skills { Combat,Force,Dodge }
        public enum Conditions { Food, State, Silver, Injury, Inner }
        public enum Items { Weapon,Armor }

        private Button Btn_Switch { get; }
        //private Button Btn_Condition { get; } //新添加： Prompt 弟子道具银子使用窗口
        private ElementManager ElementMgr { get; }
        private View_advLayout AdvLayoutView { get; }

        public View_diziAdv(IView v, DiziAdvController controller,
            Action<string,int> onItemSelectAction, 
            Action<string, int> onAdvStartAction,
            Action<string> onAdvRecallAction,
            Action<string> onDiziFinalizeAction,
            Action<string> onDiziForgetAction,
            Action<string> onDiziBuyBackAction,
            Action<string,int> onEquipSlotAction,
            Action<string, IAdjustment.Types> onConditionClick,
            Action onSwitchAction
            ) : base(v.GameObject, false)
        {
            Btn_Switch = v.GetObject<Button>("btn_switch");
            Btn_Switch.OnClickAdd(onSwitchAction);
            //Btn_Condition = v.GetObject<Button>("btn_condition");//新添加
            //Btn_Condition.OnClickAdd(() => { onLayoutClick?.Invoke(SelectedDizi?.Guid); });   //新添加
            ElementMgr = new ElementManager(
                new Element_con(v.GetObject<View>("element_conFood"),
                    () => onConditionClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Food)),
                new Element_con(v.GetObject<View>("element_conState"), 
                    () => onConditionClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Emotion)),
                new Element_con(v.GetObject<View>("element_conSilver"),
                    () => onConditionClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Silver)),
                new Element_con(v.GetObject<View>("element_conInjury"),
                    () => onConditionClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Injury)),
                new Element_con(v.GetObject<View>("element_conInner"), 
                    () => onConditionClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Inner)),
                new Element_skill(v.GetObject<View>("element_skillCombat")),
                new Element_skill(v.GetObject<View>("element_skillForce")),
                new Element_skill(v.GetObject<View>("element_skillDodge")),
                new Element_item(v.GetObject<View>("element_itemWeapon"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 0)),
                new Element_item(v.GetObject<View>("element_itemArmor"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 1))
            );
            AdvLayoutView = new View_advLayout(v: v.GetObject<View>("view_advLayout"),
                controller,
                onAdvStartAction: index => onAdvStartAction?.Invoke(SelectedDizi?.Guid, index),
                onRecallAction: () => onAdvRecallAction?.Invoke(SelectedDizi?.Guid),
                onDiziFinalizeAction: () => onDiziFinalizeAction?.Invoke(SelectedDizi?.Guid),
                onDiziForgetAction: () => onDiziForgetAction?.Invoke(SelectedDizi?.Guid),
                onDiziBuyBackAction: () => onDiziBuyBackAction?.Invoke(SelectedDizi?.Guid),
                onEquipSlotAction: index => onEquipSlotAction?.Invoke(SelectedDizi?.Guid, index)
            );
        }

        private Dizi SelectedDizi { get; set; }//cache
        public void Set(ObjectBag bag)
        {
            var guid = bag.Get<string>(0);
            var dizi = Game.World.Faction.GetDizi(guid);
            SelectedDizi = dizi;
            Update(SelectedDizi.Guid);
        }
        public void Update(string guid)
        {
            if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
            SetDizi(SelectedDizi);
            SlotUpdate(SelectedDizi?.Guid);
            var dizi = SelectedDizi;
            var isIdleState = dizi.State.Current == DiziStateHandler.States.Idle;
            ElementMgr.SetInteraction(isIdleState);
            AdvLayoutView.SetInteraction(isIdleState);
        }
        public void SlotUpdate(string diziGuid)
        {
            if (SelectedDizi.Guid == diziGuid) 
                AdvLayoutView.UpdateSlot(SelectedDizi);
        }
        private void SetDizi(Dizi dizi)
        {
            SetDiziElements(dizi);
            AdvLayoutView.Set(dizi); //历练模式
        }

        private void SetDiziElements(Dizi dizi)
        {
            var controller = Game.Controllers.Get<DiziController>();
            //ElementMgr.SetSkill(Skills.Combat, dizi.CombatSkill.Name, dizi.CombatSkill.Level);
            //ElementMgr.SetSkill(Skills.Force, dizi.ForceSkill.Name, dizi.ForceSkill.Level);
            //ElementMgr.SetSkill(Skills.Dodge, dizi.DodgeSkill.Name, dizi.DodgeSkill.Level);
            if (dizi.Weapon == null) ElementMgr.ClearItem(Items.Weapon);
            else ElementMgr.SetItem(Items.Weapon, dizi.Weapon.Name, (int) dizi.Weapon.Grade);
            if (dizi.Armor == null) ElementMgr.ClearItem(Items.Armor);
            else ElementMgr.SetItem(Items.Armor, dizi.Armor.Name, (int) dizi.Armor.Grade);
            var (foodText, fColor) = controller.GetFoodCfg(dizi.Food.ValueMaxRatio);
            var (emoText, eColor) = controller.GetEmotionCfg(dizi.Emotion.ValueMaxRatio);
            var (silverText, sColor) = controller.GetSilverCfg(dizi.Silver.ValueMaxRatio);
            var (injuryText, jColor) = controller.GetInjuryCfg(dizi.Injury.ValueMaxRatio);
            var (innerText, nColor) = controller.GetInnerCfg(dizi.Inner.ValueMaxRatio);
            SetElement(Conditions.Silver, dizi.Silver, silverText, sColor, 100);
            SetElement(Conditions.Food, dizi.Food, foodText, fColor, dizi.Capable.Food);
            SetElement(Conditions.State, dizi.Emotion, emoText, eColor, dizi.Capable.Wine);
            SetElement(Conditions.Injury, dizi.Injury, injuryText, jColor, dizi.Capable.Herb);
            SetElement(Conditions.Inner, dizi.Inner, innerText, nColor, dizi.Capable.Pill);

            void SetElement(Conditions co, IConditionValue con, string title, Color color, int capable)
            {
                ElementMgr.SetConValue(co, con.Value, con.Max);
                ElementMgr.SetConTitle(co, title);
                ElementMgr.SetColor(co, color);
                ElementMgr.SetConsumeValue(co, capable);
            }
        }

        public void AdvMsgUpdate(string diziGuid)
        {
            if (diziGuid == SelectedDizi.Guid) AdvLayoutView.SetAdvMessages(SelectedDizi);
        }
        private class Element_skill : UiBase
        {
            private Image Img_ico { get; }
            private Text Text_level { get; }
            private Text Text_skillName { get; }
            public Element_skill(IView v) : base(v.GameObject, true)
            {
                Img_ico = v.GetObject<Image>("img_ico");
                Text_level = v.GetObject<Text>("text_level");
                Text_skillName = v.GetObject<Text>("text_skillName");
            }

            public void Set(string skillName, int level)
            {
                Text_level.text = level.ToString();
                Text_skillName.text = skillName;
            }

            public void SetImage(Sprite img) => Img_ico.sprite = img;
        }
        private class Element_item : UiBase
        {
            private Image Img_item { get; }
            private Text Text_title { get; }
            private Image Img_empty { get; }
            private Button ElementButton { get; }

            public Element_item(IView v, Action onClickAction) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_title = v.GetObject<Text>("text_title");
                Img_empty = v.GetObject<Image>("img_empty");
                ElementButton = v.GameObject.GetComponent<Button>();
                ElementButton.OnClickAdd(onClickAction);

            }
            public void SetImage(Sprite img)
            {
                Img_item.sprite = img;
                Img_item.gameObject.SetActive(true);
            }

            public void SetEmpty(bool empty)
            {
                Img_empty.gameObject.SetActive(empty);
                Img_item.gameObject.SetActive(!empty);
                Text_title.gameObject.SetActive(!empty);
            }

            public void SetTitle(string title, int grade)
            {
                Text_title.text = title;
                Text_title.color = Game.GetColorFromItemGrade(grade);
                Text_title.gameObject.SetActive(true);
            }

            public void SetInteraction(bool isInteractable)
            {
                ElementButton.interactable = isInteractable;
            }
        }
        private class Element_con : UiBase
        {
            private Scrollbar Scrbar_condition { get; }
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Text Text_title { get; }
            private Image BgImg { get; }
            private Image HandleImg { get; }
            private Image Img_consumeIco { get; }
            private Text Text_consumeValue { get; }
            private Button Btn_action { get; }

            public Element_con(IView v, Action onClickAction) : base(v.GameObject, true)
            {
                Scrbar_condition = v.GetObject<Scrollbar>("scrbar_condition");
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Text_title = v.GetObject<Text>("text_title");
                BgImg = Scrbar_condition.GetComponent<Image>();
                HandleImg = Scrbar_condition.image;
                Img_consumeIco = v.GetObject<Image>("img_consumeIco");
                Text_consumeValue = v.GetObject<Text>("text_consumeValue");
                Btn_action = v.GetObject<Button>("btn_action");
                Btn_action.OnClickAdd(onClickAction);
            }
            public void SetTitle(string title)
            {
                Text_title.text = title;
            }
            public void SetValue(int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
                Scrbar_condition.size = 1f * value / max;
            }
            public void SetColor(Color color)
            {
                HandleImg.color = color;
                BgImg.color = new Color(color.r - 0.7f, color.g - 0.7f, color.b - 0.7f);
            }
            public void SetIcon(Sprite icon) => Img_consumeIco.sprite = icon;
            public void SetConsume(int value) => Text_consumeValue.text = value.ToString();

            public void SetInteraction(bool isInteractable)
            {
                Btn_action.interactable = isInteractable;
            }
        }
        private class View_advLayout : UiBase
        {
            private enum Modes
            {
                None = 0,
                Prepare,
                SelectMap,
                Adventure,
                Returning,
                Waiting,
                Failed
            }
            private Button Btn_recall { get; }
            private Image Img_costIco { get; }
            private Text Text_cost { get; }
            private Button Btn_advStart { get; }
            private Button Btn_advFinalize { get; }
            private Button Btn_advDiziBuyback{ get; }
            private Button Btn_advDiziForget { get; }
            private View_AdvMapSelector View_advMapSelector { get; }
            private ScrollContentAligner Scroll_advLog { get; }
            //private ListViewUi<LogPrefab> LogView { get; }
            private ListViewUi<Prefab_rewardItem> RewardItemView { get; }
            private Element_equip[] ItemSlots { get; }

            public View_advLayout(IView v,
                DiziAdvController controller,
                Action<int> onAdvStartAction,
                Action onRecallAction,
                Action onDiziFinalizeAction,
                Action onDiziForgetAction,
                Action onDiziBuyBackAction,
                Action<int> onEquipSlotAction) : base(v.GameObject, true)
            {
                Scroll_advLog = v.GetObject<ScrollContentAligner>("scroll_advLog");
                Scroll_advLog.OnResetElement += OnAdvMsgReset;
                Scroll_advLog.OnSetElement += OnLogSet;
                Scroll_advLog.Init();
                //LogView = new ListViewUi<LogPrefab>(v.GetObject<View>("prefab_log"), Scroll_advLog.ScrollRect);
                RewardItemView = new ListViewUi<Prefab_rewardItem>(v, "prefab_rewardItem", "scroll_rewardItem");
                Img_costIco = v.GetObject<Image>("img_costIco");
                Text_cost = v.GetObject<Text>("text_cost");
                var slot0 = new Element_equip(v.GetObject<View>("element_equipSlot0"), () => onEquipSlotAction(0));
                var slot1 = new Element_equip(v.GetObject<View>("element_equipSlot1"), () => onEquipSlotAction(1));
                var slot2 = new Element_equip(v.GetObject<View>("element_equipSlot2"), () => onEquipSlotAction(2));
                ItemSlots = new[] { slot0, slot1, slot2 };
                Btn_recall = v.GetObject<Button>("btn_recall");
                Btn_advStart = v.GetObject<Button>("btn_advStart");
                Btn_advFinalize = v.GetObject<Button>("btn_advFinalize");
                Btn_advDiziForget = v.GetObject<Button>("btn_advDiziForget");
                Btn_advDiziBuyback = v.GetObject<Button>("btn_advDiziBuyback");
                View_advMapSelector =
                    new View_AdvMapSelector(v.GetObject<View>("view_advMapSelector"), 
                        onAdvStartAction,
                        ()=>SetModes(Modes.Prepare));
                Btn_recall.OnClickAdd(onRecallAction);
                Btn_advStart.OnClickAdd(() =>
                {
                    View_advMapSelector.ListMaps(controller.AutoAdvMaps(0));
                    SetModes(Modes.SelectMap);
                });
                Btn_advFinalize.OnClickAdd(onDiziFinalizeAction);
                Btn_advDiziForget.OnClickAdd(onDiziForgetAction);
                Btn_advDiziBuyback.OnClickAdd(onDiziBuyBackAction);
            }

            private void OnAdvMsgReset(IView v)
            {
                v.GameObject.SetActive(false);
            }

            private Dizi SelectedDizi { get; set; }

            public void SetCost(Sprite icon, int cost)
            {
                Img_costIco.sprite = icon;
                Text_cost.text = cost.ToString();
            }

            public void Set(Dizi dizi)
            {
                SelectedDizi = dizi;
                //LogView.ClearList(ui => ui.Destroy());
                var mode = Modes.None;
                mode = dizi.State.Current switch
                {
                    DiziStateHandler.States.Lost => Modes.Returning,
                    DiziStateHandler.States.Idle => Modes.Prepare,
                    DiziStateHandler.States.AdvProgress => Modes.Adventure,
                    DiziStateHandler.States.AdvProduction => Modes.Adventure,
                    DiziStateHandler.States.AdvReturning => Modes.Returning,
                    DiziStateHandler.States.AdvWaiting => Modes.Waiting,
                    _ => throw new ArgumentOutOfRangeException()
                };
                SetModes(mode);
            }

            public void SetAdvMessages(Dizi d)
            {
                if (SelectedDizi.Guid != d.Guid) return;
                var messageCount = 0;
                var isAdventureMode = SelectedDizi.State is
                {
                    Current:
                    DiziStateHandler.States.AdvProduction or
                    DiziStateHandler.States.AdvProgress or
                    DiziStateHandler.States.AdvReturning or
                    DiziStateHandler.States.AdvWaiting
                };
                if (isAdventureMode)
                    messageCount = SelectedDizi.State.Adventure.StoryLog.Count;
                Scroll_advLog.SetList(messageCount);
            }

            private IView OnLogSet(int index, View view)
            {
                var log = new LogPrefab(view);
                var storyLog = SelectedDizi.State.Adventure.StoryLog;
                var message = storyLog[storyLog.Count - index - 1];
                log.Display(true);
                log.LogMessage(message);
                return view;
            }

            public void UpdateSlot(Dizi dizi)
            {
                if (SelectedDizi.Guid != dizi.Guid) return;
                for (var i = 0; i < ItemSlots.Length; i++)
                {
                    var ui = ItemSlots[i];
                    var slot = dizi.AdvItems[i];
                    if (slot == null)
                        ui.ClearItem(); //没有物件
                    else
                    {
                        ui.SetItem(slot.Item?.Name);
                    }
                }
            }

            private void SetModes(Modes mode)
            {
                DisplayButton(Btn_advStart, mode == Modes.Prepare);
                DisplayButton(Btn_recall, mode == Modes.Adventure);
                DisplayButton(Btn_advFinalize, mode == Modes.Waiting);
                DisplayButton(Btn_advDiziForget, mode == Modes.Failed);
                DisplayButton(Btn_advDiziBuyback, mode == Modes.Failed);
                View_advMapSelector.Display(mode == Modes.SelectMap);
                Scroll_advLog.gameObject.SetActive(
                    mode is Modes.Adventure 
                        or Modes.Waiting 
                        or Modes.Returning 
                        or Modes.Failed);
                SetAdvMessages(SelectedDizi);
                foreach (var slot in ItemSlots) slot.SetTimer(); //Update adv equipments
                UpdateDiziBag(SelectedDizi);
                //private method
                void DisplayButton(Button button, bool display) => button.gameObject.SetActive(display);
            }

            private void UpdateDiziBag(Dizi d)
            {
                if(SelectedDizi.Guid != d.Guid) return;
                RewardItemView.ClearList(p => p.Destroy());
                for (var i = 0; i < d.Capable.Bag; i++)
                {

                    var item = d.State.Adventure?.Rewards.Count > i ? d.State.Adventure.Rewards[i] : null;
                    var ui = RewardItemView.Instance(v => new Prefab_rewardItem(v));
                    if (item != null)
                        ui.SetIco(null);
                    else
                        ui.SetEmpty();
                }
            }

            public void SetInteraction(bool isInteractable)
            {
                for(var i = 0; i < ItemSlots.Length; i++)
                    ItemSlots[i].SetInteraction(isInteractable);
            }

            private class LogPrefab : UiBase
            {
                private const int OneLine = 20;
                private Text Text_Log { get; }
                public LogPrefab(IView v) :base(v, true)
                {
                    Text_Log = v.GetObject<Text>("text_log");
                }
                public void LogMessage(string message)
                {
                    var line = message.Length / OneLine;
                    Text_Log.text = message;
                    View.RectTransform.SetSize(line * 20, RectTransform.Axis.Vertical);
                }

                public void ResetUi() => Text_Log.text = string.Empty;
            }

            private class Element_equip : UiBase
            {
                private enum Modes{Empty,Content,InUse}
                private Scrollbar Scrbar_item { get; }
                private Image Img_ico { get; }
                private Text Text_timerMin { get; }
                private Text Text_timerSec { get; }
                private Text Text_title { get; }
                private GameObject Go_timer { get; }
                private GameObject Go_slidingArea { get; }
                private Button Btn_item { get; }
                
                public Element_equip(IView v,Action onClickAction) : base(v.GameObject, true)
                {
                    Scrbar_item = v.GetObject<Scrollbar>("scrbar_item");
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_timerMin = v.GetObject<Text>("text_timerMin");
                    Text_timerSec = v.GetObject<Text>("text_timerSec");
                    Text_title = v.GetObject<Text>("text_title");
                    Go_timer = v.GetObject("go_timer");
                    Go_slidingArea = v.GetObject("go_slidingArea");
                    Btn_item = v.GetObject<Button>("btn_item");
                    Btn_item.OnClickAdd(onClickAction);
                    SetMode(Modes.Empty);
                }

                private void SetMode(Modes mode)
                {
                    Go_slidingArea.SetActive(mode == Modes.InUse);
                    Go_timer.SetActive(mode == Modes.InUse);
                    Img_ico.gameObject.SetActive(mode != Modes.Empty);
                    Text_title.gameObject.SetActive(mode != Modes.Empty);
                }
                public void ClearItem() => SetMode(Modes.Empty);

                public void SetItem(string title, Sprite image = null)
                {
                    Text_title.text = title;
                    //Img_ico.sprite = image;
                    SetMode(Modes.Content);
                }

                public void SetTimer(int min = -1, int sec = -1)
                {
                    var isDefault = min <= 0 && sec <= 0;
                    if (!isDefault) SetMode(Modes.InUse);
                    Text_timerMin.text = isDefault ? string.Empty : min.ToString();
                    Text_timerSec.text = isDefault ? string.Empty : sec.ToString();
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_item.interactable = isInteractable;
                }
            }

            private class Prefab_rewardItem : UiBase
            {
                private enum Modes
                {
                    Empty,Item,Disable,
                }
                private Image Img_ico { get; }
                private Image Img_x { get; }

                public Prefab_rewardItem(IView v) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Img_x = v.GetObject<Image>("img_x");
                    SetEmpty();
                }

                public void SetIco(Sprite ico)
                {
                    //Img_ico.sprite = ico;
                    Set(Modes.Item);
                }
                public void SetEmpty() => Set(Modes.Empty);
                public void SetDisable() => Set(Modes.Disable);
                private void Set(Modes mode)
                {
                    Img_ico.gameObject.SetActive(mode == Modes.Item);
                    Img_x.gameObject.SetActive(mode == Modes.Disable);
                }
            }

            private class View_AdvMapSelector : UiBase
            {
                private ListViewUi<Prefab_map> MapView { get; }
                private Button Btn_advConfirm { get; }
                private Button Btn_cancel { get; }
                public View_AdvMapSelector(IView v,Action<int> onMapConfirm,Action onCancel) : base(v, false)
                {
                    Btn_advConfirm = v.GetObject<Button>("btn_advConfirm");
                    Btn_cancel = v.GetObject<Button>("btn_cancel");
                    Btn_cancel.OnClickAdd(onCancel);
                    MapView = new ListViewUi<Prefab_map>(v, "prefab_map", "scroll_selector");
                    Btn_advConfirm.OnClickAdd(() => onMapConfirm?.Invoke(SelectedMap.Id));
                }

                public void ListMaps(IAutoAdvMap[] maps)
                {
                    Btn_advConfirm.interactable = false;
                    MapView.ClearList(ui=>ui.Destroy());
                    for (var i = 0; i < maps.Length; i++)
                    {
                        var map = maps[i];
                        var index = i;
                        var ui = MapView.Instance(v => new Prefab_map(v, map, () => SetSelected(index)));
                        ui.Set(map.Name, map.About, map.ActionLingCost);
                        ui.SetImage(map.Image);
                    }
                }
                private IAutoAdvMap SelectedMap { get; set; }
                private void SetSelected(int index)
                {
                    for (var i = 0; i < MapView.List.Count; i++)
                    {
                        var ui = MapView.List[i];
                        var selected = i == index;
                        ui.SetSelected(selected);
                        if(selected) SelectedMap = ui.Map;
                    }
                    Btn_advConfirm.interactable = Game.World.Faction.ActionLing >= SelectedMap?.ActionLingCost;
                }

                private class Prefab_map : UiBase
                {
                    private Image Img_selected { get; }
                    private Image Img_map { get; }
                    private Text Text_title { get; }
                    private Text Text_about { get; }
                    private Text Text_staminaValue { get; }
                    private Button Btn_map { get; }
                    public IAutoAdvMap Map { get; }

                    public Prefab_map(IView v, IAutoAdvMap map, Action onclickAction) : base(v, true)
                    {
                        Map = map;
                        Img_selected = v.GetObject<Image>("img_selected");
                        Img_map = v.GetObject<Image>("img_map");
                        Text_title = v.GetObject<Text>("text_title");
                        Text_about = v.GetObject<Text>("text_about");
                        Text_staminaValue = v.GetObject<Text>("text_staminaValue");
                        Btn_map = v.GetObject<Button>("btn_map");
                        Btn_map.OnClickAdd(onclickAction);
                    }

                    public void SetImage(Sprite image)=> Img_map.sprite = image;
                    public void Set(string title, string about, int staminaCost)
                    {
                        Text_title.text = title;
                        Text_about.text = about;
                        Text_staminaValue.text = staminaCost.ToString();
                    }
                    public void SetSelected(bool selected) => Img_selected.gameObject.SetActive(selected);
                }
            }
        }

        private class ElementManager
        {
            private Element_con Food { get; }
            private Element_con State { get; }
            private Element_con Silver { get; }
            private Element_con Injury { get; }
            private Element_con Inner { get; }
            private Element_skill Combat { get; }
            private Element_skill Force { get; }
            private Element_skill Dodge { get; }
            private Element_item Weapon { get; }
            private Element_item Armor { get; }

            public ElementManager(Element_con food, Element_con state, Element_con silver, Element_con injury, Element_con inner, Element_skill combat,
                Element_skill force, Element_skill dodge, Element_item weapon, Element_item armor)
            {
                Food = food;
                State = state;
                Silver = silver;
                Injury = injury;
                Inner = inner;
                Combat = combat;
                Force = force;
                Dodge = dodge;
                Weapon = weapon;
                Armor = armor;
            }
            public void SetInteraction(bool isInteractable)
            {
                //弟子装备UI
                Weapon.SetInteraction(isInteractable);
                Armor.SetInteraction(isInteractable);
                //弟子属性UI
                Food.SetInteraction(isInteractable);
                State.SetInteraction(isInteractable);
                Silver.SetInteraction(isInteractable);
                Injury.SetInteraction(isInteractable);
                Inner.SetInteraction(isInteractable);
            }

            #region element_con

            public void SetConValue(Conditions con, int value, int max)
            {
                var conUi = GetConditionUi(con);
                conUi.SetValue(value, max);
            }
            public void SetConTitle(Conditions con, string title)
            {
                var conUi = GetConditionUi(con);
                conUi.SetTitle(title);
            }

            public void SetColor(Conditions con, Color color) => GetConditionUi(con).SetColor(color);

            private Element_con GetConditionUi(Conditions con) =>
                con switch
                {
                    Conditions.Food => Food,
                    Conditions.State => State,
                    Conditions.Silver => Silver,
                    Conditions.Injury => Injury,
                    Conditions.Inner => Inner,
                    _ => throw new ArgumentOutOfRangeException(nameof(con), con, null)
                };

            #endregion

            #region element_skill

            public void SetSkill(Skills skill, string skillName, int level)
            {
                var skillUi = GetSkillUi(skill);
                skillUi.Set(skillName, level);
            }

            private Element_skill GetSkillUi(Skills skill)
            {
                var skillUi = skill switch
                {
                    Skills.Combat => Combat,
                    Skills.Force => Force,
                    Skills.Dodge => Dodge,
                    _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
                };
                return skillUi;
            }

            #endregion

            #region element_item

            public void ClearItem(Items item) => ClearItem(GetItemUi(item));
            public void SetItem(Items item, string weaponName, int grade) => SetItem(GetItemUi(item), weaponName, grade);

            private Element_item GetItemUi(Items item) => item switch
            {
                Items.Weapon => Weapon,
                Items.Armor => Armor,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };

            private void SetItem(Element_item item, string title, int grade)
            {
                item.SetTitle(title, grade);
                item.SetEmpty(false);
            }


            private void ClearItem(Element_item item) => item.SetEmpty(true);

            public void SetConsumeValue(Conditions con, int capable)
            {
                var conText = GetConditionUi(con);
                conText.SetConsume(capable);
            }

            #endregion
        }
    }
}
