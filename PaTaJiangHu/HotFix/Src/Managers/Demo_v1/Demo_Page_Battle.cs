using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Models;
using System.Linq;
using System;
using Server.Controllers;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Systems.Messaging;

namespace HotFix_Project.Managers.Demo_v1;

internal class Demo_Page_Battle : PageUiManagerBase
{
    protected override string ViewName => "demo_page_battle";

    private Demo_v1Agent UiAgent { get; }

    public Demo_Page_Battle(Demo_v1Agent uiAgent) : base(uiAgent)
    {
        UiAgent = uiAgent;
    }

    private Game_BattleBanner view_battleBanner { get; set; }
    private View_BattleFinalize view_battleFinalize { get; set; }
    private Element_combatInfo element_combatInfoPlayer { get; set; }
    private Element_combatInfo element_combatInfoEnemy { get; set; }

    private BattleController BattleController => Game.Controllers.Get<BattleController>();

    protected override void Build(IView view)
    {
        view_battleBanner = new Game_BattleBanner(view.GetObject<View>("game_battleBanner"), true);
        view_battleFinalize =
            new View_BattleFinalize(view.GetObject<View>("view_battleFinalize"), BattleController.FinalizeBattle);
        element_combatInfoPlayer = new Element_combatInfo(view.GetObject<View>("element_combatInfoPlayer"), true);
        element_combatInfoEnemy = new Element_combatInfo(view.GetObject<View>("element_combatInfoEnemy"), true);
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Battle_Performer_update, bag =>
        {
            var info = bag.Get<DiziCombatInfo>(0);
            view_battleBanner.UpdateInfo(info);
        });
        Game.MessagingManager.RegEvent(EventString.Battle_Reponder_Update, bag =>
        {
            var info = bag.Get<DiziCombatInfo>(0);
            view_battleBanner.UpdateInfo(info);
        });
        Game.MessagingManager.RegEvent(EventString.Battle_Init, bag =>
        {
            var guid = bag.GetString(0);
            var diziInstanceId = bag.GetInt(1);
            var maxRound = bag.GetInt(2);
            Set(guid, diziInstanceId, maxRound);
        });
        Game.MessagingManager.RegEvent(EventString.Battle_RoundUpdate, bag =>
        {
            var round = bag.GetInt(0);
            var maxRound = bag.GetInt(1);
            RoundUpdate(round, maxRound);
        });

        Game.MessagingManager.RegEvent(EventString.Battle_SpecialUpdate,
            b => UpdateCombatUis());
        Game.MessagingManager.RegEvent(EventString.Battle_End, b =>
        {
            UpdateCombatUis();
            view_battleFinalize.Set(b.GetBool(0));
        });
        Game.MessagingManager.RegEvent(EventString.Battle_Finalized, _ =>
        {
            Reset();
            UiAgent.Redirect_MainPage_ChallengeSelector();
        });
    }

    private void UpdateCombatUis()
    {
        var dizi = Game.BattleCache.GetFighters(0).First();
        var npc = Game.BattleCache.GetFighters(1).First();
        element_combatInfoPlayer.Set(dizi);
        element_combatInfoEnemy.Set(npc);
        Show();
    }

    private void RoundUpdate(int round, int maxRound)
    {
        view_battleBanner.SetRound(round, maxRound);
        UpdateCombatUis();
    }

    private void Set(string guid, int diziInstanceId, int maxRound)
    {
        UpdateCombatUis();
        view_battleBanner.InitBattle(guid, diziInstanceId, maxRound);
    }

    private void Reset()
    {
        view_battleBanner.Reset();
        Hide();
    }

    private class Element_combatInfo : UiBase
    {
        private View_skill view_skill { get; set; }
        private View_equipment view_equipment { get; set; }
        private View_props view_props { get; set; }
        public Element_combatInfo(IView v, bool display) : base(v, display)
        {
            view_skill = new View_skill(v.GetObject<View>("view_skill"), true);
            view_equipment = new View_equipment(v.GetObject<View>("view_equipment"), true);
            view_props = new View_props(v.GetObject<View>("view_props"), true);
        }

        public void Set(IDiziCombatUnit unit)
        {
            view_skill.Set(unit);
            view_equipment.Set(unit);
            view_props.Set(unit);
        }

        private class View_skill : UiBase
        {
            private Element_skill element_skillCombat { get; }
            private Element_skill element_skillForce { get; }
            private Element_skill element_skillDodge { get; }

            public View_skill(IView v, bool display) : base(v, display)
            {
                element_skillCombat = new Element_skill(v.GetObject<View>("element_skillCombat"), true);
                element_skillForce = new Element_skill(v.GetObject<View>("element_skillForce"), true);
                element_skillDodge = new Element_skill(v.GetObject<View>("element_skillDodge"), true);
            }

            public void Set(IDiziCombatUnit unit)
            {
                var combatSkill = unit.CombatInfo;
                var forceSkill = unit.ForceInfo;
                var dodgeSkill = unit.DodgeInfo;
                element_skillCombat.Set(combatSkill.Level, combatSkill.Skill.Name, combatSkill.Skill.Icon);
                element_skillForce.Set(forceSkill.Level, forceSkill.Skill.Name, forceSkill.Skill.Icon);
                element_skillDodge.Set(dodgeSkill.Level, dodgeSkill.Skill.Name, dodgeSkill.Skill.Icon);
            }

            private class Element_skill : UiBase
            {
                private Text text_level { get; }
                private Image img_ico { get; }
                private Text text_name { get; }
                private GameObject obj_content { get; }

                public Element_skill(IView v, bool display) : base(v, display)
                {
                    text_level = v.GetObject<Text>("text_level");
                    img_ico = v.GetObject<Image>("img_ico");
                    text_name = v.GetObject<Text>("text_name");
                    obj_content = v.GetObject("obj_content");
                }

                public void Set(int level, string name, Sprite ico)
                {
                    text_level.text = level.ToString();
                    text_name.text = name;
                    img_ico.sprite = ico;
                    obj_content.SetActive(true);
                }
            }
        }

        private class View_equipment : UiBase
        {
            private Element_quip element_equipWeapon { get; }
            private Element_quip element_equipArmor { get; }
            private Element_quip element_equipShoes { get; }
            private Element_quip element_equipDecoration { get; }

            public View_equipment(IView v, bool display) : base(v, display)
            {
                element_equipWeapon = new Element_quip(v.GetObject<View>("element_equipWeapon"), true);
                element_equipArmor = new Element_quip(v.GetObject<View>("element_equipArmor"), true);
                element_equipShoes = new Element_quip(v.GetObject<View>("element_equipShoes"), true);
                element_equipDecoration = new Element_quip(v.GetObject<View>("element_equipDecoration"), true);
            }

            public void Set(IDiziCombatUnit dizi)
            {
                var weapon = dizi.Equipment.Weapon;
                if (weapon != null)
                    element_equipWeapon.Set(weapon.Name, weapon.About, weapon.Icon);
                element_equipWeapon.Display(weapon != null);
                var armor = dizi.Equipment.Armor;
                if (armor != null)
                    element_equipArmor.Set(armor.Name, armor.About, armor.Icon);
                element_equipArmor.Display(armor != null);
                var shoes = dizi.Equipment.Shoes;
                if (shoes != null)
                    element_equipShoes.Set(shoes.Name, shoes.About, shoes.Icon);
                element_equipShoes.Display(shoes != null);
                var decoration = dizi.Equipment.Decoration;
                if (decoration != null)
                    element_equipDecoration.Set(decoration.Name, decoration.About, decoration.Icon);
                element_equipDecoration.Display(decoration != null);
            }

            private class Element_quip : UiBase
            {
                private Text text_title { get; }
                private Text text_short { get; }
                private Image img_ico { get; }

                public Element_quip(IView v, bool display) : base(v, display)
                {
                    text_title = v.GetObject<Text>("text_title");
                    text_short = v.GetObject<Text>("text_short");
                    img_ico = v.GetObject<Image>("img_ico");
                }

                public void Set(string title, string shortDesc, Sprite ico)
                {
                    text_title.text = title;
                    text_short.text = shortDesc;
                    img_ico.sprite = ico;
                }

                public void Display(bool display)
                {
                    text_title.gameObject.SetActive(display);
                    text_short.gameObject.SetActive(display);
                    img_ico.gameObject.SetActive(display);
                }
            }
        }

        private class View_props : UiBase
        {
            private ListViewUi<Prefab_propInfo> PropListView { get; }

            public View_props(IView v, bool display) : base(v, display)
            {
                PropListView = new ListViewUi<Prefab_propInfo>(v.GetObject<View>("prefab_propInfo"),
                    v.GetObject<RectTransform>("tran_props"));
            }

            public void Set(IDiziCombatUnit dizi)
            {
                var combatSet = dizi.GetCombatSet();
                var self = CombatArgs.InstanceCombatUnit(dizi, true);
                var arg = CombatArgs.Instance(self, self, 0);
                var criDmgRatio = combatSet.GetCriticalDamageRatioAddOn(arg);
                var criRate = combatSet.GetCriticalRate(arg);
                var hrdDmgRatio = combatSet.GetHardDamageRatioAddOn(arg);
                var hrdRate = combatSet.GetHardRate(arg);
                var dodRate = combatSet.GetDodgeRate(arg);

                PropListView.ClearList(u => u.Destroy());

                SetList(PropListView, "重击率", $"{hrdRate:0.#}%");
                SetList(PropListView, "重击伤害倍数", $"{1 + hrdDmgRatio}");
                SetList(PropListView, "会心率", $"{criRate:0.#}%");
                SetList(PropListView, "会心伤害倍数", $"{1 + criDmgRatio}");
                SetList(PropListView, "闪避率", $"{dodRate:0.#}%");

                void SetList(ListBoardUi<Prefab_propInfo> list, string label, string value)
                {
                    var ui = list.Instance(v => new Prefab_propInfo(v, true));
                    ui.Set(label, value);
                }
            }

            private class Prefab_propInfo : UiBase
            {
                private Text text_label { get; }
                private Text text_value { get; }

                public Prefab_propInfo(IView v, bool display) : base(v, display)
                {
                    text_label = v.GetObject<Text>("text_label");
                    text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string label, string value)
                {
                    text_label.text = label;
                    text_value.text = value;
                    gameObject.SetActive(true);
                }
            }
        }
    }

    private class Game_BattleBanner : UiBase
    {
        private View_RoundInfo view_roundInfo { get; }
        private Element_CombatState element_combatStateLeft { get; }
        private Element_CombatState element_combatStateRight { get; }
        public Game_BattleBanner(IView v, bool display) : base(v,display )
        {
            view_roundInfo = new View_RoundInfo(v.GetObject<View>("view_roundInfo"));
            element_combatStateLeft = new Element_CombatState(v.GetObject<View>("element_combatStateLeft"));
            element_combatStateRight = new Element_CombatState(v.GetObject<View>("element_combatStateRight"));
        }

        private int CurrentDiziInstanceId { get; set; } = -1;
        private int NpcInstanceId { get; set; } = -1;
        private Dizi CurrentDizi { get; set; }
        public void InitBattle(string guid, int diziInstanceId, int maxRound)
        {
            CurrentDizi = Game.World.Faction.GetDizi(guid);
            var dizi = Game.BattleCache.GetDizi(guid);
            var npc = Game.BattleCache.GetFighters(1).FirstOrDefault();
            if (npc == null) throw new NullReferenceException("BattleCache 找不到 npc : teamId = 1");
            CurrentDiziInstanceId = diziInstanceId;
            NpcInstanceId = npc.InstanceId;
            SetRound(0, maxRound);
            Game.BattleCache.Avatars.TryGetValue(dizi, out var diziAvatar);
            Game.BattleCache.Avatars.TryGetValue(npc, out var npcAvatar);
            element_combatStateLeft.InitBattle(dizi, diziAvatar);
            element_combatStateRight.InitBattle(npc, npcAvatar);
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
            if (CurrentDiziInstanceId != info.InstanceId && NpcInstanceId != info.InstanceId) return;
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

            public void InitBattle(DiziCombatUnit dizi, Sprite icon)
            {
                SetInfo(dizi.Name, icon);
                SetProp(dizi.Strength.Value, dizi.Agility.Value);
                UpdateHp(dizi.Hp.Value, dizi.Hp.Max);
                UpdateMp(dizi.Mp.Value, dizi.Mp.Max);
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

            public void Reset() => Set(0, 0);
        }
        private class Element_Prop : UiBase
        {
            private Text text_value { get; }

            public Element_Prop(IView v) : base(v, true)
            {
                text_value = v.GetObject<Text>("text_value");
            }
            public void Set(int value) => text_value.text = value.ToString();
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
    private class View_BattleFinalize : UiBase
    {
        private Button btn_battleFinalize { get; }
        private Text text_win { get; }
        private Text text_lose { get; }
        public View_BattleFinalize(IView v, Action onclickAction) : base(v, false)
        {
            btn_battleFinalize = v.GetObject<Button>("btn_battleFinalize");
            text_win = v.GetObject<Text>("text_win");
            text_lose = v.GetObject<Text>("text_lose");
            btn_battleFinalize.OnClickAdd(() =>
            {
                onclickAction?.Invoke();
                Display(false);
            });
        }
        public void Set(bool win)
        {
            text_win.gameObject.SetActive(win);
            text_lose.gameObject.SetActive(!win);
            Display(true);
        }
    }
}