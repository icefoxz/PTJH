using System;
using System.Collections.Generic;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_Win_ItemMgr : UiManagerBase
    {
        private Win_Item ItemWindow { get; set; }
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
        protected override string ViewName => "demo_win_item";
        protected override bool IsDynamicPixel => true;
        public Demo_Win_ItemMgr(Demo_v1Agent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            ItemWindow = new Win_Item(view,
                () => XDebug.LogWarning("弟子佩戴装备/防具/历练物品"),
                () => XDebug.LogWarning("弟子卸下装备/防具/历练物品")
                );
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_EquipmentManagement, bag =>
            {
                var guid = bag.Get<string>(0);
                var itemType = bag.GetInt(1);
                Set(guid, itemType, 0);
            });
        }
        public void Set(string diziGuid, int itemType, int slot)
        {
            ItemWindow.Set(diziGuid, itemType, slot);
            Game.MainUi.ShowWindow(ItemWindow.View);
        }
        public override void Show() => ItemWindow.Display(true);
        public override void Hide() => ItemWindow.Display(false);
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
            public Win_Item(IView v, Action onEquip, Action onUnequip) : base(v, false)
            {
                Scroll_item = v.GetObject<ScrollRect>("scroll_item");
                ItemView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_item);
                Btn_unequip = v.GetObject<Button>("btn_unequip");
                Btn_unequip.OnClickAdd(() =>
                {
                    onUnequip?.Invoke();
                    Display(false); //Temporary
                });
                Btn_equip = v.GetObject<Button>("btn_equip");
                Btn_equip.OnClickAdd(() =>
                {
                    onEquip?.Invoke();
                    Display(false); //Temporary
                });
            }
            private string SelectedDiziGuid { get; set; }
            private int SelectedType { get; set; }
            private int SelectedSlot { get; set; }
            private bool IsDiziEquipped { get; set; }
            public void UpdateItemLsit()
            {
                var type = (ItemTypes)SelectedType;
                ListItems(type);
                for(int i = 0; i < ItemView.List.Count; i++)
                {
                    var ui = ItemView.List[i];
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

            private void ListItems(ItemTypes type)
            {
                var faction = Game.World.Faction;
                var selectedDizi = Game.World.Faction.GetDizi(SelectedDiziGuid);
                var items = new List<(string name, int factionIndex, int amount, int grade)>();
                IsDiziEquipped = false;
                switch (type)
                {
                    case ItemTypes.Weapon:
                        IsDiziEquipped = selectedDizi.Weapon != null;
                        if(IsDiziEquipped) items.Add((selectedDizi.Weapon.Name, -1, 1, (int) selectedDizi.Weapon.Grade));
                        for(var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, i, 1, (int)item.Grade));
                        }
                        break;
                    case ItemTypes.Armor:
                        IsDiziEquipped = selectedDizi.Armor != null;
                        if (IsDiziEquipped) items.Add((selectedDizi.Armor.Name, -1, 1, (int)selectedDizi.Armor.Grade));
                        for(var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, i , 1, (int)item.Grade));
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
            }


            private class Prefab_Item : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_title { get; }
                private Text Text_value { get; }
                private Text Text_info { get; }
                public Prefab_Item(IView v) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                    Text_info = v.GetObject<Text>("text_info");
                }
                public void SetIcon(Sprite img) => Img_ico.sprite = img;
                public void SetText(string name, int amount, string info, int grade)
                {
                    Text_title.text = name;
                    Text_title.color = Game.GetColorFromItemGrade(grade);
                    Text_value.text = amount.ToString();
                    Text_value.gameObject.SetActive(amount > 1);
                    Text_info.text = info;
                }
            }
        }
    }
}
