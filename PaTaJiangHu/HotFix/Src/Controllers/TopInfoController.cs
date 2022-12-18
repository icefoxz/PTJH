using HotFix_Project.Views.Bases;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Controllers
{
    /// <summary>
    /// 冒险控制器
    /// </summary>
    internal class TopInfoController 
    {
        private PlayerInfoView PlayerInfo { get; set; }
        private ResView Res { get; set; }
        private ActionTokenView ActionToken { get; set; }
        public TopInfoController()
        {
            Game.UiBuilder.Build("view_topInfoUi", v =>
            {
                PlayerInfo = new PlayerInfoView(v);
                Res = new ResView(v);
                ActionToken = new ActionTokenView(v);
            }, Game.MainUi.TopUi.transform);
        }
        private class PlayerInfoView : UiBase
        {
            private Text Text_playerName { get; }
            private Image Img_avatar { get; }
            private Scrollbar Scrbar_exp { get; }
            private Text Text_expValue { get; }
            private Text Text_expMax { get; }


            public PlayerInfoView(IView v) : base(v.GameObject, true)
            {
                Text_playerName = v.GetObject<Text>("text_playerName");
                Img_avatar = v.GetObject<Image>("img_avatar");
                Scrbar_exp = v.GetObject<Scrollbar>("scrbar_exp");
                Text_expValue = v.GetObject<Text>("text_expValue");
                Text_expMax = v.GetObject<Text>("text_expMax");
            }

            public void SetName(string name) => Text_playerName.text = name;
            public void SetAvatar(Sprite avatar)=> Img_avatar.sprite = avatar;
            public void SetExp(int value, int max)
            {
                Scrbar_exp.value = 1f * value / max;
                Text_expValue.text = value.ToString();
                Text_expMax.text = max.ToString();
            }
        }
        private class ResView : UiBase
        {
            private Text Text_resValue { get; }
            public ResView(IView v) : base(v.GameObject, true)
            {
                Text_resValue = v.GetObject<Text>("text_resValue");
            }
            public void SetValue(int value)=> Text_resValue.text = value.ToString();
        }
        private class ActionTokenView : UiBase
        {
            private Text Text_actionTokenValue { get; }
            private Text Text_actionTokenMax { get; }
            private Text Text_timerMin { get; }
            private Text Text_timerSec { get; }

            public ActionTokenView(IView v) : base(v.GameObject, true)
            {
                Text_actionTokenMax = v.GetObject<Text>("text_actionTokenMax");
                Text_actionTokenValue = v.GetObject<Text>("text_actionTokenValue");
                Text_timerMin = v.GetObject<Text>("text_timerMin");
                Text_timerSec = v.GetObject<Text>("text_timerSec");
            }

            public void SetActionToken(int value,int max = -1)
            {
                Text_actionTokenValue.text = value.ToString();
                if (max >= 0)
                    Text_actionTokenMax.text = max.ToString();
            }

            public void SetTimer(int sec, int min = -1)
            {
                Text_timerSec.text = sec.ToString();
                if (min >= 0)
                    Text_timerMin.text = min.ToString();
            }
        }
    }
}
