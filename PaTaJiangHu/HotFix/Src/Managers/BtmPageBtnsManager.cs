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
            BtmPageBtns = new View_btmPageBtns(v);
              
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

        public View_btmPageBtns(IView v/*, Action onTreasureHousePage, Action onDiziPage,
            Action onFactionPage, Action onDiziRecreuitmentPage*/) : base(v.GameObject, true)
        {
            Btn_1 = v.GetObject<Button>("btn_1");
            //Btn_1.OnClickAdd(onTreasureHousePage);
            Btn_2 = v.GetObject<Button>("btn_2");
            //Btn_2.OnClickAdd(onDiziPage);
            Btn_3 = v.GetObject<Button>("btn_3");
            //Btn_3.OnClickAdd(onFactionPage);
            Btn_4 = v.GetObject<Button>("btn_4");
            //Btn_4.OnClickAdd();
            Btn_5 = v.GetObject<Button>("btn_5");
            //Btn_5.OnClickAdd(onDiziRecreuitmentPage);
        }
    }
}
