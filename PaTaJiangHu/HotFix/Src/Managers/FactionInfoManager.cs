using HotFix_Project.Views.Bases;
using _GameClient.Models;
using HotFix_Project.Serialization;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class FactionInfoManager
{
    private View_factionInfoUi FactionInfoUi { get; set; }

    public void Init()
    {
        Game.UiBuilder.Build("view_factionInfoUi", v =>
        {
            FactionInfoUi = new View_factionInfoUi(v);
            Game.MainUi.SetTop(v, true);
        }, RegEvents);
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_Init,
            bag =>
            {
                FactionInfoUi.SetFaction();
                FactionInfoUi.Display(true);
            });
        Game.MessagingManager.RegEvent(EventString.Faction_SilverUpdate,
            bag=> FactionInfoUi.SetSilver(bag.GetInt(0)));
        Game.MessagingManager.RegEvent(EventString.Faction_YuanBaoUpdate,
            bag => FactionInfoUi.SetYuanBao(bag.GetInt(0)));
        Game.MessagingManager.RegEvent(EventString.Faction_Params_ActionLingUpdate,
            bag => FactionInfoUi.SetActionToken(bag.GetInt(0), bag.GetInt(1), bag.GetInt(2), bag.GetInt(3)));
    }

    private class View_factionInfoUi : UiBase
    {
        private Element Element_Silver { get; }
        private Element Element_Yuanbao { get; }
        private View_actionToken ActionToken { get; }

        public View_factionInfoUi(IView v) : base(v.GameObject, true)
        {
            Element_Silver = new Element(v.GetObject<View>("element_silver"));
            Element_Yuanbao = new Element(v.GetObject<View>("element_yuanbao"));
            ActionToken = new View_actionToken(v.GetObject<View>("view_actionToken"));
        }
        public void SetFaction()
        {
            var f = Game.World.Faction;
            Element_Silver.SetText(f.Silver.ToString());
            Element_Yuanbao.SetText(f.YuanBao.ToString());
            ActionToken.SetToken(f.ActionLing, f.ActionLing);
        }

        public void SetSilver(int silver) => Element_Silver.SetText(silver.ToString());
        public void SetYuanBao(int yuanBao) => Element_Yuanbao.SetText(yuanBao.ToString());
        public void SetActionToken(int value, int max, int min, int sec)
        {
            ActionToken.SetToken(value, max);
            ActionToken.SetTimer(min, sec);
        }


        //private class
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
                Text_timerMin.text = EmptyIfZero(min);
                Text_timerSec.text = EmptyIfZero(sec);
            }

            private string EmptyIfZero(int value) => value==0 ? string.Empty : value.ToString();
        }

    }
    
}
