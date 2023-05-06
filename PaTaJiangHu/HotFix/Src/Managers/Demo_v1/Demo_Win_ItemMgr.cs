using System;
using System.Collections.Generic;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_Win_ItemMgr : UiManagerBase
    {
        private Win_Item ItemWindow { get; set; }
        private DiziController DiziController { get; set; }
        private DiziAdvController DiziAdvController { get; set; }
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
        protected override string ViewName => "demo_win_item";
        protected override bool IsDynamicPixel => true;
        private Demo_v1Agent Agent { get; }
        public Demo_Win_ItemMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            DiziController = Game.Controllers.Get<DiziController>();
            DiziAdvController = Game.Controllers.Get<DiziAdvController>();
            Agent = uiAgent;
        }
        protected override void Build(IView view)
        {
            ItemWindow = new Win_Item(view,
                (guid, index, itemType, slot) =>
                {
                    if (itemType == 2)
                        DiziAdvController.SetDiziAdvItem(guid, index, slot);
                    else
                        DiziController.DiziEquip(guid, index, itemType);
                },
                (guid, index, itemType, slot) =>
                {
                    if (itemType == 2)
                        DiziAdvController.RemoveDiziAdvItem(guid, index, slot);
                    else
                        DiziController.DiziUnEquipItem(guid, itemType);
                }, Hide);
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_EquipmentManagement, bag =>
            {
                var guid = bag.Get<string>(0);
                var itemType = bag.GetInt(1);
                Set(guid, itemType, 0);
            });
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag =>
            {
                ItemWindow.UpdateItemList();
                Game.MainUi.HideWindows();
            });
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag =>
            {
                ItemWindow.UpdateItemList();
            });
        }
        public void Set(string diziGuid, int itemType, int slot)
        {
            ItemWindow.Set(diziGuid, itemType, slot);
            Game.MainUi.ShowWindow(ItemWindow.View);
        }
        public override void Show() => ItemWindow.Display(true);
        public override void Hide()
        {
            ItemWindow.Display(false);
            Agent.HideWindows();
        }

        private class Win_Item : UiBase
        {
            private enum ItemTypes
            {
                Weapon,
                Armor,
            }
            private ScrollRect Scroll_item { get; }
            private ListViewUi<Prefab_Item> ItemView { get; }
            private Button Btn_unequip { get; }
            private Button Btn_equip { get; }
            private Button Btn_x { get; }
            public Win_Item(IView v, 
                Action<string, int, int, int> onEquip, 
                Action<string, int, int, int> onUnequip,
                Action onCloseAction) : base(v, false)
            {
                Scroll_item = v.GetObject<ScrollRect>("scroll_item");
                ItemView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_item);
                Btn_unequip = v.GetObject<Button>("btn_unequip");
                Btn_unequip.OnClickAdd(() =>
                {
                    if (!IsDiziEquipped) return;
                    onUnequip?.Invoke(SelectedDiziGuid, SelectedItemIndex, SelectedType, SelectedSlot);
                    ListItems((ItemTypes)SelectedType);
                });
                Btn_equip = v.GetObject<Button>("btn_equip");
                Btn_equip.OnClickAdd(() =>
                {
                    if (SelectedItemIndex < 0) return;
                    onEquip?.Invoke(SelectedDiziGuid, SelectedItemIndex, SelectedType, SelectedSlot);
                    ListItems((ItemTypes)SelectedType);
                });
                Btn_x = v.GetObject<Button>("btn_x");
                Btn_x.OnClickAdd(onCloseAction);
            }
            private string SelectedDiziGuid { get; set; }
            private int SelectedItemIndex { get; set; }
            private int SelectedType { get; set; }
            private int SelectedSlot { get; set; }
            private bool IsDiziEquipped { get; set; }
            public void UpdateItemList()
            {
                var type = (ItemTypes)SelectedType;
                ListItems(type);
                for(int i = 0; i < ItemView.List.Count; i++)
                {
                    var ui = ItemView.List[i];
                    ui.SetEquipped(ui.ItemIndex == -1);
                }
            }
            public void Set(string diziGuid, int itemType, int slot)
            {
                SelectedDiziGuid = diziGuid;
                SelectedType = itemType;
                SelectedSlot = slot;
                var item = (ItemTypes)itemType;
                ListItems(item);
            }
            public void SetInteraction(bool isInteractable)
            {

            }
            
            private void ListItems(ItemTypes type)
            {
                var faction = Game.World.Faction;
                var selectedDizi = Game.World.Faction.GetDizi(SelectedDiziGuid);
                var items = new List<(string name, string info, int factionIndex, int amount, int grade)>();
                IsDiziEquipped = false;
                switch (type)
                {
                    case ItemTypes.Weapon:
                        IsDiziEquipped = selectedDizi.Weapon != null;
                        if(IsDiziEquipped) items.Add((selectedDizi.Weapon.Name, selectedDizi.Weapon.About, -1, 1, (int) selectedDizi.Weapon.Grade));
                        for(var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, item.About, i, 1, (int)item.Grade));
                        }
                        break;
                    case ItemTypes.Armor:
                        IsDiziEquipped = selectedDizi.Armor != null;
                        if (IsDiziEquipped) items.Add((selectedDizi.Armor.Name, selectedDizi.Armor.About, -1, 1, (int)selectedDizi.Armor.Grade));
                        for(var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, item.About, i , 1, (int)item.Grade));
                        }
                        break;
                    //case ItemTypes.AdvItems:
                    //    var advitems = faction.GetAllSupportedAdvItems();
                    //    var diziAdvItem = selectedDizi.AdvItems[SelectedSlot];
                    //    IsDiziEquipped = diziAdvItem != null;
                    //    if (IsDiziEquipped) items.Add((diziAdvItem.Item.Name, -1, 1, 0));
                    //    for(var i = 0; i < advitems.Length; i++)
                    //    {
                    //        var item = advitems[i];
                    //        items.Add((item.Item.Name, i, item.Amount, 0));
                    //    }
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                ItemView.ClearList(ui => ui.Display(false));
                for (var i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    var index = i;
                    var ui = ItemView.Instance(v => new Prefab_Item(v,
                        () => OnSelectedItem(index), item.factionIndex));
                    ui.SetEquipped(IsDiziEquipped && index == 0);
                    ui.SetText(item.name, item.amount, item.info ,item.grade);
                }
                SelectedItemIndex = -1;
                OnSelectedItem(SelectedItemIndex);
                Btn_unequip.interactable = SelectedItemIndex >= 0;
            }
            private void OnSelectedItem(int index)
            {
                for (var i = 0; i <ItemView.List.Count; i++)
                {
                    var ui = ItemView.List[i];
                    var isSelected = i == index;
                    ui.SetSelected(isSelected);
                    if(isSelected)
                    {
                        SelectedItemIndex = ui.ItemIndex;
                    }
                }
                Btn_equip.interactable = SelectedItemIndex >= 0;
                Btn_unequip.interactable = SelectedItemIndex < 0;
            }

            private class Prefab_Item : UiBase
            {
                private Image Img_select { get; }
                private Image Img_equipped { get; }
                private Button Btn_item { get; }
                private Image Img_ico { get; }
                private Text Text_title { get; }
                private Text Text_value { get; }
                private Text Text_info { get; }
                public int ItemIndex { get; }

                public Prefab_Item(IView v, Action onClickAction, int itemIndex) : base(v, true)
                {
                    ItemIndex = itemIndex;
                    Img_equipped = v.GetObject<Image>("img_equipped");
                    Img_select = v.GetObject<Image>("img_select");
                    Btn_item = v.GetObject<Button>("btn_item");
                    Btn_item.OnClickAdd(onClickAction);
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                    Text_info = v.GetObject<Text>("text_info");
                    SetSelected(false);
                }
                public void SetIcon(Sprite ico, Sprite selected)
                {
                    Img_ico.sprite = ico;
                    Img_select.sprite = selected;
                }

                public void SetText(string name, int amount, string info, int grade)
                {
                    Text_title.text = name;
                    Text_title.color = Game.GetColorFromItemGrade(grade);
                    Text_value.text = amount.ToString();
                    Text_value.gameObject.SetActive(amount > 1);
                    Text_info.text = info;
                    Text_info.gameObject.SetActive(info != null);
                }
                public void SetSelected(bool isSelected) => Img_select.gameObject.SetActive(isSelected);
                public void SetEquipped(bool isEqquipped) => Img_equipped.gameObject.SetActive(isEqquipped);
            }
        }
    }
}
