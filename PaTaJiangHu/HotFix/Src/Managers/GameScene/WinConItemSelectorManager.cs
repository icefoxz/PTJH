using System;
using System.Collections.Generic;
using _GameClient.Models;
using BattleM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class WinConItemSelectorManager : UiManagerBase
{
    private View_winConItemSelector WinConItemSelector { get; set; }
    private DiziController DiziController { get; set; }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
    protected override string ViewName => "view_winConItemSelector";
    protected override bool IsDynamicPixel => true;

    public WinConItemSelectorManager(GameSceneAgent uiAgent) : base(uiAgent)
    {
        DiziController = Game.Controllers.Get<DiziController>();
    }

    protected override void Build(IView view)
    {
        WinConItemSelector = new View_winConItemSelector(view,
            guid =>
            {
                DiziController.UseSilver(guid, 10);
                Game.MessagingManager.Send(EventString.Dizi_ConditionUpdate, guid); //Test direct update in this window
            },
            (guid, index) =>
            {
                DiziController.UseMedicine(guid, index);
                Game.MessagingManager.SendParams(EventString.Dizi_ConditionUpdate, guid, index); //Test direct update in this window
            });
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_ConditionManagement,
            bag =>
            {
                Game.MainUi.ShowWindow(WinConItemSelector.View);
                WinConItemSelector.Set(bag.Get<string>(0));
            });
        Game.MessagingManager.RegEvent(EventString.Dizi_Params_StaminaUpdate,
            bag => WinConItemSelector.StaminaUpdate(bag.Get<string>(0)));
        Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate,
            bag => WinConItemSelector.DiziUpdate(bag.Get<string>(0)));
    }

    public void Set(string guid) => WinConItemSelector.Set(guid);

    public override void Show() => WinConItemSelector.Display(true);

    public override void Hide() => WinConItemSelector.Display(false);

    private class View_winConItemSelector : UiBase
    {
        public enum Conditions { Food, State, Silver, Injury, Inner }
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private View_Stamina StaminaView { get; }
        private View_Btns BtnsView { get; }
        private ListViewUi<Prefab_Item> ItemListView { get; }
        private ElementManager ElementMgr { get; }
        public View_winConItemSelector(IView v, Action<string> onSilverApply, Action<string,int> onUse) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Btn_x.OnClickAdd(() =>
            {
                Display(false);
            });
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            BtnsView = new View_Btns(v.GetObject<View>("view_btns"), () => onSilverApply(SelectedDizi.Guid),
                () => onUse(SelectedDizi.Guid, SelectedItemIndex));
            ItemListView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_items);
            Btn_x.OnClickAdd(() => { Display(false); });
            ElementMgr = new ElementManager(
                new Element_con(v.GetObject<View>("element_conFood")),
                new Element_con(v.GetObject<View>("element_conState")),
                new Element_con(v.GetObject<View>("element_conSilver")),
                new Element_con(v.GetObject<View>("element_conInjury")),
                new Element_con(v.GetObject<View>("element_conInner"))
            );
        }

        private Dizi SelectedDizi { get; set; }
        private int SelectedItemIndex { get; set; }

        private void FactionItemsUpdate()
        {
            ItemListView.ClearList(v => v.Destroy());
            var faction = Game.World.Faction;
            var items = new List<(string name, int amount, int index)>();
            var tuples = faction.GetAllMedicines();
            for (var index = 0; index < tuples.Length; index++)
            {
                var (med, amount) = tuples[index];
                items.Add((med.Name, amount, index));
            }

            foreach (var (name, amount, index) in items)
            {
                ItemListView.Instance(v =>
                {
                    var ui = new Prefab_Item(v,index, () => SetSelectedItem(index));
                    ui.Set(name, amount);
                    return ui;
                });
            }
            SelectedItemIndex = -1;
            UpdateBtnsView();
        }

        private void SetSelectedItem(int index)
        {
            SelectedItemIndex = index;
            foreach (var ui in ItemListView.List) 
                ui.SetSelected(index == ui.Index);
            UpdateBtnsView();
        }

        private void UpdateBtnsView()
        {
            BtnsView.SetSilverInteractable(SelectedDizi.Silver.ValueMaxRatio < 1);
            BtnsView.SetUseInteractable(SelectedItemIndex >= 0);
        }

        public void Set(string guid)
        {
            Display(true);
            SelectedDizi = Game.World.Faction.GetDizi(guid);
            DiziUpdate(guid);
        }

        public void DiziUpdate(string guid)
        {
            if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
            DiziElementsUpdate(SelectedDizi);
            StaminaUpdate(guid);
            FactionItemsUpdate();
        }

        public void StaminaUpdate(string guid)
        {
            if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
            var (stamina, max, min, sec) = SelectedDizi.Stamina.GetStaminaValue();
            StaminaView.Set(stamina, max);
            StaminaView.SetTime(min, sec);
        }

        private void DiziElementsUpdate(Dizi dizi)
        {
            var controller = Game.Controllers.Get<DiziController>();
            var (foodText, fColor) = controller.GetFoodCfg(dizi.Food.ValueMaxRatio);
            var (emoText, eColor) = controller.GetEmotionCfg(dizi.Emotion.ValueMaxRatio);
            var (silverText, sColor) = controller.GetSilverCfg(dizi.Silver.ValueMaxRatio);
            var (injuryText, jColor) = controller.GetInjuryCfg(dizi.Injury.ValueMaxRatio);
            var (innerText, nColor) = controller.GetInnerCfg(dizi.Inner.ValueMaxRatio);
            SetElement(Conditions.Food, dizi.Food, foodText, fColor);
            SetElement(Conditions.State, dizi.Emotion, emoText, eColor);
            SetElement(Conditions.Silver, dizi.Silver, silverText, sColor);
            SetElement(Conditions.Injury, dizi.Injury, injuryText, jColor);
            SetElement(Conditions.Inner, dizi.Inner, innerText, nColor);

            void SetElement(Conditions co, IConditionValue con, string title, Color color)
            {
                ElementMgr.SetConValue(co, con.Value, con.Max);
                ElementMgr.SetConTitle(co, title);
                ElementMgr.SetColor(co, color);
            }
        }

        private class Prefab_Item : UiBase
        {
            private Image Img_item { get; }
            private Text Text_amount { get; }
            private Text Text_name { get; }
            private Image Img_selected{ get; }
            private Button Button { get; }
            public int Index { get; private set; }

            public Prefab_Item(IView v, int index, Action onClickAction) : base(v.GameObject, true)
            {
                Index = index;
                Img_item = v.GetObject<Image>("img_item");
                Text_amount = v.GetObject<Text>("text_amount");
                Text_name = v.GetObject<Text>("text_name");
                Img_selected = v.GetObject<Image>("img_selected");
                Button = v.GameObject.GetComponent<Button>();
                Button.OnClickAdd(onClickAction);
                SetSelected(false);
            }
            public void SetImage(Sprite img)=> Img_item.sprite = img;
            public void Set(string name, int num)
            {
                Text_name.text = name;
                Text_amount.text = num.ToString();
            }

            public void SetSelected(bool selected) => Img_selected.gameObject.SetActive(selected);
        }

        private class View_Stamina : UiBase
        {
            private View_timeView TimeView { get; }
            private Text Text_staminaValue { get; }
            private Text Text_staminaMax { get; }
            public View_Stamina(IView v) : base(v.GameObject, true)
            {
                TimeView = new View_timeView(v.GetObject<View>("view_timeView"));
                Text_staminaValue = v.GetObject<Text>("text_staminaValue");
                Text_staminaMax = v.GetObject<Text>("text_staminaMax");
            }

            public void SetTime(int min, int sec) => TimeView.SetTime(min, sec);

            public void Set(int value, int max)
            {
                Text_staminaValue.text = value.ToString();
                Text_staminaMax.text = max.ToString();
            }

            private class View_timeView : UiBase
            {
                private Text Text_value {  get; }
                private Text Text_max { get; }
                private GameObject Go_timeView { get; }
                public View_timeView(IView v) : base(v.GameObject, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                    Go_timeView = v.GetObject("go_timeView");
                }
                public void SetTime(int min, int sec)
                {
                    var isFull = min <= 0 && sec <= 0;
                    Go_timeView.SetActive(!isFull);
                    Text_value.text = min.ToString();
                    Text_max.text = sec.ToString();
                }
            }
        }

        private class View_Btns : UiBase
        {
            private Button Btn_silver { get; }
            private Button Btn_use { get; }
            private Text Text_silver { get; }
            public View_Btns(IView v, Action onSilverApply, Action onUse) : base(v.GameObject, true)
            {
                Btn_silver = v.GetObject<Button>("btn_silver");
                Btn_silver.OnClickAdd(onSilverApply);
                Btn_use = v.GetObject<Button>("btn_use");
                Btn_use.OnClickAdd(onUse);
                Text_silver = v.GetObject<Text>("text_silver");
            }

            public void SetUseInteractable(bool enable) => Btn_use.interactable = enable;
            public void SetSilverInteractable(bool enable) => Btn_silver.interactable = enable;
            public void SetText(int cost) => Text_silver.text = cost.ToString();
        }

        private class ElementManager
        {
            private Element_con Food { get; }
            private Element_con State { get; }
            private Element_con Silver { get; }
            private Element_con Injury { get; }
            private Element_con Inner { get; }
            public ElementManager(Element_con food, Element_con state, Element_con silver, Element_con injury,
                Element_con inner)
            {
                Food = food;
                State = state;
                Silver = silver;
                Injury = injury;
                Inner = inner;
            }
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

            internal void SetColor(Conditions con, Color color) => GetConditionUi(con).SetColor(color);
        }

        private class Element_con : UiBase
        {
            private Scrollbar Scrbar_condition { get; }
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Text Text_title { get; }
            private Image HandleImg { get; }
            private Image BgImg { get; }

            public Element_con(IView v) : base(v.GameObject, true)
            {
                Scrbar_condition = v.GetObject<Scrollbar>("scrbar_condition");
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Text_title = v.GetObject<Text>("text_title");
                BgImg = Scrbar_condition.GetComponent<Image>();
                HandleImg = Scrbar_condition.image;
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
        }
    }
}
