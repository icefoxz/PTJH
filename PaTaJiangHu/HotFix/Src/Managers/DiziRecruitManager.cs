using System;
using _GameClient.Models;
using HotFix_Project.Views.Bases;
using Server.Configs._script.Factions;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers;

public class DiziRecruitManager
{
    private View_diziRecruitPage DiziRecruitPage { get; set; }
    private RecruitController RecruitController { get; set; }
    private int CurrentDiziIndex { get; set; }

    public void Init()
    {
        RecruitController = Game.Controllers.Get<RecruitController>();
        InitUis();
        RegEvents();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Recruit_DiziGenerated, bag => DiziRecruitPage.SetDizi(bag));
        Game.MessagingManager.RegEvent(EventString.Recruit_DiziInSlot,
            bag => CurrentDiziIndex = bag.Get<int[]>(0)[0]);
    }


    private void InitUis()
    {
        Game.UiBuilder.Build("view_diziRecruitPage", v =>
        {
            DiziRecruitPage = new View_diziRecruitPage(v, () => RecruitController.GenerateDizi(), 
                ()=> RecruitController.RecruitDizi(CurrentDiziIndex));
            Game.MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        });
    }

    private class View_diziRecruitPage : UiBase
    {
        private Button Btn_Recruit { get; }
        private Text Text_SilverCost { get; }
        private View_RecruitWindow RecruitWindow { get; }

        public View_diziRecruitPage(IView v, Action onRecruitAction, Action onAcceptAction) : base(v.GameObject, true)
        {
            Btn_Recruit = v.GetObject<Button>("btn_recruit");
            Btn_Recruit.OnClickAdd(onRecruitAction);
            Text_SilverCost = v.GetObject<Text>("text_silverCost");
            RecruitWindow = new View_RecruitWindow(v.GetObject<View>("view_recruitWindow"), onAcceptAction);
        }

        public void SetDizi(ObjectBag bag)
        {
            var diziName = bag.Get<string>(0);
            RecruitWindow.SetDiziName(diziName);
            RecruitWindow.Display(true);
        }

        //private class
        private class View_RecruitWindow : UiBase
        {
            private Image Img_charAvatar { get; }
            private Text Text_charName { get; }
            private Button Btn_Accept { get; }
            private Button Btn_Reject { get; }
            public View_RecruitWindow(IView v,Action onAcceptAction) : base(v.GameObject, false)
            {
                Img_charAvatar = v.GetObject<Image>("img_charAvatar");
                Text_charName = v.GetObject<Text>("text_charName");
                Btn_Accept = v.GetObject<Button>("btn_accept");
                Btn_Accept.OnClickAdd(() =>
                {
                    onAcceptAction?.Invoke();
                    Display(false);
                });
                Btn_Reject = v.GetObject<Button>("btn_reject");
                Btn_Reject.OnClickAdd(OnReject);
            }
            public void SetIcon(Sprite icon) => Img_charAvatar.sprite = icon;
            public void SetDiziName(string name) => Text_charName.text = name;
            private void OnReject() => Display(false); 
        }
    }
}
