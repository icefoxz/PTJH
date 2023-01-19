using System;
using System.Collections;
using HotFix_Project.Views.Bases;
using _GameClient.Models;
using HotFix_Project.Serialization;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;
using HotFix_Project.Serialization.LitJson;
using Server.Configs.Items;
using Server.Controllers;

namespace HotFix_Project.Managers;

public class WinConItemSelectorManager
{
    private View_winConItemSelector WinConItemSelector { get; set; }
    private DiziController DiziController { get; set; }

    public void Init()
    {
        DiziController = Game.Controllers.Get<DiziController>();
        Game.UiBuilder.Build("view_winConItemSelector", v =>
        {
            WinConItemSelector = new View_winConItemSelector(v,
                () => XDebug.Log("10硬币已派发！"),
                () => XDebug.Log("物品已使用！"));
            Game.MainUi.SetWindow(v, resetPos: true);
        },RegEvents);
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_ConditionManagement,
            bag =>
            {
                Game.MainUi.ShowWindow(WinConItemSelector.View);
                WinConItemSelector.Set(bag.Get<string>(0));
            });
    }

    private class View_winConItemSelector : UiBase
    {
        public enum Conditions { Food, State, Silver, Injury, Inner }
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private View_Stamina StaminaView { get; }
        private View_Btns BtnsView { get; }
        private ListViewUi<Prefab_Item> ItemListView { get; }
        private ElementManager ElementMgr { get; }
        public View_winConItemSelector(IView v, Action onSilverApply, Action onUse) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            BtnsView = new View_Btns(v.GetObject<View>("view_btns"), onSilverApply, onUse);
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

        public void UpdateItems((string name,int amount)[] items)
        {
            ItemListView.ClearList(v => v.Destroy());
            foreach (var (name, amount) in items)
            {
                ItemListView.Instance(v =>
                {
                    var ui = new Prefab_Item(v);
                    ui.Set(name, amount);
                    return ui;
                });
            }
        }

        public void Set(string guid)
        {
            Display(true);
            //var dizi = bag.Get<DiziDto>(0);
            SelectedDizi = Game.World.Faction.GetDizi(guid);
            UpdateDiziStamina();
            View.StopAllCo();
            View.StartCo(SetTimeEverySec());

            IEnumerator SetTimeEverySec()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    UpdateDiziStamina();
                }
            }
        }
        private void UpdateDiziStamina()
        {
            var (stamina, max, min, sec) = SelectedDizi.Stamina.GetStaminaValue();
            StaminaView.Set(stamina, max);
            StaminaView.SetTime(min, sec);
        }
        public void Update()
        {
            SetDizi(SelectedDizi);
        }
        private void SetDizi(Dizi dizi)
        {
            SetDiziElements(dizi);
        }
        private void SetDiziElements(Dizi dizi)
        {
            ElementMgr.SetConValue(Conditions.Food, dizi.Food.Value, dizi.Food.Max);
            ElementMgr.SetConValue(Conditions.State, dizi.Emotion.Value, dizi.Emotion.Max);
            ElementMgr.SetConValue(Conditions.Silver, dizi.Silver.Value, dizi.Silver.Max);
            //ElementMgr.SetConValue(Conditions.Injury, dizi.Injury.Value, dizi.Injury.Max);
            //ElementMgr.SetConValue(Conditions.Silver, dizi.Inner.Value, dizi.Inner.Max);
        }

        private class Prefab_Item : UiBase
        {
            private Image Img_item { get; }
            private Text Text_amount { get; }
            private Text Text_name { get; }

            public Prefab_Item(IView v) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_amount = v.GetObject<Text>("text_amount");
                Text_name = v.GetObject<Text>("text_name");
            }
            public void SetImage(Sprite img)=> Img_item.sprite = img;
            public void Set(string name, int num)
            {
                Text_name.text = name;
                Text_amount.text = num.ToString();
            }
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
        }

        private class Element_con : UiBase
        {
            private Scrollbar Scrbar_condition { get; }
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Text Text_title { get; }
            public Element_con(IView v) : base(v.GameObject, true)
            {
                Scrbar_condition = v.GetObject<Scrollbar>("scrbar_condition");
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Text_title = v.GetObject<Text>("text_title");
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
            
        }
    }
}
