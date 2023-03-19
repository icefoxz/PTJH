using System;
using _GameClient.Models;
using DiziM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class Page_dizi_activity :MainPageBase
{
    private View_main_diziState Main_DiziState { get; set; }
    private ScrollRect Scroll_activityLog { get; set; }
    private View_status Status { get; set; }
    private View_rewards Rewards { get; set; }
    private DiziController DiziController { get; set; }

    public Page_dizi_activity(GameSceneAgent uiAgent) : base(uiAgent)
    {
    }

    protected override string ViewName => "page_dizi_activity";
    protected override bool IsDynamicPixel => true;
    protected override void Build(IView view)
    {
        Main_DiziState = new View_main_diziState(view);
        Status = new View_status(view, () => XDebug.Log(""));
        Rewards = new View_rewards(view);
        DiziController = Game.Controllers.Get<DiziController>();
    }

    protected override void RegEvents()
    {
        throw new System.NotImplementedException();
    }

    public override void Show()
    {
        Main_DiziState.Display(true);
        Status.Display(true);
        Rewards.Display(true);
    }

    public override void Hide()
    {
        Main_DiziState.Display(false);
        Status.Display(false);
        Rewards.Display(false);
    }

    protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Btm;

    private class View_main_diziState : UiBase
    {
        private View_diziIcon DiziIcon { get; }
        private View_diziStatus DiziStatus { get; }
        public View_main_diziState(IView v) : base(v, true)
        {
            DiziIcon = new View_diziIcon(v.GetObject<View>("view_diziIcon"));
            DiziStatus = new View_diziStatus(v.GetObject<View>("view_diziStatus"));
        }
        public void Set(Dizi dizi)
        {
            DiziIcon.SetName(dizi.Name, dizi.Grade);
            DiziIcon.SetLevel(dizi.Level);
            DiziIcon.SetExp(dizi.Exp.Value, dizi.Exp.Max);
            DiziIcon.SetPower(dizi.Power);

            DiziStatus.Update(dizi.Guid);
        }

        private class View_diziIcon : UiBase
        {
            private Text Text_diziName { get; }
            private Text Text_diziLevel { get; }
            private Scrollbar Scrbar_exp { get; }
            private Text Text_charExpValue { get; }
            private Text Text_charExpMax { get; }
            private Text Text_powerValue { get; }
            private View_timerView TimerView { get; }
            public View_diziIcon(IView v) : base(v.GameObject, true)
            {
                Text_diziName = v.GetObject<Text>("text_diziName");
                Text_diziLevel = v.GetObject<Text>("text_diziLevel");
                Scrbar_exp = v.GetObject<Scrollbar>("scrbar_exp");
                Text_charExpValue = v.GetObject<Text>("text_charExpValue");
                Text_charExpMax = v.GetObject<Text>("text_charExpMax");
                Text_powerValue = v.GetObject<Text>("text_powerValue");
                TimerView = new View_timerView(v.GetObject<View>("view_timerView"));
            }
            public void SetName(string name, int grade)
            {
                Text_diziName.text = name;
                Text_diziName.color = Game.GetColorFromGrade(grade);
            }
            public void SetLevel(int level) => Text_diziLevel.text = $"{level}级";
            public void SetExp(int value, int max)
            {
                Text_charExpValue.text = value.ToString();
                Text_charExpMax.text = max.ToString();
                Scrbar_exp.size = 1f * value / max;
            }
            public void SetTime(int min, int sec) => TimerView.SetTime(min, sec);
            public void SetPower(int power) => Text_powerValue.text = power.ToString();
            private class View_timerView : UiBase
            {
                private Text Text_time { get; }
                private GameObject GO_timeView { get; }
                private Text Text_value { get; }
                private Text Text_max { get; }
                public View_timerView(IView v) : base(v.GameObject, true)
                {
                    Text_time = v.GetObject<Text>("text_time");
                    GO_timeView = v.GetObject<GameObject>("go_timeView");
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                }
                public void SetText(string text) => Text_time.text = text;
                public void SetTime(int min, int sec)
                {
                    var full = min <= 0 && sec <= 0;
                    GO_timeView.SetActive(full);
                    Text_value.text = min.ToString("00");
                    Text_max.text = sec.ToString("00");
                }
            }
        }

        private class View_diziStatus : UiBase
        {
            public enum Props { Strength, Agility, Hp, Mp }
            public enum Conditions { Food, State, Injury, Inner, Silver }
            private ElementManager ElementMgr { get; }
            public View_diziStatus(IView v) : base(v.GameObject, true)
            {
                ElementMgr = new ElementManager(
                    new Element_prop(v.GetObject<View>("element_propStrength")),
                    new Element_prop(v.GetObject<View>("element_propAgility")),
                    new Element_prop(v.GetObject<View>("element_propHp")),
                    new Element_prop(v.GetObject<View>("element_propMp")),
                    new Element_con(v.GetObject<View>("element_conFood")),
                    new Element_con(v.GetObject<View>("element_conState")),
                    new Element_con(v.GetObject<View>("element_conInjury")),
                    new Element_con(v.GetObject<View>("element_conInner")),
                    new Element_con(v.GetObject<View>("element_conSilver"))
                    );
            }
            private Dizi SelectedDizi { get; set; }//cache
            public void Set(ObjectBag bag)
            {
                var guid = bag.Get<string>(0);
                var dizi = Game.World.Faction.GetDizi(guid);
                SelectedDizi = dizi;
                Update(SelectedDizi.Guid);
            }
            public void Update(string guid)
            {
                if (SelectedDizi == null || SelectedDizi.Guid != guid) return;
                SetDizi(SelectedDizi);
                var dizi = SelectedDizi;                
            }
            
            private void SetDizi(Dizi dizi)
            {
                SetDiziElements(dizi);
            }

            private void SetDiziElements(Dizi dizi)
            {
                SetElementProp(Props.Strength, dizi.Strength);
                SetElementProp(Props.Agility, dizi.Agility);
                SetElementProp(Props.Hp, dizi.Hp);
                SetElementProp(Props.Mp, dizi.Mp);
                SetElementCon(Conditions.Silver, dizi.Silver);
                SetElementCon(Conditions.Food, dizi.Food);
                SetElementCon(Conditions.State, dizi.Emotion);
                SetElementCon(Conditions.Injury, dizi.Injury);
                SetElementCon(Conditions.Inner, dizi.Inner);

                void SetElementCon(Conditions co, IConditionValue con)
                {
                    ElementMgr.SetConValue(co, con.Value, con.Max);
                }
                void SetElementProp(Props prop, int value)
                {
                    ElementMgr.SetPropValue(prop, value);
                }
            }
            private class Element_prop : UiBase
            {
                private Image Img_ico { get; }
                private Text Text_prop { get; }
                public Element_prop(IView v) : base(v.GameObject, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_prop = v.GetObject<Text>("text_prop");
                }
                public void SetIcon(Sprite icon) => Img_ico.sprite = icon;
                public void SetText(int value) => Text_prop.text = value.ToString();
            }
            private class Element_con : UiBase
            {
                private Scrollbar Scrbar_condition { get; }
                private Text Text_value { get; }
                private Text Text_max { get; }
                public Element_con(IView v) : base(v.GameObject, true)
                {
                    Scrbar_condition = v.GetObject<Scrollbar>("scrbac_condition");
                    Text_value = v.GetObject<Text>("text_value");
                    Text_max = v.GetObject<Text>("text_max");
                }
                public void SetValue(int value, int max)
                {
                    Text_value.text = value.ToString();
                    Text_max.text = max.ToString();
                    Scrbar_condition.size = 1f * value/max;
                }

            }

            private class ElementManager
            {
                private Element_prop Strength { get; }
                private Element_prop Agility { get; }
                private Element_prop Hp { get; }
                private Element_prop Mp { get; }
                private Element_con Food { get; }
                private Element_con State { get; }
                private Element_con Injury { get; }
                private Element_con Inner { get; }
                private Element_con Silver { get; }
                public ElementManager(Element_prop strength, Element_prop agility, Element_prop hp, Element_prop mp,
                    Element_con food, Element_con state, Element_con injury, Element_con inner, Element_con silver)
                {
                    Strength = strength;
                    Agility = agility;
                    Hp = hp;
                    Mp = mp;
                    Food = food;
                    State = state;
                    Injury = injury;
                    Inner = inner;
                    Silver = silver;
                }

                private Element_prop GetPropUi(Props props) =>
                    props switch
                    {
                        Props.Strength => Strength,
                        Props.Agility => Agility,
                        Props.Hp => Hp,
                        Props.Mp => Mp,
                        _ => throw new ArgumentOutOfRangeException(nameof(props), props, null)
                    };
                private Element_con GetConditionUI(Conditions con) =>
                    con switch
                    {
                        Conditions.Food => Food,
                        Conditions.State => State,
                        Conditions.Injury => Injury,
                        Conditions.Inner => Inner,
                        Conditions.Silver => Silver,
                        _ => throw new ArgumentOutOfRangeException(nameof(con), con, null)
                    };

                public void SetConValue(Conditions co, int value, int max) => GetConditionUI(co).SetValue(value, max);

                public void SetPropValue(Props prop, int value) => GetPropUi(prop).SetText(value);
            }
        }
    }

    private class View_status : UiBase
    {
        private Button Btn_back { get; }
        private Text Text_status { get; }
        public View_status(IView v, Action onClickAction) : base(v, true)
        {
            Btn_back = v.GetObject<Button>("btn_back");
            Btn_back.OnClickAdd(onClickAction);
            Text_status = v.GetObject<Text>("text_status");
        }
        public void SetText(string status) => Text_status.text = status;
    }

    private class View_rewards : UiBase
    {
        private ScrollRect Scroll_reward { get; }
        private ListViewUi<RewardPrefab> RewardList { get; }
        public View_rewards(IView v) : base(v, true)
        {
            Scroll_reward = v.GetObject<ScrollRect>("scroll_reward");
            RewardList = new ListViewUi<RewardPrefab>(v.GetObject<View>("prefab_reward"), Scroll_reward);
        }

        private class RewardPrefab : UiBase
        {
            private Image Img_ico { get; }
            private Text Text_title { get; }
            private Button Btn_prefab { get; }
            public RewardPrefab(IView v, Action onClickAction) : base(v, true)
            {
                Img_ico = v.GetObject<Image>("img_ico");
                Text_title = v.GetObject<Text>("text_title");
                Btn_prefab = v.GetObject<Button>("btn_prefab");
                Btn_prefab.OnClickAdd(onClickAction);
            }
            public void SetIcon(Sprite icon) => Img_ico.sprite = icon;
            public void SetText(string title) => Text_title.text = title;
        }
    }
}