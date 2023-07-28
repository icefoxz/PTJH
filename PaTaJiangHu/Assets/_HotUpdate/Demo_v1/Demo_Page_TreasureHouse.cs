using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Page_TreasureHouse : PageUiManagerBase
    {
        private enum Types
        {
            Temp,
            Equipment,
            Books,
            Packages,
            Misc,
        }
        private View_itemDesk view_itemDesk { get; set; }
        private View_itemListview ViewItemListview { get; set; }
        private FactionController FactionController => Game.Controllers.Get<FactionController>();
        private Faction Faction => Game.World.Faction;

        private Types SelectedTypes { get; set; }
        private int SelectedIndex { get; set; }
        private bool IsDisplay { get; set; }

        public Demo_Page_TreasureHouse(MainUiAgent uiAgent) : base(uiAgent)
        {
        }

        protected override string ViewName => "demo_page_treasureHouse";
        protected override void Build(IView v)
        {
            view_itemDesk = new View_itemDesk(v.GetObject<View>("view_itemDesk"), OnDisposeItem);
            ViewItemListview = new View_itemListview(v.GetObject<View>("view_itemListview"), index =>
            {
                SelectedIndex = index;
                AutoSelectItemToDesk(false);
            }, SetTypeRefreshList);
            v.Hide();
        }

        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_Equipment_Update, _ => RefreshCurrentElement(true));
            Game.MessagingManager.RegEvent(EventString.Faction_FunctionItem_Update, _ => RefreshCurrentElement(true));
            Game.MessagingManager.RegEvent(EventString.Faction_TempItem_Update, _ => RefreshCurrentElement(true));
            Game.MessagingManager.RegEvent(EventString.Faction_Book_Update, _ => RefreshCurrentElement(true));
            Game.MessagingManager.RegEvent(EventString.Faction_Package_Update, _ => RefreshCurrentElement(true));
        }

        private void SetTypeRefreshList(Types type)
        {
            if (SelectedTypes == type) return;
            SelectedTypes = type;
            ViewItemListview.SetSideFilterSelected(type);
            RefreshCurrentElement(true);
        }

        private (Sprite icon,string name)[] GetItemsFromFaction(Types type)
        {
            return type switch
            {
                Types.Temp => Faction.TempItems.Select(t => (t.Icon, t.Name)).ToArray(),
                Types.Equipment => Faction.Equipments.Select(e => (e.Icon, e.Name)).ToArray(),
                Types.Books => Faction.Books.Select(b => (b.Icon, b.Name)).ToArray(),
                Types.Packages => Faction.Packages.Select(ResolvePackage).ToArray(),
                Types.Misc => Faction.FuncItems.Select(i => (i.Icon, i.Name)).ToArray(),
                _ => throw new ArgumentOutOfRangeException()
            };

            (Sprite icon, string name) ResolvePackage(IItemPackage package) => 
                (null, $"包裹[{package.Grade}]");
        }

        // 设置选中物品并显示信息
        private void SetItemToDesk(int index)
        {
            SelectedIndex = index;

            // Empty
            if (!TryToSelectItemFrom(index))
            {
                SetEmptyAndSelectNone();
                return;
            }

            // Package
            if (SelectedTypes == Types.Packages)
            {
                view_itemDesk.SetItem(null, "包裹", "这是一个未打开的包裹.");// todo: 包裹的图标, 名称, 描述 或许根据品阶设置
                view_itemDesk.SetAction("打开", () =>
                {
                    FactionController.OpenPackage(SelectedIndex);// 打开包裹
                    ViewItemListview.SetList(GetItemsFromFaction(Types.Packages));// 刷新包裹列表
                    SetItemToDesk(0);
                });
                view_itemDesk.EnableDispose(false);// 包裹不可以丢弃
                return;
            }

            //由于上面判断了index是否有效, 所以下面的代码不会出现index越界的情况
            //并且下面不会刷新列表
            // GameItem
            var item = SelectedTypes switch
            {
                Types.Temp => Faction.TempItems[SelectedIndex],
                Types.Equipment => Faction.Equipments[SelectedIndex],
                Types.Books => Faction.Books[SelectedIndex],
                Types.Misc => Faction.FuncItems[SelectedIndex],
                _ => throw new ArgumentOutOfRangeException()
            };
            view_itemDesk.SetItem(item.Icon, item.Name, item.About);// 设置物品
            view_itemDesk.EnableDispose(true);// 可以丢弃
            ViewItemListview.SetPrefabSelected(SelectedIndex);// 设置选中
        }

        // 设置空物品并取消选中
        private void SetEmptyAndSelectNone()
        {
            view_itemDesk.SetEmpty(); // 置空
            view_itemDesk.EnableDispose(false); // 不可丢弃
            ViewItemListview.SetPrefabSelected(-1); // 取消选中
            ViewItemListview.SetList(GetItemsFromFaction(SelectedTypes)); // 刷新列表
        }

        // 检查index是否有效
        private bool TryToSelectItemFrom(int index)
        {
            return SelectedTypes switch
            {
                Types.Temp => TryIndex(Faction.TempItems, index),
                Types.Equipment => TryIndex(Faction.Equipments, index),
                Types.Books => TryIndex(Faction.Books, index),
                Types.Packages => TryIndex(Faction.Packages, index),
                Types.Misc => TryIndex(Faction.FuncItems, index),
                _ => throw new ArgumentOutOfRangeException()
            };

            bool TryIndex<T>(IReadOnlyCollection<T> items, int i) => i >= 0 && i < items.Count;
        }

        // 当丢弃物品时的方法
        private void OnDisposeItem()
        {
            if (SelectedTypes == Types.Packages) throw new InvalidOperationException("包裹不可以丢弃!");

            var item = SelectedTypes switch
            {
                Types.Temp => Faction.TempItems[SelectedIndex],
                Types.Equipment => Faction.Equipments[SelectedIndex],
                Types.Books => Faction.Books[SelectedIndex],
                Types.Misc => Faction.FuncItems[SelectedIndex],
                _ => throw new ArgumentOutOfRangeException()
            };
            FactionController.DisposeItem(item);
            SetItemToDesk(0);
        }

        public override void Show()
        {
            base.Show();
            IsDisplay = true;
            RefreshCurrentElement(true);
        }

        // 刷新列表
        private void RefreshCurrentElement(bool forceRefreshList)
        {
            if (!IsDisplay) return;
            AutoSelectItemToDesk(forceRefreshList);
        }

        // 自动选择物品
        private void AutoSelectItemToDesk(bool forceRefreshList)
        {
            if (forceRefreshList || !TryToSelectItemFrom(SelectedIndex))//如果当前index不合法需要刷新列表
            {
                SelectedIndex = 0;
                ViewItemListview.SetList(GetItemsFromFaction(SelectedTypes));
            }
            SetItemToDesk(SelectedIndex);
        }

        public override void Hide()
        {
            base.Hide();
            IsDisplay = false;
        }

        private class View_itemDesk : UiBase
        {
            private Image img_icon { get; }
            private Text text_name { get; }
            private View_action view_action { get; }
            private Button btn_dispose { get; }
            private Text text_info { get; }

            public View_itemDesk(IView v,Action disposeAction) : base(v, true)
            {
                img_icon = v.GetObject<Image>("img_icon");
                text_name = v.GetObject<Text>("text_name");
                view_action = new View_action(v.GetObject<View>("view_action"), false);
                btn_dispose = v.GetObject<Button>("btn_dispose");
                text_info = v.GetObject<Text>("text_info");
                btn_dispose.OnClickAdd(disposeAction);
            }

            public void SetItem(Sprite icon, string name, string info)
            {
                SetIcon(icon);
                text_name.text = name;
                text_info.text = info;
                view_action.Display(false);
            }

            private void SetIcon(Sprite icon)
            {
                img_icon.sprite = icon;
                img_icon.gameObject.SetActive(img_icon.sprite);
            }

            public void SetEmpty()
            {
                SetIcon(null);
                text_name.text = string.Empty;
                text_info.text = string.Empty;
                view_action.Display(false);
            }

            public void SetAction(string text, Action action)
            {
                view_action.SetAction(text, action);
                view_action.Display(true);
            }

            public void EnableDispose(bool enable) => btn_dispose.gameObject.SetActive(enable);

            private class View_action : UiBase
            {
                private Button btn_action { get; }
                private Text text_action { get; }
                public View_action(IView v, bool display) : base(v, display)
                {
                    btn_action = v.GetObject<Button>("btn_action");
                    text_action = v.GetObject<Text>("text_action");
                }

                public void SetAction(string text, Action action)
                {
                    text_action.text = text;
                    btn_action.OnClickAdd(action);
                }
            }

        }

        private class View_itemListview : UiBase
        {
            private ListViewUi<Prefab_item> ItemView { get; }
            private Element_filter element_filterTemp { get; }
            private Element_filter element_filterEquipments { get; }
            private Element_filter element_filterBooks { get; }
            private Element_filter element_filterPackages { get; }
            private Element_filter element_filterMisc { get; }
            private event Action <int> OnItemSelected;

            public View_itemListview(IView v, Action<int> onIndexSelectedAction, Action<Types> onTypeSelectedAction) :
                base(v, true)
            {
                element_filterTemp = new Element_filter(v.GetObject<View>("element_filterTemp"), () => onTypeSelectedAction(Types.Temp));
                element_filterEquipments = new Element_filter(v.GetObject<View>("element_filterEquipments"), () => onTypeSelectedAction(Types.Equipment));
                element_filterBooks = new Element_filter(v.GetObject<View>("element_filterBooks"), () => onTypeSelectedAction(Types.Books));
                element_filterPackages = new Element_filter(v.GetObject<View>("element_filterPackages"), () => onTypeSelectedAction(Types.Packages));
                element_filterMisc = new Element_filter(v.GetObject<View>("element_filterMisc"), () => onTypeSelectedAction(Types.Misc));
                ItemView = new ListViewUi<Prefab_item>(v, "prefab_item", "scr_item");
                OnItemSelected = onIndexSelectedAction;
            }

            public void SetSideFilterSelected(Types type)
            {
                element_filterTemp.SetSelected(type == Types.Temp);
                element_filterEquipments.SetSelected(type == Types.Equipment);
                element_filterBooks.SetSelected(type == Types.Books);
                element_filterPackages.SetSelected(type == Types.Packages);
                element_filterMisc.SetSelected(type == Types.Misc);
            }

            public void SetList((Sprite icon,string name)[] list)
            {
                ItemView.ClearList(ui => ui.Destroy());
                for (var i = 0; i < list.Length; i++)
                {
                    var index = i;
                    var (icon, name) = list[i];
                    var ui = ItemView.Instance(v => new Prefab_item(v, () => OnItemSelected?.Invoke(index)));
                    ui.Set(icon, name);
                }
            }

            public void SetPrefabSelected(int index)
            {
                for (var i = 0; i < ItemView.List.Count; i++)
                {
                    var item = ItemView.List[i];
                    item.SetSelected(i == index);
                }
            }

            private class Element_filter : UiBase
            {
                private Text text_label { get; }
                private Image img_selected { get; }
                private Button btn_action { get; }
                private View_minMax view_minMax { get; }
                
                public Element_filter(IView v, Action onclickAction) : base(v, true)
                {
                    text_label = v.GetObject<Text>("text_label");
                    img_selected = v.GetObject<Image>("img_selected");
                    btn_action = v.GetObject<Button>("btn_action");
                    view_minMax = new View_minMax(v.GetObject<View>("view_minMax"), false);
                    btn_action.OnClickAdd(onclickAction);
                }

                public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);

                private void SetMinMax(int min, int max)
                {
                    view_minMax.Set(min, max);
                    view_minMax.Display(true);
                }

                private class View_minMax : UiBase
                {
                    private Text text_value { get; }
                    private Text text_max { get; }
                    public View_minMax(IView v, bool display) : base(v, display)
                    {
                        text_value = v.GetObject<Text>("text_value");
                        text_max = v.GetObject<Text>("text_max");
                    }

                    public void Set(int min, int max)
                    {
                        text_value.text = min.ToString();
                        text_max.text = max.ToString();
                    }
                }
            }
            private class Prefab_item : UiBase
            {
                private Image img_selected { get; }
                private Image img_icon { get; }
                private Text text_name { get; }
                private Button btn_item { get; }

                public Prefab_item(IView v, Action onClickAction) : base(v, true)
                {
                    img_selected = v.GetObject<Image>("img_selected");
                    img_icon = v.GetObject<Image>("img_icon");
                    text_name = v.GetObject<Text>("text_name");
                    btn_item = v.GetObject<Button>("btn_item");
                    btn_item.OnClickAdd(onClickAction);
                }

                public void Set(Sprite icon, string name)
                {
                    img_icon.sprite = icon;
                    img_icon.gameObject.SetActive(img_icon.sprite);
                    text_name.text = name;
                }

                public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);
            }
        }
    }
}