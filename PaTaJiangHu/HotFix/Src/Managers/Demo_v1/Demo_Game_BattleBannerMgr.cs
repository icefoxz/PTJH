using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

internal class Demo_Game_BattleBannerMgr : UiManagerBase
{
    private Game_BattleBanner View_BattleBanner { get; set; }
    private Demo_v1Agent UiAgent { get; }
    public Demo_Game_BattleBannerMgr(Demo_v1Agent uiAgent) : base(uiAgent)
    {
        UiAgent = uiAgent;
    }

    protected override MainUiAgent.Sections Section { get; }
    protected override string ViewName { get; } = "demo_game_battleBanner";
    protected override bool IsDynamicPixel { get; } = true;
    protected override void Build(IView view) => View_BattleBanner = new Game_BattleBanner(view);

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Battle_Performer_update, bag =>
        {
            var info = bag.Get<CombatUnitInfo<DiziCombatUnit>>(0);
            View_BattleBanner.UpdateInfo(info);
        });

        Game.MessagingManager.RegEvent(EventString.Battle_Reponser_Update, bag =>
        {
            var info = bag.Get<CombatUnitInfo<DiziCombatUnit>>(0);
            View_BattleBanner.UpdateInfo(info);
        });
    }

    public override void Show() => View_BattleBanner.Display(true);

    public override void Hide() => View_BattleBanner.Display(false);

    private class Game_BattleBanner : UiBase
    {
        private View_Avatar view_avatar { get; }
        private Scrollbar scrbar_statusHp { get; }
        private Scrollbar scrbar_statusMp { get; }
        private Element_Status element_statusHp { get; }
        private Element_Status element_statusMp { get; }
        private Element_Prop element_prop_strength { get; }
        private Element_Prop element_prop_agility { get; }
        private View_RoundInfo view_roundInfo { get; }
        public Game_BattleBanner(IView v) : base(v, true)
        {
            view_avatar = new View_Avatar(v.GetObject<View>("view_avatar"));
            scrbar_statusHp = v.GetObject<Scrollbar>("scrbar_statusHp");
            scrbar_statusMp = v.GetObject<Scrollbar>("scrbar_statusMp");
            element_statusHp = new Element_Status(v.GetObject<View>("element_statusHp"));
            element_statusMp = new Element_Status(v.GetObject<View>("element_statusMp"));
            element_prop_strength = new Element_Prop(v.GetObject<View>("element_prop_strength"));
            element_prop_agility = new Element_Prop(v.GetObject<View>("element_prop_agility"));
            view_roundInfo = new View_RoundInfo(v.GetObject<View>("view_roundInfo"));
        }

        public void SetInfo(string name,Sprite avatar = null)
        {
            view_avatar.SetName(name);
            if (avatar != null) view_avatar.SetAvatar(avatar);
        }
        public void SetRound(int round, int maxRound) => view_roundInfo.Set(round, maxRound);
        public void UpdateHp(int hp,int max)
        {
            element_statusHp.Set(hp, max);
            scrbar_statusHp.value = 1f * hp / max;
        }
        public void UpdateMp(int mp, int max)
        {
            element_statusMp.Set(mp, max);
            scrbar_statusMp.value = 1f * mp / max;
        }
        public void SetProp(int strength, int agility)
        {
            element_prop_strength.Set(strength);
            element_prop_agility.Set(agility);
        }
        private class View_Avatar : UiBase
    {
        private Image img_avatar { get; }
        private Text text_name { get; }

        public View_Avatar(IView v) : base(v, true)
        {
            img_avatar = v.GetObject<Image>("img_avatar");
            text_name = v.GetObject<Text>("text_name");
        }

        public void SetName(string name) => text_name.text = name;
        public void SetAvatar(Sprite sprite) => img_avatar.sprite = sprite;
    }
        private class Element_Status : UiBase
        {
            private Text text_statusValue { get; }
            private Text text_statusMax { get; }
            public Element_Status(IView v) : base(v, true)
            {
                text_statusValue = v.GetObject<Text>("text_statusValue");
                text_statusMax = v.GetObject<Text>("text_statusMax");
            }

            public void Set(int value, int max)
            {
                text_statusValue.text = value.ToString();
                text_statusMax.text = max.ToString();
            }
        }
        private class Element_Prop : UiBase
        {
            private Text text_value { get; }

            public Element_Prop(IView v) : base(v, true)
            {
                text_value = v.GetObject<Text>("text_value");
            }
            public void Set(int value)=> text_value.text = value.ToString();
        }
        private class View_RoundInfo : UiBase
        {
            private Text text_value { get; }
            private Text text_max { get; }
            public View_RoundInfo(IView v) : base(v, true)
            {
                text_value = v.GetObject<Text>("text_value");
                text_max = v.GetObject<Text>("text_max");
            }

            public void Set(int value, int max)
            {
                text_value.text = value.ToString();
                text_max.text = max.ToString();
            }
        }

        public void UpdateInfo(CombatUnitInfo<DiziCombatUnit> info)
        {
            UpdateHp(info.Hp,info.MaxHp);
            //UpdateMp(info.mp);
        }
    }
}