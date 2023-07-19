using System;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Win_ItemSelector : WinUiManagerBase
    {
        private Win_itemSelector win_itemSelector { get; set; }

        public Demo_Win_ItemSelector(MainUiAgent uiAgent) : base(uiAgent)
        {
        }

        protected override string ViewName => "demo_win_itemSelector";

        private event Action<int> OnConfirm;

        protected override void Build(IView view) =>
            win_itemSelector = new Win_itemSelector(view, id => { OnConfirm?.Invoke(id); }, Hide);

        protected override void RegEvents()
        {
        }

        public override void Show() => win_itemSelector.Display(true);

        //public override void Hide() => win_itemSelector.Display(false);

        public void SetItems((int id, string title, Sprite icon)[] items, Action<int> onConfirmAction)
        {
            OnConfirm = onConfirmAction;
            win_itemSelector.SetItems(items);
            Show();
        }

        private class Win_itemSelector : UiBase
        {
            private ListViewUi<Prefab_item> ItemList { get; }
            private Button btn_x { get; }
            private Button btn_confirm { get; }

            private int SelectedId { get; set; }

            public Win_itemSelector(IView view, Action<int> onConfirmAction, Action onCloseAction) : base(view, false)
            {
                ItemList = new ListViewUi<Prefab_item>(view, "prefab_item", "scroll_item");
                btn_x = view.GetObject<Button>("btn_x");
                btn_confirm = view.GetObject<Button>("btn_confirm");
                btn_x.OnClickAdd(onCloseAction);
                btn_confirm.OnClickAdd(() => onConfirmAction?.Invoke(SelectedId));
            }

            public void SetItems((int id, string title, Sprite icon)[] items, int selectedIndex = 0)
            {
                ItemList.ClearList(ui => ui.Destroy());
                btn_confirm.gameObject.SetActive(items.Length > 0);
                for (int i = 0; i < items.Length; i++)
                {
                    var index = i;
                    var (id, title, icon) = items[i];
                    var ui = ItemList.Instance(v => new Prefab_item(v, id));
                    ui.SetItem(title, icon, () => OnItemSelected(index));
                }

                OnItemSelected(selectedIndex);
            }

            private void OnItemSelected(int index)
            {
                for (var i = 0; i < ItemList.List.Count; i++)
                {
                    var ui = ItemList.List[i];
                    var selected = i == index;
                    ui.SetSelected(selected);
                    if (selected) SelectedId = ui.Id;
                }
            }

            private class Prefab_item : UiBase
            {
                private Image img_select { get; }
                private Image img_ico { get; }
                private Text text_title { get; }
                private Button btn_item { get; }
                public int Id { get; }

                public Prefab_item(IView v, int id) : base(v, true)
                {
                    Id = id;
                    img_select = v.GetObject<Image>("img_select");
                    img_ico = v.GetObject<Image>("img_ico");
                    text_title = v.GetObject<Text>("text_title");
                    btn_item = v.GetObject<Button>("btn_item");
                }

                public void SetItem(string title, Sprite icon, Action onClickAction)
                {
                    text_title.text = title;
                    img_ico.sprite = icon;
                    btn_item.OnClickAdd(onClickAction);
                }

                public void SetSelected(bool selected) => img_select.gameObject.SetActive(selected);
            }
        }
    }
}