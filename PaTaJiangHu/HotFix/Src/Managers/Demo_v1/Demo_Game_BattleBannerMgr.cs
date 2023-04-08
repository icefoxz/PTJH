using System;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

internal class Demo_Game_BattleBannerMgr : MainPageBase
{
    private Game_BattleBanner View_BattleBanner { get; set; }
    private Demo_v1Agent UiAgent { get; }
    public Demo_Game_BattleBannerMgr(Demo_v1Agent uiAgent) : base(uiAgent)
    {
        UiAgent = uiAgent;
    }

    protected override string ViewName => "demo_game_battleBanner";
    protected override bool IsDynamicPixel => true;
    protected override void Build(IView view) => View_BattleBanner = new Game_BattleBanner(view);
    protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Game;

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Battle_Performer_update, bag =>
        {
            var info = bag.Get<DiziCombatInfo>(0);
            View_BattleBanner.UpdateInfo(info);
        });

        Game.MessagingManager.RegEvent(EventString.Battle_Reponder_Update, bag =>
        {
            var info = bag.Get<DiziCombatInfo>(0);
            View_BattleBanner.UpdateInfo(info);
        });

        Game.MessagingManager.RegEvent(EventString.Battle_Init, bag =>
        {
            var guid = bag.GetString(0);
            var diziInstanceId = bag.GetInt(1);
            var maxRound = bag.GetInt(2);
            View_BattleBanner.InitBattle(guid, diziInstanceId, maxRound);
        });

        Game.MessagingManager.RegEvent(EventString.Battle_RoundUpdate, bag =>
        {
            var round = bag.GetInt(0);
            var maxRound = bag.GetInt(1);
            View_BattleBanner.SetRound(round, maxRound);
        });
    }

    public void Reset() => View_BattleBanner.Reset();

    public override void Show() => View_BattleBanner.Display(true);

    public override void Hide() => View_BattleBanner.Display(false);

    private class Game_BattleBanner : UiBase
    {
        private View_RoundInfo view_roundInfo { get; }
        private Element_CombatState element_combatStateLeft { get; }
        private Element_CombatState element_combatStateRight { get; }
        public Game_BattleBanner(IView v) : base(v, false)
        {
            view_roundInfo = new View_RoundInfo(v.GetObject<View>("view_roundInfo"));
            element_combatStateLeft = new Element_CombatState(v.GetObject<View>("element_combatStateLeft"));
            element_combatStateRight = new Element_CombatState(v.GetObject<View>("element_combatStateRight"));
        }

        private int CurrentDiziInstanceId { get; set; } = -1;
        private int NpcInstanceId { get; set; } = -1;
        private Dizi CurrentDizi { get; set; }
        public void InitBattle(string guid, int diziInstanceId,int maxRound)
        {
            CurrentDizi = Game.World.Faction.GetDizi(guid);
            var dizi = Game.BattleCache.GetDizi(guid);
            var npc = Game.BattleCache.GetFighters(1).FirstOrDefault();
            if (npc == null) throw new NullReferenceException("BattleCache 找不到 npc : teamId = 1");
            CurrentDiziInstanceId = diziInstanceId;
            NpcInstanceId = npc.InstanceId;
            SetRound(0, maxRound);
            element_combatStateLeft.InitBattle(dizi);
            element_combatStateRight.InitBattle(npc);
            Display(true);
        }

        public void Reset()
        {
            CurrentDizi = null;
            CurrentDiziInstanceId = -1;
            NpcInstanceId = -1;
            element_combatStateLeft.Reset();
            element_combatStateRight.Reset();
            SetRound(0, 0);
            Display(false);
        }

        public void UpdateInfo(DiziCombatInfo info)
        {
            if (CurrentDiziInstanceId != info.InstanceId || NpcInstanceId != info.InstanceId) return;
            if (info.TeamId == 0)
                element_combatStateLeft.UpdateInfo(info);
            else
                element_combatStateRight.UpdateInfo(info);
        }
        public void SetRound(int round, int maxRound) => view_roundInfo.Set(round, maxRound);

        private class Element_CombatState : UiBase
        {
            private View_Avatar view_avatar { get; }
            private Scrollbar scrbar_statusHp { get; }
            private Scrollbar scrbar_statusMp { get; }
            private Element_Status element_statusHp { get; }
            private Element_Status element_statusMp { get; }
            private Element_Prop element_prop_strength { get; }
            private Element_Prop element_prop_agility { get; }

            public Element_CombatState(IView v) : base(v, true)
            {
                view_avatar = new View_Avatar(v.GetObject<View>("view_avatar"));
                scrbar_statusHp = v.GetObject<Scrollbar>("scrbar_statusHp");
                scrbar_statusMp = v.GetObject<Scrollbar>("scrbar_statusMp");
                element_statusHp = new Element_Status(v.GetObject<View>("element_statusHp"));
                element_statusMp = new Element_Status(v.GetObject<View>("element_statusMp"));
                element_prop_strength = new Element_Prop(v.GetObject<View>("element_prop_strength"));
                element_prop_agility = new Element_Prop(v.GetObject<View>("element_prop_agility"));
            }

            private void SetInfo(string name, Sprite avatar = null)
            {
                view_avatar.SetName(name);
                if (avatar != null) view_avatar.SetAvatar(avatar);
            }
            private void UpdateHp(int hp, int max)
            {
                element_statusHp.Set(hp, max);
                scrbar_statusHp.size = 1f * hp / max;
            }
            private void UpdateMp(int mp, int max)
            {
                element_statusMp.Set(mp, max);
                scrbar_statusMp.size = 1f * mp / max;
            }
            private void SetProp(int strength, int agility)
            {
                element_prop_strength.Set(strength);
                element_prop_agility.Set(agility);
            }

            public void Reset()
            {
                element_statusHp.Reset();
                element_statusMp.Reset();
                scrbar_statusHp.size = 1;
                scrbar_statusMp.size = 1;
                SetInfo(string.Empty);
                SetProp(0, 0);
            }

            public void InitBattle(DiziCombatUnit dizi)
            {
                SetInfo(dizi.Name);
                SetProp(dizi.Strength, dizi.Agility);
                UpdateHp(dizi.Hp, dizi.Hp);
                UpdateMp(dizi.Mp, dizi.Mp);
            }

            public void UpdateInfo(DiziCombatInfo info)
            {
                UpdateHp(info.Hp, info.MaxHp);
                UpdateMp(info.Mp, info.MaxMp);
            }
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

            public void Reset() => Set(0,0);
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
    }
}