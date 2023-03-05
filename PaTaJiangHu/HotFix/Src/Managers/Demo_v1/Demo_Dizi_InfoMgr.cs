﻿using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers.Demo_v1
{
    internal class Demo_Dizi_InfoMgr : MainPageBase
    {
        private Dizi_Info Dizi_info {  get; set; }
        private DiziController DiziController { get; set; }
        protected override MainPageLayout.Sections MainPageSection { get; } = MainPageLayout.Sections.Top;
        protected override string ViewName => "demo_dizi_info";
        protected override bool IsDynamicPixel => false;
        public Demo_Dizi_InfoMgr(MainUiAgent uiAgent) : base(uiAgent)
        {
            DiziController = Game.Controllers.Get<DiziController>();
        }
        protected override void Build(IView view)
        {
            Dizi_info = new Dizi_Info(view);
        }
        protected override void RegEvents()
        {
            Game.MessagingManager.RegEvent(EventString.Faction_DiziSelected, bag =>
            {
                Dizi_info.SetDizi(bag.Get<string>(0));
            });
        }

        public override void Show() => Dizi_info.Display(true);
        public override void Hide() => Dizi_info.Display(false);
        private class Dizi_Info : UiBase
        {
            private Image Img_diziAvatar { get; }
            private Text Text_diziName { get; }
            private View_Level LevelView { get; }
            private View_Stamina StaminaView { get; }
            public Dizi_Info(IView v) : base(v, true)
            {
                Img_diziAvatar = v.GetObject<Image>("img_diziAvatar");
                Text_diziName = v.GetObject<Text>("text_diziName");
                LevelView = new View_Level(v.GetObject<View>("view_level"));
                StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"));
            }
            private Dizi SelectedDizi { get; set; }
            public void SetIcon(Sprite ico) => Img_diziAvatar.sprite = ico;
            public void SetName(string name, int color)
            {
                Text_diziName.text = name;
                Text_diziName.color = Game.GetColorFromGrade(color);
            }

            public void SetDizi(string guid)
            {
                var faction = Game.World.Faction;
                var dizi = faction.GetDizi(guid);
                SelectedDizi = dizi;
                SetDizi(dizi);
            }
            private void SetDizi(Dizi dizi)
            {
                SetName(dizi.Name, dizi.Grade);
                LevelView.SetLevel(dizi.Level);
                LevelView.SetExp(dizi.Exp.Value, dizi.Exp.Max);
                StaminaView.SetHour(0);
            }
            private class View_Level : UiBase
            {
                private Text Text_levelValue { get; }
                private Text Text_expValue { get; }
                private Text Text_expMax { get; }
                private Slider Slider_exp { get; }
                public View_Level(IView v) : base(v, true)
                {
                    Text_levelValue = v.GetObject<Text>("text_levelValue");
                    Text_expValue = v.GetObject<Text>("text_expValue");
                    Text_expMax = v.GetObject<Text>("text_expMax");
                    Slider_exp = v.GetObject<Slider>("slider_exp");
                }
                public void SetLevel(int level) => Text_levelValue.text = level.ToString();
                public void SetExp(int value, int max)
                {
                    Text_expValue.text = value.ToString();
                    Text_expMax.text = max.ToString();
                    Slider_exp.value = 1f * value / max;
                }
            }

            private class View_Stamina : UiBase
            {
                private View_Volume VolumeView { get; }
                private Text Text_hour { get; }
                public View_Stamina(IView v) : base(v, true)
                {
                    VolumeView = new View_Volume(v.GetObject<View>("view_volume"));
                    Text_hour = v.GetObject<Text>("text_hour");
                }
                public void SetHour(int hour) => Text_hour.text = hour.ToString();

                private class View_Volume : UiBase
                {
                    private Text Text_value { get; }
                    private Text Text_max { get; }
                    public View_Volume(IView v) : base(v, true)
                    {
                        Text_value = v.GetObject<Text>("text_value");
                        Text_max = v.GetObject<Text>("text_max");
                    }
                    public void SetVolume(int value, int max)
                    {
                        Text_value.text = value.ToString();
                        Text_max.text = max.ToString();
                    }
                }
            }
        }
    }
}
