using System;
using System.Linq;
using _GameClient.Models;
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
    internal class Demo_View_EquipmentMgr : MainPageBase
    {
        private View_Equipment Equipment { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Top;
        protected override string ViewName => "demo_view_equipment";
        protected override bool IsDynamicPixel => true;
        public Demo_View_EquipmentMgr(Demo_v1Agent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            Equipment = new View_Equipment(view,
                onItemSelection: (guid, itemType) =>
                Game.MessagingManager.SendParams(EventString.Dizi_EquipmentManagement, guid, itemType, 0)
                //MainUiAgent.Show<Demo_Win_ItemMgr>(mgr => mgr.Set(guid, itemType, 0))
                );;
        }

        protected override void RegEvents()
        {
            //Game.MessagingManager.RegEvent(EventString.Dizi_AdvManagement, bag =>
            //{
            //    Equipment.Set(bag);
            //});
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => Equipment.Update(bag.Get<string>(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => Equipment.Update(bag.Get<string>(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag => Equipment.Update(bag.GetString(0)));
        }
        public override void Show() => Equipment.Display(true);
        public override void Hide() => Equipment.Display(false);

        public void Set(Dizi dizi) => Equipment.Set(dizi.Guid);

        private class View_Equipment : UiBase
        {
            private Element WeaponElement { get; }
            private Element ArmorElement { get; }
            public View_Equipment(IView v, Action<string, int> onItemSelection) : base(v, true)
            {
                WeaponElement = new Element(v.GetObject<View>("element_weapon"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 0));
                ArmorElement = new Element(v.GetObject<View>("element_armor"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 1));
            }

            private Dizi SelectedDizi { get; set; }
            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                SetDizi(SelectedDizi);
                var dizi = SelectedDizi;
                WeaponElement.SetInteraction(dizi.Adventure == null);
                ArmorElement.SetInteraction(dizi.Adventure == null);
            }
            private void SetDizi(Dizi dizi)
            {
                SetDiziEquipment(dizi);
            }
            private void SetDiziEquipment(Dizi dizi)
            {
                if (dizi.Weapon == null) WeaponElement.ClearItem(WeaponElement);
                else WeaponElement.SetItem(WeaponElement, dizi.Weapon.Name, (int)dizi.Weapon.Grade);
                if (dizi.Armor == null) ArmorElement.ClearItem(ArmorElement);
                else ArmorElement.SetItem(ArmorElement, dizi.Weapon.Name, (int)dizi.Armor.Grade);
            }

            public void Set(string guid)
            {
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }

            private class Element : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_title { get; }
                private Button Btn_element { get; }
                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_element = v.GetObject<Button>("btn_element");
                    Btn_element.OnClickAdd(onClickAction);
                }
                public void SetImage(Sprite img)
                {
                    Img_ico.sprite = img;
                    Img_ico.gameObject.SetActive(true);
                }
                public void SetTitle(string title, int grade)
                {
                    Text_title.text = title;
                    Text_title.color = Game.GetColorFromItemGrade(grade);
                    Text_title.gameObject.SetActive(true);
                }
                public void ClearItem(Element item)
                {
                    item.SetEmpty(true);
                }

                private void SetEmpty(bool empty)
                {
                    Img_ico.gameObject.SetActive(!empty);
                    Text_title.gameObject.SetActive(!empty);
                }

                public void SetItem(Element item, string itemName, int grade)
                {
                    item.SetTitle(itemName, grade);
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_element.interactable = isInteractable;
                }
            }
        }
    }
}
