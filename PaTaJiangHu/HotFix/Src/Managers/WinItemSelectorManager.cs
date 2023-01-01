using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Visual.BaseUis;

namespace HotFix_Project.Managers;

public class WinItemSelectorManager
{
    private View_winItemSelector WinItemSelector { get; set; }
    private class View_winItemSelector : UiBase
    {
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private Prefab Prefab_item { get; }
        private View_Stamina StaminaView { get; }
        private View_Btns BtnsView { get; }
        public View_winItemSelector(IView v) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Btn_x.OnClickAdd(() =>
            {
                Display(false);
            });
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            Prefab_item = new Prefab(v.GetObject<View>("prefab_item"));
            StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            BtnsView = new View_Btns(v.GetObject<View>("view_btns"));
        }

        private class Prefab : UiBase
        {
            private Image Img_item { get; }
            private Text Text_amount { get; }
            private Text Text_name { get; }

            public Prefab(IView v) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_amount = v.GetObject<Text>("text_amount");
                Text_name = v.GetObject<Text>("text_name");
            }
            public void SetImage(Sprite img)=> Img_item.sprite = img;
            public void Set(int num, string name)
            {
                Text_amount.text = num.ToString();
                Text_name.text = name;
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

            public void Set(int value, int max)
            {
                Text_staminaValue.text = value.ToString();
                Text_staminaMax.text = max.ToString();
            }

            private class View_timeView : UiBase
            {
                private Text Text_value {  get; }
                private Text Text_max { get; }
                public View_timeView(IView v) : base(v.GameObject, true)
                {
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                }
                public void SetTimer(int min, int sec)
                {
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
