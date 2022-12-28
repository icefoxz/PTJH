using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers;

public class FactionInfoManager
{
    private View_factionInfo FactionInfo { get; set; }

    public void Init()
    {
        //Init method......
    }

    private class View_factionInfo : UiBase
    {
        private Element Element_Silver { get; }
        private Element Element_Yuanbao { get; }
        private View_actionToken ActionToken { get; }

        public View_factionInfo(IView v) : base(v.GameObject, true)
        {
            Element_Silver = new Element(v.GetObject<View>("element_silver"));
            Element_Yuanbao = new Element(v.GetObject<View>("element_yuanbao"));
            ActionToken = new View_actionToken(v.GetObject<View>("view_actionToken"));
        }
        private class Element : UiBase
        {
            private Text Text_resValue { get; }

            public Element(IView v) : base(v.GameObject, true)
            {
                Text_resValue = v.GetObject<Text>("text_resValue");
            }
            public void SetText(string text)
            {
                Text_resValue.text = text;
            }
        }
        private class View_actionToken : UiBase
        {
            private Text Text_actionTokenValue { get; }
            private Text Text_actionTokenMax { get; }
            private Text Text_timerMin { get; }
            private Text Text_timerSec { get; } 

            public View_actionToken(IView v) : base(v.GameObject, true)
            {
                Text_actionTokenValue = v.GetObject<Text>("text_actionTokenValue");
                Text_actionTokenMax = v.GetObject<Text>("text_actionTokenMax");
                Text_timerMin = v.GetObject<Text>("text_timerMin");
                Text_timerSec = v.GetObject<Text>("text_timerSec");
            }

            //Methods
            public void SetToken(int value, int max)
            {
                Text_actionTokenValue.text = value.ToString();
                Text_actionTokenMax.text = max.ToString();
            }
            public void SetTimer(int min, int sec)
            {
                Text_timerMin.text = min.ToString();
                Text_timerSec.text = sec.ToString();
            }
        }
    }
    
}
