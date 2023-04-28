using System;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

internal class Demo_Win_ItemSelector : UiManagerBase
{
    private Win_itemSelector win_itemSelector { get; set; }
    public Demo_Win_ItemSelector(MainUiAgent uiAgent) : base(uiAgent)
    {
    }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
    protected override string ViewName => "demo_win_itemSelector";
    protected override bool IsDynamicPixel => true;

    private event Action<int> OnConfirm;
    protected override void Build(IView view) => win_itemSelector = new Win_itemSelector(view, OnConfirm);

    protected override void RegEvents() { }

    public override void Show() => win_itemSelector.Display(true);

    public override void Hide() => win_itemSelector.Display(false);

    public void SetItems((int id, string title, Sprite icon)[] items, Action<int> onConfirmAction)
    {
        OnConfirm = onConfirmAction;
        win_itemSelector.SetItems(items, 0);
        Show();
    }

    private class Win_itemSelector : UiBase
    {
        private ListViewUi<Prefab_item> ItemList { get; }
        private Button btn_x { get; }
        private Button btn_confirm { get; }

        private int SelectedId { get; set; }

        public Win_itemSelector(IView view, Action<int> onConfirmAction) : base(view, false)
        {
            ItemList = new ListViewUi<Prefab_item>(view, "prefab_item", "scroll_item");
            btn_x = view.GetObject<Button>("btn_x");
            btn_confirm = view.GetObject<Button>("btn_confirm");
            btn_x.OnClickAdd(() => Display(false));
            btn_confirm.OnClickAdd(() => onConfirmAction?.Invoke(SelectedId));
        }

        public void SetItems((int id,string title, Sprite icon)[] items,int selectedIndex = 0)
        {
            ItemList.ClearList(ui => ui.Destroy());
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
            private Image img_icon { get; }
            private Text text_title { get; }
            private Button btn_item { get; }
            public int Id { get; }

            public Prefab_item(IView v, int id) : base(v, true)
            {
                Id = id;
                img_select = v.GetObject<Image>("img_select ");
                img_icon = v.GetObject<Image>("img_icon ");
                text_title = v.GetObject<Text>("text_title ");
                btn_item = v.GetObject<Button>("btn_item ");
            }

            public void SetItem(string title, Sprite icon,Action onClickAction)
            {
                text_title.text = title;
                img_icon.sprite = icon;
                btn_item.OnClickAdd(onClickAction);
            }

            public void SetSelected(bool selected) => img_select.gameObject.SetActive(selected);
        }
    }
}