using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers;

public class DiziRecruitManager
{
    private View_diziRecruitPage DiziRecruitPage { get; set; }

    public void Init()
    {
        ///Init...
    }
    private class View_diziRecruitPage : UiBase
    {
        private Button Btn_Recruit { get; }
        private Text Text_SilverCost { get; }
        private View_RecruitWindow RecruitWindow { get; }

        public View_diziRecruitPage(IView v) : base(v.GameObject, true)
        {
            Btn_Recruit = v.GetObject<Button>("btn_recruit");
            Text_SilverCost = v.GetObject<Text>("text_silverCost");
            RecruitWindow = new View_RecruitWindow(v.GetObject<View>("view_recruitWindow"));
        }


        private class View_RecruitWindow : UiBase
        {
            private Image Img_charAvatar { get; }
            private Text Text_charName { get; }
            private Button Btn_Accept { get; }
            private Button Btn_Reject { get; }
            public View_RecruitWindow(IView v) : base(v.GameObject, false)
            {
                Img_charAvatar = v.GetObject<Image>("img_charAvatar");
                Text_charName = v.GetObject<Text>("text_charName");
                Btn_Accept = v.GetObject<Button>("btn_accept");
                Btn_Reject = v.GetObject<Button>("btn_reject");
            }
            public void SetIcon(Sprite icon) => Img_charAvatar.sprite = icon;
            public void SetDiziName(string name) => Text_charName.text = name;
        }
    }
}
