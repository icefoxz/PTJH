using System;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_ConsumeResMgr : MainPageBase
    {
        private View_ConsumeResMgr ConsumeRes { get; set; }
        private FactionController FactionController { get; }
        private DiziController DiziController { get; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Top;
        protected override string ViewName => "demo_view_consumeRes";
        protected override bool IsDynamicPixel => true;
        public Demo_View_ConsumeResMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            FactionController = Game.Controllers.Get<FactionController>();
            DiziController = Game.Controllers.Get<DiziController>();
        }

        protected override void Build(IView view)
        {
            ConsumeRes = new View_ConsumeResMgr(view,
                onResourceClick: (guid, res) => FactionController.ConsumeResourceByStep(guid, res),
                onSilverAction: (guid, silver) => DiziController.UseSilver(guid, silver));
        }
        protected override void RegEvents()
        {
            //Game.MessagingManager.RegEvent(EventString.Faction_DiziSelected, bag =>
            //{
            //    ConsumeRes.Set(bag);
            //});
            Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate, bag => ConsumeRes.Update(bag.GetString(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag => ConsumeRes.Update(bag.GetString(0)));
            Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Finalize, bag => ConsumeRes.Update(bag.GetString(0)));
        }
        public override void Show() => ConsumeRes.Display(true);
        public override void Hide() => ConsumeRes.Display(false);

        public void Set(Dizi dizi) => ConsumeRes.Set(dizi.Guid);

        private class View_ConsumeResMgr : UiBase
        {
            private Element Silver { get; }
            private Element Food { get; }
            private Element Emotion { get; }
            private Element Injury { get; }
            private Element Inner { get; }
            public View_ConsumeResMgr(IView v,
                Action<string, IAdjustment.Types> onResourceClick,
                Action<string, int> onSilverAction) : base(v, true)
            {
                Silver = new Element(v.GetObject<View>("element_silver"), 
                    ()=> onSilverAction?.Invoke(SelectedDizi?.Guid, 1));
                Food = new Element(v.GetObject<View>("element_food"),
                    ()=> onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Food));
                Emotion = new Element(v.GetObject<View>("element_emotion"),
                    ()=> onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Emotion));
                Injury = new Element(v.GetObject<View>("element_injury"),
                    ()=> onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Injury));
                Inner = new Element(v.GetObject<View>("element_inner"),
                    ()=> onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Inner));
            }

            private Dizi SelectedDizi { get; set; }
            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                SetDiziElement(SelectedDizi);
                var dizi = SelectedDizi;
                var isIdleState = dizi.State.Current == DiziStateHandler.States.Idle;
                Silver.SetInteraction(isIdleState);
                Food.SetInteraction(isIdleState);
                Emotion.SetInteraction(isIdleState);
                Injury.SetInteraction(isIdleState);
                Inner.SetInteraction(isIdleState);
                if (isIdleState && dizi.Silver.Value == 100) Silver.SetInteraction(false);
                else Silver.SetInteraction(true);
            }
            private void SetDiziElement(Dizi dizi)
            {
                var controller = Game.Controllers.Get<DiziController>();
                var (silverText, sColor) = controller.GetSilverCfg(dizi.Silver.ValueMaxRatio);
                var (foodText, fColor) = controller.GetFoodCfg(dizi.Food.ValueMaxRatio);
                var (emotionText, eColor) = controller.GetEmotionCfg(dizi.Emotion.ValueMaxRatio);
                var (injuryText, jColor) = controller.GetInjuryCfg(dizi.Injury.ValueMaxRatio);
                var (innerText, nColor) = controller.GetInnerCfg(dizi.Inner.ValueMaxRatio);
                Silver.SetElement(1, sColor, silverText);
                Silver.SetValue(dizi.Silver.Value, dizi.Silver.Max);
                Food.SetElement(dizi.Capable.Food, fColor, foodText);
                Food.SetValue(dizi.Food.Value, dizi.Food.Max);
                Emotion.SetElement(dizi.Capable.Wine, eColor, emotionText);
                Emotion.SetValue(dizi.Emotion.Value, dizi.Emotion.Max);
                Injury.SetElement(dizi.Capable.Herb, jColor, injuryText);
                Injury.SetValue(dizi.Injury.Value, dizi.Injury.Max);
                Inner.SetElement(dizi.Capable.Pill, nColor, innerText);
                Inner.SetValue(dizi.Inner.Value, dizi.Inner.Max);

            }

            internal void Set(string guid)
            {
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }

            private class Element : UiBase
            {
                private Text Text_consume { get; }
                private Scrollbar Scrbar_status { get; }
                private Text Text_statusValue { get; }
                private Button Btn_status { get; }
                private Text Text_statusInfo { get; }
                private Image BgImg { get; }
                private Image HandleImg { get; }
                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    Text_consume = v.GetObject<Text>("text_consume");
                    Scrbar_status = v.GetObject<Scrollbar>("scrbar_status");
                    HandleImg = Scrbar_status.image;
                    BgImg = Scrbar_status.GetComponent<Image>();
                    Text_statusValue = v.GetObject<Text>("text_statusValue");
                    Btn_status = v.GetObject<Button>("btn_status");
                    Btn_status.OnClickAdd(onClickAction);
                    Text_statusInfo = v.GetObject<Text>("text_statusInfo");
                }
                public void SetElement(int value, Color color, string info)
                {
                    SetConsume(value);
                    SetColor(color);
                    SetInfo(info);
                }
                public void SetConsume(int value) => Text_consume.text = value.ToString();
                public void SetInfo(string info) => Text_statusInfo.text = info;
                public void SetColor(Color color)
                {
                    HandleImg.color = color;
                    BgImg.color = new Color(color.r - 0.7f, color.g - 0.7f, color.b - 0.7f);
                }
                public void SetValue(int value, int max)
                {
                    Text_statusValue.text = value.ToString();
                    Scrbar_status.size = 1f * value/max;
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_status.interactable = isInteractable;
                }
            }
        }
    }
}
