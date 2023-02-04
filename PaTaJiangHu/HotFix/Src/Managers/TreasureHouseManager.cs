using System;
using System.Collections.Generic;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

internal class TreasureHouseManager : UiManagerBase
{
    private View_treasureHouse TreasureHouse { get; set; }

    protected override UiManager.Sections Section { get; } = UiManager.Sections.Page;
    protected override string ViewName { get; } = "view_treasureHouse";
    protected override bool IsFixPixel { get; } = true;

    public TreasureHouseManager(UiManager uiManager) : base(uiManager)
    {
    }
    protected override void Build(IView view)
    {
        TreasureHouse = new View_treasureHouse(view);
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Page_TreasureHouse, bag =>
        {
            UiManager.Show(this);
        });
    }

    public override void Show() => TreasureHouse.Display(true);

    public override void Hide() => TreasureHouse.Display(false);

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
            private void ListItems((string title, string diziName, int amount)[]items)
            {
                ItemView.ClearList(item => item.Destroy());
                for (int i = 0; i < items.Length; i++)
                {
                    var item = items[i];
                    var index = i;
                    var ui = ItemView.Instance(v => new Prefab_Item(v, () => OnItemSelected(index)));
                    ui.Set(item.diziName, item.title, item.amount);
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
                var items = new List<(string name, string diziName, int amount)>();
                switch (type)
                {
                    case TreasureTypes.Equipment: //武器+防具=装备
                        foreach (var dizi in faction.DiziList)
                        {
                            if (dizi.Weapon != null)
                                items.Add((dizi.Weapon.Name, dizi.Name, 1));
                            if (dizi.Armor != null)
                                items.Add((dizi.Armor.Name, dizi.Name, 1));
                        }
                        for (var i = 0; i < faction.Weapons.Count; i++)
                        {
                            var item = faction.Weapons[i];
                            items.Add((item.Name, string.Empty,  1));
                        }
                        for (var i = 0; i < faction.Armors.Count; i++)
                        {
                            var item = faction.Armors[i];
                            items.Add((item.Name, string.Empty, 1));
                        }
                        break;
                    case TreasureTypes.Medicine: //药膳
                        var medicine = faction.GetAllMedicines();
                        for(var i = 0; i < medicine.Length; i++)
                        {
                            var item = medicine[i];
                            items.Add((item.med.Name, string.Empty, 1));
                        }
                        break;
                    case TreasureTypes.Adventure: 
                        for (var i = 0; i < faction.AdvProps.Count; i++)
                        {
                            var item = faction.AdvProps;
                            items.Add(("", string.Empty, 1));
                        }
                        break;
                    case TreasureTypes.Reward: 
                        for (var i = 0; i < faction.Packages.Count; i++)
                        {
                            var item = faction.Packages;
                            items.Add(("package", string.Empty, 1));
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
                private Button Btn_item { get; }
                private Image Img_selected { get; }
                private Text Text_amount { get; }

                public Prefab_Item(IView v, Action onClickAction) : base(v.GameObject, true)
                {
                    Img_item = v.GetObject<Image>("img_item");
                    Text_diziName = v.GetObject<Text>("text_diziName");
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_item = v.GetObject<Button>("btn_item");
                    Btn_item.OnClickAdd(onClickAction);
                    Img_selected = v.GetObject<Image>("img_selected");
                    Text_amount = v.GetObject<Text>("text_amount");
                    SetSelected(false);
                }
                public void SetSelected(bool selected)
                {
                    Img_selected.gameObject.SetActive(selected);
                }
                public void SetIcon(Sprite img)
                {
                    Img_item.sprite = img;
                }
                public void Set(string name, string title, int amount)
                {
                    Text_diziName.text = name;
                    Text_diziName.gameObject.SetActive(!string.IsNullOrWhiteSpace(name));
                    Text_amount.text = amount.ToString();
                    Text_amount.gameObject.SetActive(amount > 1);
                    Text_title.text = title;
                }
            }
        }

    }
}
