using System;
using System.Collections.Generic;
using BattleM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
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
        DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        Game.UiBuilder.Build("view_winEquipment", v =>
        {
            WinEquipment = new View_winEquipment(v,
                (guid, index, itemType) =>
                {
                    if (itemType == 2)
                        DiziAdvController.SetDiziAdvItem(guid, index);
                    else
                        DiziController.DiziEquip(guid, index, itemType);
                },
                (guid, index, itemType) => 
                {
                    if (itemType == 2)
                        XDebug.LogWarning($"卸下弟子历炼道具(未完成)，slot{index}");
                    else
                        DiziController.DiziUnEquipItem(guid, itemType);

                    
                });
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
            WinEquipment.UpdateItemList();
            Game.MainUi.HideWindows();
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag =>
        {
            WinEquipment.UpdateItemList();
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_SlotManagement, bag =>
        {
            var guid = bag.Get<string>(0);
            WinEquipment.Set(guid, 2); //advitems
            Game.MainUi.ShowWindow(WinEquipment.View);
        });
        Game.MessagingManager.RegEvent(EventString.Faction_AdvItemsUpdate, bag =>
        {
            WinEquipment.UpdateItemList();
            Game.MainUi.HideWindows();
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
            Action<string,int,int> onUnequip)
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
                onUnequip?.Invoke(SelectedDiziGuid, SelectedItemIndex, SelectedType);
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

        public void UpdateItemList()
        {
            var type = (ItemTypes)SelectedType;
            ListItems(type);
            for (int i = 0; i < ItemView.List.Count; i++)
            {
                var ui = ItemView.List[i];
                ui.SetEquipped(ui.ItemIndex == -1);
            }
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
            var items = new List<(string name,int factionIndex, int amount)>();
            IsDiziEquipped = false;
            switch (type)
            {
                case ItemTypes.Weapon:
                    IsDiziEquipped = selectedDizi.Weapon != null;
                    if (IsDiziEquipped) items.Add((selectedDizi.Weapon.Name, -1, 1));
                    for (var i = 0; i < faction.Weapons.Count; i++)
                    {
                        var item = faction.Weapons[i];
                        items.Add((item.Name, i, 1));
                        SetEquipmentText(true);
                    }
                    break;
                case ItemTypes.Armor:
                    IsDiziEquipped = selectedDizi.Armor != null;
                    if (IsDiziEquipped) items.Add((selectedDizi.Armor.Name, -1, 1));
                    for (var i = 0; i < faction.Armors.Count; i++)
                    {
                        var item = faction.Armors[i];
                        items.Add((item.Name, i, 1));
                        SetEquipmentText(true);
                    }
                    break;
                case ItemTypes.AdvItems:
                    var advitems = faction.GetAllSupportedAdvItems();
                    for(var i = 0; i < advitems.Length; i++)
                    {
                        IsDiziEquipped = selectedDizi.AdvItems[i] != null;
                        if (IsDiziEquipped) items.Add((selectedDizi.AdvItems[i].Item.Name, -1, 1));
                    }
                    for (var i = 0; i < advitems.Length; i++)
                    {
                        var item = advitems[i].Item.Name;
                        var itemAmount = advitems[i].Amount;
                        items.Add((item, i, itemAmount));
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
                ui.SetText(item.name, item.amount);
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
            private Text Text_amount { get; }
            private Button SelectBtn { get; }
            public int ItemIndex { get; }

            public Prefab_Item(IView v, Action onclickAction, int itemIndex) : base(v.GameObject, true)
            {
                ItemIndex = itemIndex;
                
                Img_selected = v.GetObject<Image>("img_selected");
                Img_item = v.GetObject<Image>("img_item");
                Img_equipped = v.GetObject<Image>("img_equipped");
                Text_name = v.GetObject<Text>("text_name");
                Text_amount = v.GetObject<Text>("text_amount");
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
            public void SetText(string name, int amount)
            {
                Text_name.text = name;
                Text_amount.text = amount.ToString();
                Text_amount.gameObject.SetActive(amount > 1);
            }

            public void SetEquipped(bool isEquipped)
            {
                Img_equipped.gameObject.SetActive(isEquipped);
            }
        }
    }
}
