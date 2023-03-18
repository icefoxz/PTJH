using System;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_Win_RewardMgr : UiManagerBase
    {
        private Win_Reward RewardWindow { get; set; } 
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
        protected override string ViewName => "demo_win_reward";
        protected override bool IsDynamicPixel => true;
        public Demo_Win_RewardMgr(Demo_v1Agent uiAgent) : base(uiAgent) { }
        protected override void Build(IView view)
        {
            RewardWindow = new Win_Reward(view, () => Game.MainUi.HideWindows());
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Rewards_Propmt, bag =>
            {
                RewardWindow.ShowRewardsContainer();
                Game.MainUi.ShowWindow(RewardWindow.View);
            });
        }
        public override void Show() => RewardWindow.Display(true);
        public override void Hide() => RewardWindow.Display(false);

        private class Win_Reward : UiBase
        {
            private ScrollRect Scroll_reward { get; }
            private ListViewUi<Prefab_item> ItemPrefab { get; }
            private Button Btn_confirm { get; }
            public Win_Reward(IView v, Action onClickAction) : base(v, true)
            {
                Scroll_reward = v.GetObject<ScrollRect>("scroll_reward");
                ItemPrefab = new ListViewUi<Prefab_item>(v.GetObject<View>("prefab"), Scroll_reward);
                Btn_confirm = v.GetObject<Button>("btn_confirm");
                Btn_confirm.OnClickAdd(() =>
                {
                    onClickAction?.Invoke();
                    Display(false);
                });
            }
            public void ShowRewardsContainer()
            {
                ItemPrefab.ClearList(ui => ui.Destroy());
                foreach( var rewards in Game.World.RewardContainer.Rewards)
                {
                    foreach (var item in rewards.AllItems)
                        InstancePrefab().Set(item.Item.Name, item.Item.About, item.Amount);
                    foreach (var package in rewards.Packages)
                        InstancePrefab().Set($"{package.Grade}阶包裹", package.Grade.ToString() ,1);
                }
                Display(true);
                Prefab_item InstancePrefab() => ItemPrefab.Instance(v => new Prefab_item(v, true));
            }
            private class Prefab_item : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_title { get; }
                private Text Text_value { get; }
                private Text Text_info { get; }
                public Prefab_item(IView v, bool display) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                    Text_info = v.GetObject<Text>("text_info");
                }
                public void SetIcon(Sprite ico) => Img_ico.sprite = ico;
                public void Set(string title, string info, int amount)
                {
                    var amtText = amount > 1 ? amount.ToString() : string.Empty;
                    Text_title.text = title;
                    Text_value.text = amtText;
                    Text_info.text = info;
                }
            }
        }
    }
}
