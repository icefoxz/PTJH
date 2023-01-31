using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Src.Managers;

public class BtmPageBtnsManager
{
    private View_btmPageBtns BtmPageBtns { get; set; }

    public void Init()
    {
        InitUi();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_btmPageBtns", v =>
        {
            BtmPageBtns = new View_btmPageBtns(v,
                onTreasureHouse: () => Game.MessagingManager.Send(EventString.Page_TreasureHouse, null),
                onDizi: () => Game.MessagingManager.Send(EventString.Page_DiziList, null),
                onFaction: () => Game.MessagingManager.Send(EventString.Page_Faction, null),
                onAdventure: () => Game.MessagingManager.Send(EventString.Page_Adventure, null),
                onDiziRecreuitment: () => Game.MessagingManager.Send(EventString.Page_DiziRecruit, null)
                );
              
            Game.MainUi.SetBtm(v, true);
        }, RegEvents);
    }

    private void RegEvents()
    {
        BtmPageBtns.Display(true);
    }

    private class View_btmPageBtns : UiBase
    {
        private Button Btn_1 { get; }
        private Button Btn_2 { get; }
        private Button Btn_3 { get; }
        private Button Btn_4 { get; }
        private Button Btn_5 { get; }

        public View_btmPageBtns(IView v, 
            Action onTreasureHouse, 
            Action onDizi,
            Action onFaction, 
            Action onAdventure, 
            Action onDiziRecreuitment) : base(v.GameObject, true)
        {
            Btn_1 = v.GetObject<Button>("btn_1");
            Btn_1.OnClickAdd(onTreasureHouse);
            Btn_2 = v.GetObject<Button>("btn_2");
            Btn_2.OnClickAdd(onDizi);
            Btn_3 = v.GetObject<Button>("btn_3");
            Btn_3.OnClickAdd(onFaction);
            Btn_4 = v.GetObject<Button>("btn_4");
            Btn_4.OnClickAdd(onAdventure);
            Btn_5 = v.GetObject<Button>("btn_5");
            Btn_5.OnClickAdd(onDiziRecreuitment);
        }
    }
}
