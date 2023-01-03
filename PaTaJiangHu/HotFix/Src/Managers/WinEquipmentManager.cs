using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using HotFix_Project.Views.Bases;
using Server.Configs._script.Factions;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class WinEquipmentManager
{
    private View_winEquipment WinEquipment { get; set; }
    private DiziController DiziController { get; set; }

    public void Init()
    {
        DiziController = Game.Controllers.Get<DiziController>();
        InitUis();
        RegEvents();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_EquipmentManagement, bag =>
        {
            var guid = bag.Get<string>(0);
            var itemType = bag.GetInt(1);
            WinEquipment.Set(guid, itemType); //weapon or armor
            Game.MainUi.ShowWindow(WinEquipment.View);
        });
    }

    private void InitUis()
    {
        Game.UiBuilder.Build("view_winEquipment", v =>
        {
            WinEquipment = new View_winEquipment(v, (guid, index, itemType) =>
            {
                DiziController.DiziEquip(guid, index, itemType);
            }, (guid, itemType) => DiziController.DiziUnEquipItem(guid, itemType));
            Game.MainUi.SetWindow(v, resetPos: true);
        });
    }

    private class View_winEquipment : UiBase
    {
        private enum ItemTypes
        {
            Weapon,
            Armor
        }
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private Button Btn_unequip { get; }
        private Button Btn_equip { get; }

        private ListViewUi<Prefab_Item> ItemView { get; }

        public View_winEquipment(IView v, Action<string,int,int> onEquip, Action<string,int> onUnequip) : base(v.GameObject, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Btn_x.OnClickAdd(() =>
            {
                Display(false);
            });
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            ItemView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_items.content.gameObject);
            Btn_unequip = v.GetObject<Button>("btn_unequip");
            Btn_unequip.OnClickAdd(() => onUnequip?.Invoke(SelectedDizi, SelectedType));
            Btn_equip = v.GetObject<Button>("btn_equip");
            Btn_equip.OnClickAdd(() => onEquip?.Invoke(SelectedDizi, SelectedIndex, SelectedType));
        }

        private string SelectedDizi { get; set; }
        private int SelectedIndex { get; set; }
        private int SelectedType { get; set; }

        public void Set(string guid, int itemType)
        {
            SelectedDizi = guid;
            SelectedType = itemType;
            var item = (ItemTypes)itemType;
            ListItems(item);
        }


        private void ListItems(ItemTypes type)
        {
            var faction = Game.World.Faction;
            var items = type switch
            {
                ItemTypes.Weapon => faction.GetAllWeapons(),
                ItemTypes.Armor => faction.GetAllArmors(),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            ItemView.ClearList(ui => ui.Display(false));
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var index = i;
                var ui = ItemView.Instance(v => new Prefab_Item(v, () => OnSelectedItem(index)));
                ui.SetName(item.ItemName);
            }
        }

        private void OnSelectedItem(int index)
        {
            for (var i = 0; i < ItemView.List.Count; i++)
            {
                var ui = ItemView.List[i];
                ui.SetSelected(i == index);
                SelectedIndex = index;
            }
        }

        private class Prefab_Item : UiBase
        {
            private Image Img_selected { get; }
            private Image Img_item { get; }
            private Image Img_equipped { get; }
            private Text Text_name { get; }
            private Button SelectBtn { get; }

            public Prefab_Item(IView v,Action onclickAction) : base(v.GameObject, true)
            {
                Img_selected = v.GetObject<Image>("img_selected");
                Img_item = v.GetObject<Image>("img_item");
                Img_equipped = v.GetObject<Image>("img_equipped");
                Text_name = v.GetObject<Text>("text_name");
                SelectBtn = v.GameObject.GetComponent<Button>();
                SelectBtn.OnClickAdd(onclickAction);
                SetDefault();
                SetSelected(false);
            }

            private void SetDefault()
            {
                
            }

            public void SetSelected(bool isSelected) => Img_selected.gameObject.SetActive(isSelected);
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
