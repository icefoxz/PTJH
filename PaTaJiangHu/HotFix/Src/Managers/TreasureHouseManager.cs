using _GameClient.Models;
using HotFix_Project.Views.Bases;
using Server.Configs._script.Adventures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers;

public class TreasureHouseManager
{
    private View_treasureHouse TreasureHouse { get; set; }
    public void Init()
    {
        InitUis();
        RegEvents();
    }

    private void RegEvents()
    {

    }

    private void InitUis()
    {
        Game.UiBuilder.Build("view_treasureHouse", v =>
        {
            Game.MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        });
    }

    private class View_treasureHouse : UiBase
    {
        private View_selectedItem SelectedItem { get; }
        private View_contentList ContentList { get; }
        public View_treasureHouse(IView v, Action<int> TreasureEquipmentView, Action<int> TreasureMedicineView) : base(v.GameObject, true)
        {
            SelectedItem = new View_selectedItem(v.GetObject<View>("view_selectedItem"));
            ContentList = new View_contentList(v.GetObject<View>("view_contentList"), TreasureEquipmentView, TreasureMedicineView);
        }

        private class View_selectedItem : UiBase
        {
            private Image Img_item { get; }
            private Text Text_title { get; }
            private Text Text_info { get; }
            public View_selectedItem(IView v) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_title = v.GetObject<Text>("text_title");
                Text_info = v.GetObject<Text>("text_info");
            }
            public void SetIcon(Sprite img)
            {
                Img_item.sprite = img;
            }
            public void SetText(string title, string info)
            {
                Text_title.text = title;
                Text_info.text = info;
            }
        }

        private class View_contentList : UiBase
        {
            private enum TreasureTypes
            {
                Equipment,
                Medicine
            }
            private ScrollRect Scroll_item { get; }
            private ListViewUi<Prefab_Item>  Prefab_item { get; }
            private Button Btn_equipment { get; }
            private Button Btn_medicine { get; }
            public View_contentList(IView v, Action<int> TreasureEquipmentList, Action<int> TreasureMedicineList) : base(v.GameObject, false)

            {
                Scroll_item = v.GetObject<ScrollRect>("scroll_item");
                Prefab_item = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_item.content.gameObject);
                Btn_equipment = v.GetObject<Button>("btn_euipment");
                Btn_equipment.OnClickAdd(() =>
                {
                    TreasureEquipmentList?.Invoke(SelectedTreasureType);
                    ListTreasure((TreasureTypes)SelectedTreasureType);
                });
                Btn_medicine = v.GetObject<Button>("btn_medicine");
                Btn_medicine.OnClickAdd(() =>
                {
                    TreasureMedicineList?.Invoke(SelectedTreasureType);
                    ListTreasure((TreasureTypes)SelectedTreasureType);
                });
            }
            private int SelectedTreasureType { get; set; }

            private void ListTreasure(TreasureTypes type)
            {
                var faction = Game.World.Faction;
                var items = new List<(string name, int factionIndex)>();
                switch (type)
                {
                    case TreasureTypes.Equipment:
                        for (var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, i));
                        }
                        break;
                    //case TreasureTypes.Medicine:
                    //    for(var i = 0; i < faction.Medicine.Count; i++)
                    //    {
                    //        var item = faction.Medicine[i];
                    //        items.Add((item.Name, i));
                    //    }
                    //    break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
                
            }
            private class Prefab_Item : UiBase
            {
                private Image Img_item { get; }
                private Text Text_diziName { get; }
                private Text Text_title { get; }
                public int ItemIndex { get; }

                public Prefab_Item(IView v, int itemIndex) : base(v.GameObject, true)
                {
                    ItemIndex = itemIndex;

                    Img_item = v.GetObject<Image>("img_item");
                    Text_diziName = v.GetObject<Text>("text_diziName");
                    Text_title = v.GetObject<Text>("text_title");
                }
                public void SetIcon(Sprite img)
                {
                    Img_item.sprite = img;
                }
                public void SetText(string name, string title)
                {
                    Text_diziName.text = name;
                    Text_title.text = title;
                }
            }
        }
    }
}
