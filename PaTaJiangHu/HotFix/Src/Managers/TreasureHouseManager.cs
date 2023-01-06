using System;
using System.Collections.Generic;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

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
            TreasureHouse = new View_treasureHouse(v);
            Game.MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        });
    }

    private class View_treasureHouse : UiBase
    {
        private View_selectedItem SelectedItem { get; }
        private View_contentList ContentList { get; }
        public View_treasureHouse(IView v) : base(v.GameObject, true)
        {
            SelectedItem = new View_selectedItem(v.GetObject<View>("view_selectedItem"));
            ContentList = new View_contentList(v.GetObject<View>("view_contentList"), index => SelectedItem.SetItem(index));
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

            public void SetItem(int index)
            {
                var faction =  Game.World.Faction;
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
            private ListViewUi<Prefab_Item>  ItemView { get; }
            private Button Btn_equipment { get; }
            private Button Btn_medicine { get; }
            private event Action<int> OnSelectedAction;
            
            public View_contentList(IView v, Action<int> onSelectedAction) : base(v.GameObject, false)
            {
                OnSelectedAction = onSelectedAction;
                Scroll_item = v.GetObject<ScrollRect>("scroll_item");
                ItemView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_item);
                Btn_equipment = v.GetObject<Button>("btn_euipment");
                Btn_equipment.OnClickAdd(() => SortItems(TreasureTypes.Equipment));
                Btn_medicine = v.GetObject<Button>("btn_medicine");
                Btn_medicine.OnClickAdd(() => SortItems(TreasureTypes.Medicine));
            }
            private void ListItems((string title, string diziName)[]items)
            {
                ItemView.ClearList(item => item.Destroy());
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var index = i;
                    ItemView.Instance(v => new Prefab_Item(v, ()=> OnItemSelected(index)));
                }
            }
            private void OnItemSelected(int index)
            {
                for (int i = 0; i < ItemView.List.Count; i++)
                {
                    var item = ItemView.List[i];
                    var selected = i == index;
                    item.SetSelected(selected);
                    if (selected)
                    {
                        OnSelectedAction?.Invoke(index);
                    }
                }
            }

            private void SortItems(TreasureTypes type)
            {
                var faction = Game.World.Faction;
                var items = new List<(string name, string diziName)>();
                switch (type)
                {
                    case TreasureTypes.Equipment:
                        for (var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, string.Empty));
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
                ListItems(items.ToArray());
            }
            private class Prefab_Item : UiBase
            {
                private Image Img_item { get; }
                private Text Text_diziName { get; }
                private Text Text_title { get; }
                private Button Btn_Selected { get; }
                private Image Img_selected { get; }

                public Prefab_Item(IView v, Action onSelectedAction) : base(v.GameObject, true)
                {
                    Img_item = v.GetObject<Image>("img_item");
                    Text_diziName = v.GetObject<Text>("text_diziName");
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_Selected = v.GetObject<Button>("btn_selected");
                    Btn_Selected.OnClickAdd(onSelectedAction);
                    Img_selected = v.GetObject<Image>("img_selected");
                }
                public void SetSelected(bool selected)
                {
                    Img_selected.gameObject.SetActive(selected);
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
