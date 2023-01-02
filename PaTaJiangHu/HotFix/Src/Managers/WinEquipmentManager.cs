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

namespace HotFix_Project.Src.Managers;

public class WinEquipmentManager
{
    private View_winEquipment WinEquipment { get; set; }

    public void Init()
    {
        InitUi();
        RegEvents();
    }

    private void RegEvents()
    {
        
    }

    private void InitUi()
    {
        
    }

    private class View_winEquipment : UiBase
    {
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private Prefab Prefab_item { get; }
        private Button Btn_unequip { get; }
        private Button Btn_equip { get; }
        public View_winEquipment(IView v, Action OnUnequip, Action OnEquip) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Btn_x.OnClickAdd(() =>
            {
                Display(false);
            });
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            Prefab_item = new Prefab(v.GetObject<View>("prefab_item"));
            Btn_unequip = v.GetObject<Button>("btn_unequip");
            Btn_unequip.OnClickAdd(() =>
            {
                OnUnequip?.Invoke();
            });
            Btn_equip = v.GetObject<Button>("btn_equip");
            Btn_equip.OnClickAdd(() =>
            {
                OnEquip?.Invoke();
            });
        }

        private class Prefab : UiBase
        {
            private Image Img_selected { get; }
            private Image Img_item { get; }
            private Image Img_equipped { get; }
            private Text Text_name { get; }
            public Prefab(IView v) : base(v.GameObject, true)
            {
                Img_selected = v.GetObject<Image>("img_selected");
                Img_item = v.GetObject<Image>("img_item");
                Img_equipped = v.GetObject<Image>("img_equipped");
                Text_name = v.GetObject<Text>("text_name");
            }
            public void SetIcon(Sprite selected, Sprite item, Sprite equipped)
            {
                Img_selected.sprite = selected;
                Img_item.sprite = item;
                Img_equipped.sprite = equipped;
            }
            public void SetName(string name)
            {
                Text_name.text = name;
            }
        }
    }
}
