using System;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Models;
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
        private Demo_v1Agent Agent { get; }
        public Demo_View_EquipmentMgr(Demo_v1Agent uiAgent) : base(uiAgent) => Agent = uiAgent;

        protected override void Build(IView view)
        {
            Equipment = new View_Equipment(view, onItemSelection: Agent.Win_EquipmentManagement);
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
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Finalize, bag => Equipment.Update(bag.GetString(0)));
        }
        public override void Show() => Equipment.Display(true);
        public override void Hide() => Equipment.Display(false);

        public void Set(Dizi dizi) => Equipment.Set(dizi.Guid);

        private class View_Equipment : UiBase
        {
            private Element element_weapon { get; }
            private Element element_armor { get; }
            private Element element_shoes { get; }
            private Element element_decoration { get; }

            public View_Equipment(IView v, Action<string, int> onItemSelection) : base(v, true)
            {
                element_weapon = new Element(v.GetObject<View>("element_weapon"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 0));
                element_armor = new Element(v.GetObject<View>("element_armor"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 1));
                element_shoes = new Element(v.GetObject<View>("element_shoes"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 2));
                element_decoration = new Element(v.GetObject<View>("element_decoration"),
                    () => onItemSelection?.Invoke(SelectedDizi?.Guid, 3));
            }

            private Dizi SelectedDizi { get; set; }
            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                UpdateDiziEquipment(SelectedDizi);
                var dizi = SelectedDizi;
                var isIdleState = dizi.State.Current == DiziStateHandler.States.Idle;
                element_weapon.SetInteraction(isIdleState);
                element_armor.SetInteraction(isIdleState);
                element_shoes.SetInteraction(isIdleState);
                element_decoration.SetInteraction(isIdleState);
            }

            private void UpdateDiziEquipment(Dizi dizi)
            {
                if (dizi.Equipment.Weapon == null) element_weapon.ClearItem(element_weapon);
                else
                    element_weapon.SetItem(element_weapon, dizi.Equipment.Weapon.Name, (int)dizi.Equipment.Weapon.Grade,
                        $"{dizi.Equipment.Weapon.Quality}品", dizi.Equipment.Weapon.Icon);
                if (dizi.Equipment.Armor == null) element_armor.ClearItem(element_armor);
                else
                    element_armor.SetItem(element_armor, dizi.Equipment.Armor.Name, (int)dizi.Equipment.Armor.Grade,
                        $"{dizi.Equipment.Armor.Quality}品", dizi.Equipment.Armor.Icon);
                if (dizi.Equipment.Shoes == null) element_shoes.ClearItem(element_shoes);
                else
                    element_shoes.SetItem(element_shoes, dizi.Equipment.Shoes.Name, (int)dizi.Equipment.Shoes.Grade,
                        $"{dizi.Equipment.Shoes.Quality}品", dizi.Equipment.Shoes.Icon);
                if (dizi.Equipment.Decoration == null) element_decoration.ClearItem(element_decoration);
                else
                    element_decoration.SetItem(element_decoration, dizi.Equipment.Decoration.Name,
                        (int)dizi.Equipment.Decoration.Grade,
                        $"{dizi.Equipment.Decoration.Quality}品", dizi.Equipment.Decoration.Icon);
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
                private Text Text_short { get; }
                private Button Btn_element { get; }
                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Text_short = v.GetObject<Text>("text_short");
                    Btn_element = v.GetObject<Button>("btn_element");
                    Btn_element.OnClickAdd(onClickAction);
                }

                private void SetImage(Sprite img)
                {
                    Img_ico.sprite = img;
                    Img_ico.gameObject.SetActive(true);
                }

                private void SetTitle(string title, int grade, string @short)
                {
                    Text_title.text = title;
                    Text_short.text = @short;
                    Text_title.color = Game.GetColorFromItemGrade(grade);
                    Text_title.gameObject.SetActive(true);
                    Text_short.gameObject.SetActive(true);
                }
                public void ClearItem(Element item)
                {
                    item.SetEmpty(true);
                    SetInteraction(false);
                }

                private void SetEmpty(bool empty)
                {
                    Img_ico.gameObject.SetActive(!empty);
                    Text_title.gameObject.SetActive(!empty);
                    Text_short.gameObject.SetActive(!empty);
                }

                public void SetItem(Element item, string itemName, int grade,string @short ,Sprite icon)
                {
                    item.SetTitle(itemName, grade, @short);
                    SetImage(icon);
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_element.interactable = isInteractable;
                }
            }
        }
    }
}
