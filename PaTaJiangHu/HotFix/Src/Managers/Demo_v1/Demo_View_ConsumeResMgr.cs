using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Server.Configs.Adventures;
using Server.Controllers;
using System;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_View_ConsumeResMgr : MainPageBase
    {
        private View_ConsumeResMgr ConsumeRes { get; set; }
        private FactionController FactionController { get; set; }
        protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Top;
        protected override string ViewName => "demo_view_consumeRes";
        protected override bool IsDynamicPixel => true;
        public Demo_View_ConsumeResMgr(MainUiAgent uiAgent) : base(uiAgent)
        {
            FactionController = Game.Controllers.Get<FactionController>();
        }

        protected override void Build(IView view)
        {
            ConsumeRes = new View_ConsumeResMgr(view,
                onResourceClick: (guid, res)=> FactionController.ConsumeResourceByStep(guid, res));
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Dizi_ConditionUpdate, bag => ConsumeRes.Update(bag.GetString(0)));
        }
        public override void Show() => ConsumeRes.Display(true);
        public override void Hide() => ConsumeRes.Display(false);

        private class View_ConsumeResMgr : UiBase
        {
            private Element Silver { get; }
            private Element Food { get; }
            private Element Emotion { get; }
            private Element Injury { get; }
            private Element Inner { get; }
            public View_ConsumeResMgr(IView v,
                Action<string, IAdjustment.Types> onResourceClick) : base(v, true)
            {
                Silver = new Element(v.GetObject<View>("element_silver"),
                    ()=> onResourceClick?.Invoke(SelectedDizi?.Guid, IAdjustment.Types.Silver));
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
                Silver.SetInteraction(dizi.Adventure == null);
                Food.SetInteraction(dizi.Adventure == null);
                Emotion.SetInteraction(dizi.Adventure == null);
                Injury.SetInteraction(dizi.Adventure == null);
                Inner.SetInteraction(dizi.Adventure == null);
            }
            private void SetDiziElement(Dizi dizi)
            {
                var controller = Game.Controllers.Get<DiziController>();
                var (silverText, sColor) = controller.GetSilverCfg(dizi.Silver.ValueMaxRatio);
                var (foodText, fColor) = controller.GetFoodCfg(dizi.Food.ValueMaxRatio);
                var (emotionText, eColor) = controller.GetEmotionCfg(dizi.Emotion.ValueMaxRatio);
                var (injuryText, jColor) = controller.GetInjuryCfg(dizi.Injury.ValueMaxRatio);
                var (innerText, nColor) = controller.GetInnerCfg(dizi.Inner.ValueMaxRatio);
                Silver.SetElement(100, sColor, silverText);
                Food.SetElement(dizi.Capable.Food, fColor, foodText);
                Emotion.SetElement(dizi.Capable.Wine, eColor, emotionText);
                Injury.SetElement(dizi.Capable.Herb, jColor, injuryText);
                Inner.SetElement(dizi.Capable.Pill, nColor, innerText);

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
                public void SetValue(int value)
                {
                    Text_statusValue.text = value.ToString();
                    Scrbar_status.size = 1 * value/100;
                }

                public void SetInteraction(bool isInteractable)
                {
                    Btn_status.interactable = isInteractable;
                }
            }
        }
    }
}
