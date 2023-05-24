using System;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class WinRewardManager : UiManagerBase
{
    private View_winReward WinReward { get; set; }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
    protected override string ViewName => "view_winReward";
    protected override bool IsDynamicPixel => false;

    public WinRewardManager(GameSceneAgent uiAgent) : base(uiAgent)
    {
    }

    protected override void Build(IView view)
    {
        WinReward = new View_winReward(view, () => Game.MainUi.HideWindows());
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Rewards_Prompt, bag =>
        {
            WinReward.ShowContainer();
            Game.MainUi.ShowWindow(WinReward.View);
        });
    }

    public override void Show() => WinReward.Display(true);

    public override void Hide() => WinReward.Display(false);


    private class View_winReward : UiBase
    {
        private Button Btn_x { get; }
        private Button Btn_confirm { get; }
        private ListViewUi<Prefab_item> ItemView { get; }

        public View_winReward(IView v, Action onCloseAction) : base(v, false)
        {
            Btn_x = v.GetObject<Button>("btn_x");
            Btn_confirm = v.GetObject<Button>("btn_confirm");
            ItemView = new ListViewUi<Prefab_item>(v, "prefab_item", "scroll_items");
            Btn_x.OnClickAdd(() =>
            {
                onCloseAction?.Invoke();
                Display(false);
            });
            Btn_confirm.OnClickAdd(() =>
            {
                onCloseAction?.Invoke();
                Display(false);
            });
        }

        public void ShowContainer()
        {
            ItemView.ClearList(ui => ui.Destroy());
            foreach (var reward in Game.World.RewardBoard.Rewards)
            {
                foreach (var item in reward.AllItems) 
                    InstancePrefab().Set(item.Item.Name, item.Amount);

                foreach (var package in reward.Packages) 
                    InstancePrefab().Set($"{package.Grade}阶包裹", 1);
            }
            Display(true);
            Prefab_item InstancePrefab() => ItemView.Instance(v => new Prefab_item(v, true));
        }


        private class Prefab_item : UiBase
        {
            private Image Img_item { get; }
            private Text Text_name { get; }
            private Text Text_value { get; }
            public Prefab_item(IView v, bool display) : base(v, display)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_name = v.GetObject<Text>("text_name");
                Text_value = v.GetObject<Text>("text_value");
            }

            public void Set(string title, int amount)
            {
                var amtText = amount > 1 ? amount.ToString() : string.Empty;
                Text_name.text = title;
                Text_value.text = amtText;
            }
        }
    }
}