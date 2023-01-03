using System;
using System.Collections;
using HotFix_Project.Views.Bases;
using _GameClient.Models;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers;

public class WinConItemSelectorManager
{
    private View_winConItemSelector WinConItemSelector { get; set; }

    public void Init()
    {
        InitUi();
        RegEvents();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_winConItemSelector", v =>
        {
            WinConItemSelector = new View_winConItemSelector(v, () =>
            {
                Game.MainUi.HideWindow();
                WinConItemSelector.Display(false);
            });
            Game.MainUi.SetWindow(v, true);
        });
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
        private Button Btn_x { get; }
        private View_Stamina StaminaView { get; }
        private View_Btns BtnsView { get; }

        private ListViewUi<Prefab_Item> ItemListView { get; }
        public View_winConItemSelector(IView v,Action onCloseAction) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            var itemScroll = v.GetObject<ScrollRect>("scroll_items");
            StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            BtnsView = new View_Btns(v.GetObject<View>("view_btns"));
            ItemListView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), itemScroll.content.gameObject);
            Btn_x.OnClickAdd(onCloseAction);
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
            SelectedDizi = Game.World.Faction.DiziMap[guid];
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
            public View_Btns(IView v/*, Action onSilverApply, Action onUse*/) : base(v.GameObject, true)
            {
                Btn_silver = v.GetObject<Button>("btn_silver");
                Btn_silver.OnClickAdd(() =>
                {
                    ///onSilverApply?.Invoke();
                });
                Btn_use = v.GetObject<Button>("btn_use");
                Btn_use.OnClickAdd(() =>
                {
                    ///onUse?.Invoke();
                    ///Display(false);
                });
                Text_silver = v.GetObject<Text>("text_silver");
            }
            public void SetText(int cost) => Text_silver.text = cost.ToString();
        }
    }
}
