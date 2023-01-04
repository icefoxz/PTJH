﻿using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using BattleM;
using HotFix_Project.Models.Charators;
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
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => WinEquipment.OnItemEquipped());
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => WinEquipment.OnItemUnequipped());
    }

    private void InitUis()
    {
        Game.UiBuilder.Build("view_winEquipment", v =>
        {
            WinEquipment = new View_winEquipment(v,
                (guid, index, itemType)=>DiziController.DiziEquip(guid,index,itemType),
                (guid, itemType) => DiziController.DiziUnEquipItem(guid, itemType));
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
            Btn_unequip.OnClickAdd(() =>
            {
                if (!IsDiziEquipped) return;
                onUnequip?.Invoke(SelectedDiziGuid, SelectedType);
                ListItems((ItemTypes)SelectedType);
            });
            Btn_equip = v.GetObject<Button>("btn_equip");
            Btn_equip.OnClickAdd(() =>
            {
                if (SelectedItemIndex < 0) return;
                onEquip?.Invoke(SelectedDiziGuid, SelectedItemIndex, SelectedType);
                ListItems((ItemTypes)SelectedType);
            });//装备按键
        }

        public void OnItemEquipped()
        {
            for (var index = 0; index < ItemView.List.Count; index++)
            {
                var ui = ItemView.List[index];
                ui.SetEquipped(SelectedItemIndex == index);
            }
        }

        public void OnItemUnequipped()
        {
            foreach (var ui in ItemView.List) ui.SetEquipped(false);
        }

        private string SelectedDiziGuid { get; set; }
        private int SelectedItemIndex { get; set; }
        private int SelectedType { get; set; }
        private bool IsDiziEquipped { get; set; }

        public void Set(string guid, int itemType)
        {
            SelectedDiziGuid = guid;
            SelectedType = itemType;
            var item = (ItemTypes)itemType;
            ListItems(item);
        }


        private void ListItems(ItemTypes type)
        {
            var faction = Game.World.Faction;
            var selectedDizi = Game.World.Faction.DiziMap[SelectedDiziGuid];
            var items = new List<(string name,int factionIndex)>();
            IsDiziEquipped = false;
            switch (type)
            {
                case ItemTypes.Weapon:
                    IsDiziEquipped = selectedDizi.Weapon != null;
                    if (IsDiziEquipped) items.Add((selectedDizi.Weapon.Name, -1));
                    for (var i = 0; i < faction.Weapons.Count; i++)
                    {
                        var item = faction.Weapons[i];
                        items.Add((item.Name, i));
                    }
                    break;
                case ItemTypes.Armor:
                    IsDiziEquipped = selectedDizi.Armor != null;
                    if (IsDiziEquipped) items.Add((selectedDizi.Armor.Name, -1));
                    for (var i = 0; i < faction.Armors.Count; i++)
                    {
                        var item = faction.Armors[i];
                        items.Add((item.Name, i));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            ItemView.ClearList(ui => ui.Display(false));
            for (var i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var index = i;
                var ui = ItemView.Instance(v => new Prefab_Item(v, () => OnSelectedItem(index), item.factionIndex));
                ui.SetEquipped(IsDiziEquipped && index == 0);
                ui.SetName(item.name);
            }
        }

        private void OnSelectedItem(int index)
        {
            for (var i = 0; i < ItemView.List.Count; i++)
            {
                var ui = ItemView.List[i];
                ui.SetSelected(i == index);
                SelectedItemIndex = ui.ItemIndex;
            }
        }

        private class Prefab_Item : UiBase
        {
            private Image Img_selected { get; }
            private Image Img_item { get; }
            private Image Img_equipped { get; }
            private Text Text_name { get; }
            private Button SelectBtn { get; }
            public int ItemIndex { get; }

            public Prefab_Item(IView v, Action onclickAction, int itemIndex) : base(v.GameObject, true)
            {
                ItemIndex = itemIndex;
                
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

            public void SetEquipped(bool isEquipped)
            {
                Img_equipped.gameObject.SetActive(isEquipped);
            }
        }
        
        public class ItemDto
        {
            public string ItemName { get; set; }
            public ItemDto()
            {

            }
            public ItemDto(IWeapon weapon)
            {
                ItemName = weapon.Name;
            }

            public ItemDto(IArmor armor)
            {
                ItemName = armor.Name;
            }
        }
    }
}
