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
        Game.UiBuilder.Build("view_treasureHouse", v =>
        {
            TreasureHouse = new View_treasureHouse(v);
            Game.MainUi.SetMid(v, true);
        }, RegEvents);
    }

    private void RegEvents()
    {

    }

    private class View_treasureHouse : UiBase
    {
        private View_selectedItem SelectedItem { get; }
        private View_contentList ContentList { get; }
        public View_treasureHouse(IView v) : base(v.GameObject, false)
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
            private enum TreasureTypes { Equipment, Medicine, Adventure, Reward }
            private ScrollRect Scroll_item { get; }
            private ListViewUi<Prefab_Item>  ItemView { get; }
            private Button Btn_equipment { get; }
            private Button Btn_medicine { get; }
            private Button Btn_advItem { get; }
            private Button Btn_reward { get; }

            private event Action<int> OnSelectedAction;
            
            public View_contentList(IView v, Action<int> onSelectedAction) : base(v.GameObject, true)
            {
                OnSelectedAction = onSelectedAction;
                Scroll_item = v.GetObject<ScrollRect>("scroll_item");
                ItemView = new ListViewUi<Prefab_Item>(v.GetObject<View>("prefab_item"), Scroll_item);
                Btn_equipment = v.GetObject<Button>("btn_euipment");
                Btn_equipment.OnClickAdd(() => SortItems(TreasureTypes.Equipment));
                Btn_medicine = v.GetObject<Button>("btn_medicine");
                Btn_medicine.OnClickAdd(() => SortItems(TreasureTypes.Medicine));
                Btn_advItem = v.GetObject<Button>("btn_advItem");
                Btn_advItem.OnClickAdd(() => SortItems(TreasureTypes.Adventure));
                Btn_reward = v.GetObject<Button>("btn_reward");
                Btn_reward.OnClickAdd(() => SortItems(TreasureTypes.Reward));
            }
            private void ListItems((string title, string diziName)[]items)
            {
                ItemView.ClearList(item => item.Destroy());
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var index = i;
                    var ui = ItemView.Instance(v => new Prefab_Item(v));
                    ui.SetText(item.diziName, item.title);
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
                    case TreasureTypes.Equipment: //武器+防具=装备
                        for (var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, string.Empty));
                        }
                        for (var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, string.Empty));
                        }
                        break;
                    case TreasureTypes.Medicine: //暂时用防具数据
                        for(var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, string.Empty));
                        }
                        break;
                    case TreasureTypes.Adventure: //暂时用防具数据
                        for (var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, string.Empty));
                        }
                        break;
                    case TreasureTypes.Reward: //暂时用防具数据
                        for (var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, string.Empty));
                        }
                        break;
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
                //private Button Btn_Selected { get; }
                //private Image Img_selected { get; }

                public Prefab_Item(IView v) : base(v.GameObject, true)
                {
                    Img_item = v.GetObject<Image>("img_item");
                    Text_diziName = v.GetObject<Text>("text_diziName");
                    Text_title = v.GetObject<Text>("text_title");
                    //Btn_Selected = v.GetObject<Button>("btn_selected");
                    //Btn_Selected.OnClickAdd(onSelectedAction);
                    //Img_selected = v.GetObject<Image>("img_selected");
                }
                public void SetSelected(bool selected)
                {
                    //Img_selected.gameObject.SetActive(selected);
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
