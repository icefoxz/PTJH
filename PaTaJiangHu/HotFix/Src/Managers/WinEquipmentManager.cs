using System;
using System.Collections.Generic;
using BattleM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class WinEquipmentManager
{
    private View_winEquipment WinEquipment { get; set; }
    private DiziController DiziController { get; set; }
    private DiziAdvController DiziAdvController { get; set; }
    public void Init()
    {
        DiziController = Game.Controllers.Get<DiziController>();
        Game.UiBuilder.Build("view_winEquipment", v =>
        {
            WinEquipment = new View_winEquipment(v,
                (guid, index, itemType) => 
                {
                    if(itemType == 2)
                        DiziAdvController.SetDiziAdvItem(guid, index, itemType);
                    else
                        DiziController.DiziEquip(guid, index, itemType);
                },
                (guid, itemType) => DiziController.DiziUnEquipItem(guid, itemType));
            Game.MainUi.SetWindow(v, resetPos: true);
        }, RegEvents);
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
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => 
        {
            WinEquipment.OnItemEquipped();
            Game.MainUi.HideWindows();
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag =>
        {
            WinEquipment.OnItemUnequipped();
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_SlotManagement, bag =>
        {
            var guid = bag.Get<string>(0);
            WinEquipment.Set(guid, 2); //advitems
            Game.MainUi.ShowWindow(WinEquipment.View);
        });
    }
    
    private class View_winEquipment : UiBase
    {
        private enum ItemTypes
        {
            Weapon,
            Armor,
            AdvItems
        }
        private Button Btn_x { get; }
        private ScrollRect Scroll_items { get; }
        private Button Btn_unequip { get; }
        private Button Btn_equip { get; }
        private Text Text_drop { get; }
        private Text Text_use { get; }
        private Text Text_unequip { get; }
        private Text Text_equip { get; }
        private ListViewUi<Prefab_Item> ItemView { get; }

        public View_winEquipment(IView v, 
            Action<string,int,int> onEquip,
            Action<string,int> onUnequip)
            : base(v.GameObject, false)
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
            Text_drop = v.GetObject<Text>("text_drop");
            Text_use = v.GetObject<Text>("text_use");
            Text_unequip = v.GetObject<Text>("text_unequip");
            Text_equip = v.GetObject<Text>("text_euip");
        }

        public void OnItemEquipped()
        {
            ListItems((ItemTypes)SelectedType);
            for (var index = 0; index < ItemView.List.Count; index++)
            {
                var ui = ItemView.List[index];
                ui.SetEquipped(ui.ItemIndex == -1);
            }
        }

        public void OnItemUnequipped()
        {
            ListItems((ItemTypes)SelectedType);
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

        public void SetEquipmentText(bool isEquipment)
        {
            Text_equip.gameObject.SetActive(isEquipment);
            Text_unequip.gameObject.SetActive(isEquipment);
            Text_drop.gameObject.SetActive(!isEquipment);
            Text_use.gameObject.SetActive(!isEquipment);
        }

        private void ListItems(ItemTypes type)
        {
            var faction = Game.World.Faction;
            var selectedDizi = Game.World.Faction.GetDizi(SelectedDiziGuid);
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
                        SetEquipmentText(true);
                    }
                    break;
                case ItemTypes.Armor:
                    IsDiziEquipped = selectedDizi.Armor != null;
                    if (IsDiziEquipped) items.Add((selectedDizi.Armor.Name, -1));
                    for (var i = 0; i < faction.Armors.Count; i++)
                    {
                        var item = faction.Armors[i];
                        items.Add((item.Name, i));
                        SetEquipmentText(true);
                    }
                    break;
                case ItemTypes.AdvItems:
                    var advitems = faction.GetAllSupportedAdvItems();
                    for (var i = 0; i < advitems.Length; i++)
                    {
                        var item = advitems[i];
                        items.Add((item.Name, i));
                        SetEquipmentText(false);
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
            SelectedItemIndex = -1;
            OnSelectedItem(SelectedItemIndex);
            Btn_unequip.interactable = SelectedItemIndex >= 0;
        }

        private void OnSelectedItem(int index)
        {
            for (var i = 0; i < ItemView.List.Count; i++)
            {
                var ui = ItemView.List[i];
                var isSelected = i == index;
                ui.SetSelected(isSelected);
                if (isSelected)
                { 
                    SelectedItemIndex = ui.ItemIndex;
                }
            }
            Btn_equip.interactable = SelectedItemIndex >= 0;
            Btn_unequip.interactable = SelectedItemIndex < 0;
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
    }
}
