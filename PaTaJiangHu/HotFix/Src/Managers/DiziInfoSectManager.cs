using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;
using _GameClient.Models;
using BattleM;
using HotFix_Project.Views.Bases;
using Server.Configs._script.Factions;
using Systems.Messaging;

namespace HotFix_Project.Managers;

/// <summary>
/// 弟子信息板块Ui控制器
/// </summary>
public class DiziInfoSectManager
{
    private View_diziInfoSect DiziInfo { get; set; }
    private DiziController Controller { get; set; }

    public void Init()
    {
        Controller = Game.Controllers.Get<DiziController>();
        InitUi();
        RegEvents();
        //Game.MessagingManager.RegEvent(EventString.Model_DiziInfo_StaminaUpdate,
        //    arg => DiziInfo.UpdateStamina(JsonMapper.ToObject<int[]>(arg)));
        //Game.MessagingManager.RegEvent(EventString.Model_DiziInfo_StateUpdate,
        //    arg => DiziInfo.UpdateState(JsonMapper.ToObject<string[]>(arg)));
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Faction_DiziSelected, bag =>
        {
            DiziInfo.SetDizi(bag.Get<string>(0));
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => DiziInfo.Update());
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => DiziInfo.Update());
    }


    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziInfoSect", v =>
        {
            DiziInfo = new View_diziInfoSect(v, guid => Controller.ManageDiziCondition(guid));
            Game.MainUi.MainPage.Set(v, MainPageLayout.Sections.Top, true);
        });
    }

    private class View_diziInfoSect : UiBase
    {
        private View_charInfo CharInfo { get; }
        private View_diziProps DiziProps { get; }
        public View_diziInfoSect(IView v,Action<string> onStaminaBtnClick) : base(v.GameObject, true)
        {
            CharInfo = new View_charInfo(v.GetObject<View>("view_charInfo"),
                () => onStaminaBtnClick(SelectedDizi?.Guid));
            DiziProps = new View_diziProps(v.GetObject<View>("view_diziProps"));
        }

        private Dizi SelectedDizi { get; set; }

        public void SetDizi(string guid)
        {
            var faction = Game.World.Faction;
            var dizi = faction.DiziMap[guid];
            SelectedDizi = dizi;
            
            //var dizi = bag.Get<DiziDto>(0);
            SetDizi(dizi);
        }
        public void Update()
        {
            SetDizi(SelectedDizi);
            XDebug.Log($"弟子【{SelectedDizi.Name}】更新了!");
        }

        private void SetDizi(Dizi dizi)
        {
            var c = dizi.Capable;
            CharInfo.SetName(dizi.Name);
            CharInfo.SetLevel(dizi.Level);
            UpdateDiziStamina();

            SetProp(View_diziProps.Props.Strength, c.Strength.Grade, dizi.Strength, dizi.Weapon?.Damage ?? 0);
            SetProp(View_diziProps.Props.Agility, c.Agility.Grade, dizi.Agility);
            SetProp(View_diziProps.Props.Hp, c.Hp.Grade, dizi.Hp);
            SetProp(View_diziProps.Props.Mp, c.Mp.Grade, dizi.Mp);
            View.StopAllCo();
            View.StartCo(UpdateStaminaEverySecond());
        }

        IEnumerator UpdateStaminaEverySecond()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                UpdateDiziStamina();
            }
        }
        void UpdateDiziStamina()
        {
            var (stamina, max, min, sec) = SelectedDizi.Stamina.GetStaminaValue();
            CharInfo.SetStamina(stamina, max, min, sec);
        }
        private void SetPropIcon(View_diziProps.Props prop, Sprite icon) => DiziProps.SetIcon(prop, icon);
        private void SetProp(View_diziProps.Props prop, int grade, int value, int equip = 0, int condition = 0) =>
            DiziProps.Set(prop, grade, value, equip, condition);
        private class View_charInfo : UiBase
        {
            private Image Img_charAvatar { get; }
            private Text Text_charName { get; }
            private Text Text_charLevel { get; }
            private Text Text_charExpValue { get; }
            private Text Text_charExpMax { get; }
            private Scrollbar Scrbar_exp { get; }

            private View_statusList StatusList { get; }
            public View_charInfo(IView v,Action onStaminaBtnClick) : base(v.GameObject, true)
            {
                Img_charAvatar = v.GetObject<Image>("img_charAvatar");
                Text_charName = v.GetObject<Text>("text_charName");
                Text_charLevel = v.GetObject<Text>("text_charLevel");
                Text_charExpValue = v.GetObject<Text>("text_charExpValue");
                Text_charExpMax = v.GetObject<Text>("text_charExpMax");
                Scrbar_exp = v.GetObject<Scrollbar>("scrbar_exp");
                StatusList = new View_statusList(v.GetObject<View>("view_statusList"), onStaminaBtnClick);
            }

            public void SetAvatar(Sprite avatar) => Img_charAvatar.sprite = avatar;
            public void SetName(string name) => Text_charName.text = name;
            public void SetLevel(int level) => Text_charLevel.text = $"{level}级";
            public void SetExp(int value, int max)
            {
                Text_charExpValue.text = value.ToString();
                Text_charExpMax.text = max.ToString();
                Scrbar_exp.value = 1f * value / max;
            }
            public void SetPower(int power) => StatusList.SetPower(power);

            public void SetStamina(int staValue, int staMax, int min, int sec) =>
                StatusList.SetStamina(staValue, staMax, min, sec);

            public void SetState(string shortTitle, string description, int min, int sec) =>
                StatusList.SetState(shortTitle, description, min, sec);

            private class View_statusList : UiBase
            {
                private Text Text_powerValue { get; }
                private View_Stamina StaminaView { get; }
                private View_State StateView { get; }
                public View_statusList(IView v, Action onStaminaBtnAction) : base(v.GameObject, true)
                {
                    Text_powerValue = v.GetObject<Text>("text_powerValue");
                    StaminaView = new View_Stamina(v.GetObject<View>("view_stamina"), onStaminaBtnAction);
                    StateView = new View_State(v.GetObject<View>("view_state"));
                }

                public void SetPower(int power) => Text_powerValue.text = power.ToString();
                public void SetStamina(int staValue, int staMax, int min, int sec)
                {
                    StaminaView.SetTime(min, sec);
                    StaminaView.SetStamina(staValue, staMax);
                }
                public void SetState(string shortTitle, string description, int min, int sec)
                {
                    StateView.SetTitle(shortTitle);
                    StateView.SetDescription(description);
                    StateView.SetTime(min, sec);
                }

                private class View_Stamina : UiBase
                {
                    private Text Text_staminaMax { get; }
                    private Text Text_staminaValue { get; }
                    private View_time TimeView { get; }
                    private Button Btn_stamina { get; }

                    public View_Stamina(IView v, Action onStaminaBtnAction) : base(v.GameObject, true)
                    {
                        Text_staminaMax = v.GetObject<Text>("text_staminaMax");
                        Text_staminaValue = v.GetObject<Text>("text_staminaValue");
                        TimeView = new View_time(v.GetObject<View>("view_timeView"));
                        Btn_stamina = v.GetObject<Button>("btn_stamina");
                        Btn_stamina.OnClickAdd(onStaminaBtnAction);
                    }
                    public void SetStamina(int value, int max)
                    {
                        Text_staminaValue.text = value.ToString();
                        Text_staminaMax.text = max.ToString();
                    }
                    public void SetTime(int min, int sec) => TimeView.SetTime(min, sec);
                }
                private class View_State : UiBase
                {
                    private Text Text_stateTitle { get; }
                    private Text Text_stateTime { get; }
                    private Text Text_stateDescription { get; }
                    private View_time TimeView { get; }

                    public View_State(IView v) : base(v.GameObject, true)
                    {
                        Text_stateTitle = v.GetObject<Text>("text_stateTitle");
                        Text_stateDescription = v.GetObject<Text>("text_stateDescription");
                        TimeView = new View_time(v.GetObject<View>("view_timeView"));
                    }
                    public void SetTitle(string title) => Text_stateTitle.text = title;
                    public void SetTime(int min, int sec) => TimeView.SetTime(min, sec);
                    public void SetDescription(string description) => Text_stateDescription.text = description;
                }
                private class View_time : UiBase
                {
                    private Text Text_value { get; }
                    private Text Text_max { get; }
                    private GameObject Go_timeView { get; }

                    public View_time(IView v) : base(v.GameObject, true)
                    {
                        Text_value = v.GetObject<Text>("text_value");
                        Text_max = v.GetObject<Text>("text_max");
                        Go_timeView = v.GetObject("go_timeView");
                    }

                    public void SetTime(int min, int sec)
                    {
                        var full = min <= 0 && sec <= 0;
                        Go_timeView.SetActive(!full);
                        Text_value.text = min.ToString();
                        Text_max.text = sec.ToString();
                    }

                    public void Hide() => Display(false);
                }
            }
        }
        private class View_diziProps : UiBase
        {
            public enum Props { Strength, Agility, Hp, Mp }
            private Element_prop Strength { get; }
            private Element_prop Agility { get; }
            private Element_prop Hp { get; }
            private Element_prop Mp { get; }
            public View_diziProps(IView v) : base(v.GameObject, true)
            {
                Strength = new Element_prop(v.GetObject<View>("element_propStrength"));
                Agility = new Element_prop(v.GetObject<View>("element_propAgility"));
                Hp = new Element_prop(v.GetObject<View>("element_propHp"));
                Mp = new Element_prop(v.GetObject<View>("element_propMp"));
            }

            public void SetIcon(Props prop, Sprite icon) => GetProp(prop).SetIcon(icon);

            public void Set(Props prop, int grade, int value, int equip, int condition) =>
                GetProp(prop).Set(grade, value, equip, condition);

            private Element_prop GetProp(Props prop) => prop switch
            {
                Props.Strength => Strength,
                Props.Agility => Agility,
                Props.Hp => Hp,
                Props.Mp => Mp,
                _ => throw new ArgumentOutOfRangeException(nameof(prop), prop, null)
            };

            private class Element_prop : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_grade { get; }
                private Text Text_prop { get; }
                private Text Text_equip { get; }
                private Text Text_condition { get; }

                public Element_prop(IView v) : base(v.GameObject, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_grade = v.GetObject<Text>("text_grade");
                    Text_prop = v.GetObject<Text>("text_prop");
                    Text_equip = v.GetObject<Text>("text_equip");
                    Text_condition = v.GetObject<Text>("text_condition");
                }

                public void SetIcon(Sprite icon) => Img_ico.sprite = icon;
                public void Set(int grade, int value, int equip, int condition)
                {
                    Text_grade.text = GetGrade(grade);
                    Text_prop.text = SetText(value);
                    Text_equip.text = SetText(equip);
                    Text_condition.text = SetText(condition);

                    string SetText(int v,string prefix = "") => v == 0 ? string.Empty : prefix + v;
                }

                private static string GetGrade(int grade) => grade switch
                {
                    0 => "F",
                    1 => "E",
                    2 => "D",
                    3 => "C",
                    4 => "B",
                    5 => "A",
                    6 => "S",
                    _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
                };
            }
        }
    }
}