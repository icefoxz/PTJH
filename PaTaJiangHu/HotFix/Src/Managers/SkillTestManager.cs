using System;
using BattleM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

public class SkillTestManager
{
    private CombatLevelWindow CombatLevel { get; set; }
    private ISkillController Controller { get; set; }
    private SkillLevelWindow SkillLevel { get; set; }
    private enum MyEnum
    {
        Force,
        Dodge
    }
    private MyEnum SelectedSkill { get; set; }
    public void Init()
    {
        Controller = TestCaller.Instance.InstanceSkillController();
        InitUi();
        EventReg();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("test_testCombatSo",
            v => CombatLevel = new CombatLevelWindow(v,
                Controller.ListCombatSkills,
                Controller.SelectCombat,
                Controller.OnCombatLeveling, false));
        Game.UiBuilder.Build("test_testSkillSo",
            v => SkillLevel = new SkillLevelWindow(v,
                OnSkillSelected,
                OnSkillLeveling, false));
    }

    private void OnSkillLeveling(int level)
    {
        switch (SelectedSkill)
        {
            case MyEnum.Force: Controller.OnForceLeveling(level); break;
            case MyEnum.Dodge: Controller.OnDodgeLeveling(level); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void OnSkillSelected(int index)
    {
        switch (SelectedSkill)
        {
            case MyEnum.Force: Controller.SelectForce(index); break;
            case MyEnum.Dodge: Controller.SelectDodge(index); break;
            default: throw new ArgumentOutOfRangeException();
        }
    }

    private void EventReg()
    {
        Game.MessagingManager.RegEvent(EventString.Test_CombatSoList, bag =>
        {
            CombatLevel.ListCombats(bag.Get<SkillController.Combat[]>(0));
            CombatLevel.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_CombatSkillLeveling,
            bag => CombatLevel.OnCombatLeveling(bag.Get<SkillController.Combat>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_CombatSkillSelected,
            bag => CombatLevel.OnSelectedCombat(bag.Get<SkillController.Combat>(0)));

        Game.MessagingManager.RegEvent(EventString.Test_ForceSoList, bag =>
        {
            SelectedSkill = MyEnum.Force;
            SkillLevel.ResetUi();
            SkillLevel.ListForce(bag.Get<SkillController.Force[]>(0));
            SkillLevel.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_DodgeSoList, bag =>
        {
            SelectedSkill = MyEnum.Dodge;
            SkillLevel.ResetUi();
            SkillLevel.ListDodge(bag.Get<SkillController.DodgeSkill[]>(0));
            SkillLevel.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_DodgeSkillLeveling,
            bag => SkillLevel.DodgeUpdate(bag.Get<SkillController.DodgeSkill>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_DodgeSkillSelected,
            bag => SkillLevel.DodgeUpdate(bag.Get<SkillController.DodgeSkill>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_ForceSkillLeveling,
            bag => SkillLevel.ForceUpdate(bag.Get<SkillController.Force>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_ForceSkillSelected,
            bag => SkillLevel.ForceUpdate(bag.Get<SkillController.Force>(0)));
    }

    private class CombatLevelWindow : UiBase
    {
        private event Action<int> OnSelectCombat;
        private Button Btn_load { get; }
        private Button Btn_back { get; }
        private LevelingUi Leveling { get; }
        private BuffViewUi BuffView { get; }
        private ComboViewUi ComboView { get; }
        private CombatViewUi CombatView { get; }
        private PropViewUi PropView { get; }
        private CombatTitleUi CombatTitle { get; }

        public CombatLevelWindow(IView v, 
            Action onListCombats, 
            Action<int> onCombatSelected, 
            Action<int> onLeveling,
            bool display) : base(v.GameObject, display)
        {
            OnSelectCombat = onCombatSelected;
            Btn_load = v.GetObject<Button>("btn_load");
            Btn_back = v.GetObject<Button>("btn_back");
            Leveling = new LevelingUi(v.GetObject<View>("view_leveling"),
                () => onLeveling(LevelUp()),
                () => onLeveling(LevelDown()));
            BuffView = new BuffViewUi(v, display);
            ComboView = new ComboViewUi(v, display);
            CombatView = new CombatViewUi(v, display);
            PropView = new PropViewUi(v, v.GetObject("trans_prop"), display);
            CombatTitle = new CombatTitleUi(v.GetObject<View>("view_title"));
            Btn_back.OnClickAdd(() =>
            {
                onListCombats();
                Btn_back.gameObject.SetActive(false);
                Btn_load.gameObject.SetActive(true);
                Leveling.Display(false);
            });
            Btn_back.gameObject.SetActive(false);
            CombatTitle.Display(false);
            Leveling.Display(false);
        }

        public void ListCombats(SkillController.Combat[] combats)
        {
            if (combats == null || combats.Length == 0)
                throw new NotImplementedException("找不到武功配置 ，请设置武功配置！");
            CombatView.ClearList();
            PropView.ClearList();
            BuffView.ClearList();
            ComboView.ClearList();
            CombatTitle.Display(false);
            for (var i = 0; i < combats.Length; i++)
            {
                var combat = combats[i];
                var index = i;
                CombatView.AddCombat(combat, () => OnSelectCombat?.Invoke(index));
            }
        }

        public void OnSelectedCombat(SkillController.Combat combat)
        {
            PropView.ClearList();
            BuffView.ClearList();
            foreach (var form in combat.Combats)
                PropView.AddUi(form.Name, $"息:{form.Breath},Buff[{form.Buffs?.Length ?? 0}]");
            foreach (var exert in combat.Exerts)
            {
                var text = exert.Activity switch
                {
                    IPerform.Activities.Attack => "进攻技",
                    IPerform.Activities.Recover => "恢复技",
                    IPerform.Activities.Auto => throw new NotImplementedException(),
                    _ => throw new ArgumentOutOfRangeException()
                };
                PropView.AddUi(text, exert.Name);
            }
            Btn_load.OnClickAdd(() => CombatLoad(combat));

            void CombatLoad(SkillController.Combat cb)
            {
                CombatTitle.SetTitle(cb.Name);
                CombatView.ClearList();
                PropView.ClearList();
                SetForms(cb.Combats);
                Level = 0;
                Leveling.InitUi();
                Btn_back.gameObject.SetActive(true);
            }
        }

        private void SetForms(SkillController.Combat.Form[] forms)
        {
            Btn_load.gameObject.SetActive(false);
            foreach (var form in forms) CombatView.AddForm(form, () => FormSelected(form));

            void FormSelected(SkillController.Combat.Form form)
            {
                PropView.ClearList();
                PropView.AddUi("息", form.Breath);
                PropView.AddUi("攻内", form.CombatMp);
                PropView.AddUi("招架", form.Parry);
                PropView.AddUi("招架内消", form.ParryMp);
                PropView.AddUi("己硬直", form.OffBusy);
                PropView.AddUi("敌硬直", form.TarBusy);
                PropView.AddUi("敌削内", form.DamageMp);
                ComboView.ClearList();
                var combo = form.Combo;
                if (combo != null)
                    for (var i = 0; i < combo.Rates.Length; i++)
                    {
                        var rate = combo.Rates[i];
                        ComboView.AddUi(i, rate);
                    }

                BuffView.ClearList();
                foreach (var buff in form.Buffs) AddBuffUi(buff);
            }
        }

        private void AddBuffUi(SkillController.Buff buff)
        {
            var (title, text) = GetBuffUiText(buff);
            BuffView.AddUi(title, text);
        }

        public void OnCombatLeveling(SkillController.Combat combat)
        {
            CombatView.ClearList();
            PropView.ClearList();
            SetForms(combat.Combats);
        }

        #region Leveling
        private int Level { get; set; }
        private int LevelDown()
        {
            Level--;
            if (Level <= 0)
                Level = 0;
            Leveling.SetLevelUi(Level);
            return Level;
        }
        private int LevelUp()
        {
            Leveling.SetLevelUi(++Level);
            return Level;
        }
        #endregion

        private class CombatTitleUi : UiBase
        {
            private Text Text_combat { get; }

            public CombatTitleUi(IView v) : base(v.GameObject, false)
            {
                Text_combat = v.GetObject<Text>("text_combat");
            }

            public void SetTitle(string title)
            {
                Text_combat.text = title;
                Display(true);
            }
        }
        //等级栏
        private class LevelingUi : UiBase
        {
            private Button Btn_addLevel { get; }
            private Button Btn_subLevel { get; }
            private Text Text_level { get; }

            public LevelingUi(IView v, Action upLevelAction, Action downLevelAction) : base(v.GameObject, true)
            {
                Btn_addLevel = v.GetObject<Button>("btn_addLevel");
                Btn_addLevel.OnClickAdd(upLevelAction);
                Btn_subLevel = v.GetObject<Button>("btn_subLevel");
                Btn_subLevel.OnClickAdd(downLevelAction);
                Text_level = v.GetObject<Text>("text_level");
            }

            public void SetLevelUi(int level)
            {
                Text_level.text = level.ToString();
            }

            public void InitUi()
            {
                Text_level.text = 0.ToString();
                Display(true);
            }
        }
        //buff栏
        private class BuffViewUi : UiBase
        {
            private ScrollRect Scroll_buff { get; }
            private ListViewUi<BuffUi> BuffView { get; }

            public BuffViewUi(IView v,bool display) : base(v.GameObject, display)
            {
                Scroll_buff = v.GetObject<ScrollRect>("scroll_buff");
                BuffView = new ListViewUi<BuffUi>(v.GetObject<View>("prefab_buff"), Scroll_buff.content.gameObject);
            }
            private class BuffUi : UiBase
            {
                private Text Text_title { get; }
                private Text Text_value { get; }

                public BuffUi(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string title, object value)
                {
                    Text_title.text = title;
                    Text_value.text = value.ToString();
                }
            }
            public void ClearList() => BuffView.ClearList(ui => ui.Destroy());

            public void AddUi(string title, object value)
            {
                var ui = BuffView.Instance(v => new BuffUi(v));
                ui.Set(title, value);
            }
        }
        //连击栏
        private class ComboViewUi:UiBase
        {
            private ScrollRect Scroll_combo { get; }
            private ListViewUi<ComboUi> ComboView { get; }
            public ComboViewUi(IView v, bool display) : base(v.GameObject, display)
            {
                Scroll_combo = v.GetObject<ScrollRect>("scroll_combo");
                ComboView = new ListViewUi<ComboUi>(v.GetObject<View>("prefab_combo"), Scroll_combo.content.gameObject);
            }
            private class ComboUi : UiBase
            {
                private Text Text_title { get; }
                private Text Text_value { get; }
                public ComboUi(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string title, string value)
                {
                    Text_title.text = title;
                    Text_value.text = value;
                }
            }

            public void ClearList() => ComboView.ClearList(ui => ui.Destroy());

            public void AddUi(int index,int rate)
            {
                var ui = ComboView.Instance(v => new ComboUi(v));
                ui.Set($"{index + 1}连击", $"{rate}%伤害");
            }
        }
        //属性栏
        private class PropViewUi : UiBase
        {
            private ListViewUi<PropUi> PropView { get; }
            public PropViewUi(IView v, GameObject contentObj, bool display) : base(v.GameObject, display)
            {
                PropView = new ListViewUi<PropUi>(v.GetObject<View>("prefab_prop"), contentObj);
            }

            public void ClearList() => PropView.ClearList(p => p.Destroy());
            private class PropUi:UiBase
            {
                private Text Text_title { get; }
                private Text Text_value { get; }
                public PropUi(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string title, string value)
                {
                    Text_title.text = title;
                    Text_value.text = value;
                }
            }

            public void AddUi(string title, object value)
            {
                var ui = PropView.Instance(v => new PropUi(v));
                ui.Set(title, value.ToString());
            }
        }
        //技能栏
        private class CombatViewUi : UiBase
        {
            private ScrollRect ScrollRect { get; }
            private ListViewUi<CombatPrefab> CombatListView { get; }
            public CombatViewUi(IView v, bool display) : base(v.GameObject, display)
            {
                ScrollRect = v.GetObject<ScrollRect>("scroll_combat");
                CombatListView =
                    new ListViewUi<CombatPrefab>(v.GetObject<View>("prefab_combat"), ScrollRect.content.gameObject);
            }

            private class CombatPrefab : UiBase
            {
                private Text Text_title { get; }
                private Button Btn_combat { get; }
                private Outline Selected { get; }
                public CombatPrefab(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_combat = v.GetObject<Button>("btn_combat");
                    Selected = Btn_combat.GetComponent<Outline>();
                }

                public void SetTitle(string title,Action onSelectedAction)
                {
                    Text_title.text = title;
                    Btn_combat.OnClickAdd(onSelectedAction);
                }
                public void SetSelected(bool selected) => Selected.enabled = selected;
            }

            public void AddForm(SkillController.Combat.Form form, Action onclickAction)
            {
                var ui = CombatListView.Instance(v => new CombatPrefab(v));
                ui.SetTitle(form.Name, () =>
                {
                    SetUiSelected(ui);
                    onclickAction();
                });
                ui.SetSelected(false);
            }

            private void SetUiSelected(CombatPrefab selected)
            {
                foreach (var prefab in CombatListView.List) prefab.SetSelected(prefab == selected);
            }

            public void ClearList()=>CombatListView.ClearList(u=>u.Destroy());

            public void AddCombat(SkillController.Combat combat, Action onclickAction)
            {
                var ui = CombatListView.Instance(v => new CombatPrefab(v));
                ui.SetTitle(combat.Name, () =>
                {
                    SetUiSelected(ui);
                    onclickAction();
                });
                ui.SetSelected(false);
            }
        }
    }

    private static (string title, string text) GetBuffUiText(SkillController.Buff buff)
    {
        var append = buff.Append switch
        {
            ICombatBuff.Appends.Self => "自身",
            ICombatBuff.Appends.TargetForce => "敌强制",
            ICombatBuff.Appends.TargetIfHit => "敌击中",
            _ => throw new ArgumentOutOfRangeException()
        };
        var consumption = buff.Consumption switch
        {
            ICombatBuff.Consumptions.Round => $"回({buff.Lasting})",
            ICombatBuff.Consumptions.Consume => $"次({buff.Lasting})",
            _ => throw new ArgumentOutOfRangeException()
        };
        var title = $"{append}{buff.SoName}";
        var text = $"{consumption},叠[{buff.Stacks}]";
        return (title, text);
    }

    private class SkillLevelWindow : UiBase
    {
        private SkillViewUi SkillView { get; }
        private BuffViewUi BuffView { get; }
        private PropViewUi PropView { get; }
        private LevelingUi Leveling { get; }

        private event Action<int> OnSelectSkill;
        public SkillLevelWindow(IView v, Action<int> onSkillSelected,
            Action<int> levelingAction, bool display) : base(v.GameObject, display)
        {
            OnSelectSkill = onSkillSelected;
            SkillView = new SkillViewUi(v, display);
            BuffView = new BuffViewUi(v, display);
            PropView = new PropViewUi(v, v.GetObject("trans_prop"), display);
            Leveling = new LevelingUi(v.GetObject<View>("view_leveling"),
                () => levelingAction(LevelUp()),
                () => levelingAction(LevelDown()));
        }

        public void ListForce(SkillController.Force[] forces)
        {
            Leveling.Display(false);
            SkillView.ClearList();
            for (var i = 0; i < forces.Length; i++)
            {
                var force = forces[i];
                var index = i;
                SkillView.AddForce(force, () =>
                {
                    OnSelectSkill?.Invoke(index);
                    Leveling.InitUi();
                });
            }
        }

        public void ListDodge(SkillController.DodgeSkill[] dodges)
        {
            Leveling.Display(false);
            SkillView.ClearList();
            for (var i = 0; i < dodges.Length; i++)
            {
                var dodge = dodges[i];
                var index = i;
                SkillView.AddDodge(dodge, () =>
                {
                    OnSelectSkill?.Invoke(index);
                    Leveling.InitUi();
                });
            }
        }

        public void DodgeUpdate(SkillController.DodgeSkill dodge)
        {
            MaxLevel = dodge.MaxLevel;
            PropView.ClearList();
            PropView.AddUi("息", dodge.Breath);
            PropView.AddUi("身法值", dodge.Dodge);
            PropView.AddUi("内耗", dodge.DodgeMp);
            UpdateBuffs(dodge.Buffs);
        }

        public void ForceUpdate(SkillController.Force force)
        {
            MaxLevel = force.MaxLevel;
            PropView.ClearList();
            PropView.AddUi("息", force.Breath);
            PropView.AddUi("内功转化", force.ForceRate);
            PropView.AddUi("恢复值", force.Recover);
            PropView.AddUi("蓄转内", force.MpCharge);
            PropView.AddUi("护甲", force.Armor);
            PropView.AddUi("护甲消耗", force.ArmorCost);
            UpdateBuffs(force.Buffs);
        }

        private void UpdateBuffs(SkillController.Buff[] buffs)
        {
            BuffView.ClearList();
            foreach (var buff in buffs)
            {
                var (title, text) = GetBuffUiText(buff);
                BuffView.AddUi(title, text);
            }
        }

        #region Leveling
        private int MaxLevel { get; set; }
        private int Level { get; set; }
        private int LevelDown()
        {
            Level--;
            if (Level <= 0)
                Level = 0;
            Leveling.SetLevelUi(Level);
            return Level;
        }
        private int LevelUp()
        {
            Level++;
            if (Level > MaxLevel)
                Level = MaxLevel;
            Leveling.SetLevelUi(Level);
            return Level;
        }
        #endregion

        public void ResetUi()
        {
            Level = 0;
            SkillView.ClearList();
            BuffView.ClearList();
            PropView.ClearList();
            Leveling.ResetUi();
        }
        //技能栏
        private class SkillViewUi : UiBase
        {
            private ScrollRect ScrollRect { get; }
            private ListViewUi<CombatPrefab> CombatListView { get; }
            public SkillViewUi(IView v,bool display) : base(v.GameObject, display)
            {
                ScrollRect = v.GetObject<ScrollRect>("scroll_skill");
                CombatListView =
                    new ListViewUi<CombatPrefab>(v.GetObject<View>("prefab_skill"), ScrollRect.content.gameObject);
            }

            private class CombatPrefab : UiBase
            {
                private Text Text_title { get; }
                private Button Btn_combat { get; }
                private Outline Selected { get; }
                public CombatPrefab(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Btn_combat = v.GetObject<Button>("btn_combat");
                    Selected = Btn_combat.GetComponent<Outline>();
                }

                public void SetTitle(string title, Action onSelectedAction)
                {
                    Text_title.text = title;
                    Btn_combat.OnClickAdd(onSelectedAction);
                }
                public void SetSelected(bool selected) => Selected.enabled = selected;
            }

            public void AddForce(SkillController.Force force, Action onclickAction)
            {
                var ui = CombatListView.Instance(v => new CombatPrefab(v));
                ui.SetTitle(force.Name, () =>
                {
                    SetUiSelected(ui);
                    onclickAction();
                });
                ui.SetSelected(false);
            }

            public void AddDodge(SkillController.DodgeSkill dodge, Action onclickAction)
            {
                var ui = CombatListView.Instance(v => new CombatPrefab(v));
                ui.SetTitle(dodge.Name, () =>
                {
                    SetUiSelected(ui);
                    onclickAction();
                });
                ui.SetSelected(false);
            }

            private void SetUiSelected(CombatPrefab selected)
            {
                foreach (var prefab in CombatListView.List) prefab.SetSelected(prefab == selected);
            }

            public void ClearList() => CombatListView.ClearList(u => u.Destroy());

        }
        //buff栏
        private class BuffViewUi : UiBase
        {
            private ScrollRect Scroll_buff { get; }
            private ListViewUi<BuffUi> BuffView { get; }

            public BuffViewUi(IView v, bool display) : base(v.GameObject, display)
            {
                Scroll_buff = v.GetObject<ScrollRect>("scroll_buff");
                BuffView = new ListViewUi<BuffUi>(v.GetObject<View>("prefab_buff"), Scroll_buff.content.gameObject);
            }
            private class BuffUi : UiBase
            {
                private Text Text_title { get; }
                private Text Text_value { get; }

                public BuffUi(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string title, object value)
                {
                    Text_title.text = title;
                    Text_value.text = value.ToString();
                }
            }
            public void ClearList() => BuffView.ClearList(ui => ui.Destroy());

            public void AddUi(string title, object value)
            {
                var ui = BuffView.Instance(v => new BuffUi(v));
                ui.Set(title, value);
            }
        }
        //属性栏
        private class PropViewUi : UiBase
        {
            private ListViewUi<PropUi> PropView { get; }
            public PropViewUi(IView v, GameObject contentObj, bool display) : base(v.GameObject, display)
            {
                PropView = new ListViewUi<PropUi>(v.GetObject<View>("prefab_prop"), contentObj);
            }

            public void ClearList() => PropView.ClearList(p => p.Destroy());
            private class PropUi : UiBase
            {
                private Text Text_title { get; }
                private Text Text_value { get; }
                public PropUi(IView v) : base(v.GameObject, true)
                {
                    Text_title = v.GetObject<Text>("text_title");
                    Text_value = v.GetObject<Text>("text_value");
                }

                public void Set(string title, string value)
                {
                    Text_title.text = title;
                    Text_value.text = value;
                }
            }

            public void AddUi(string title, object value)
            {
                var ui = PropView.Instance(v => new PropUi(v));
                ui.Set(title, value.ToString());
            }
        }
        //等级栏
        private class LevelingUi : UiBase
        {
            private Button Btn_addLevel { get; }
            private Button Btn_subLevel { get; }
            private Text Text_level { get; }

            public LevelingUi(IView v, Action upLevelAction, Action downLevelAction) : base(v.GameObject, true)
            {
                Btn_addLevel = v.GetObject<Button>("btn_addLevel");
                Btn_addLevel.OnClickAdd(upLevelAction);
                Btn_subLevel = v.GetObject<Button>("btn_subLevel");
                Btn_subLevel.OnClickAdd(downLevelAction);
                Text_level = v.GetObject<Text>("text_level");
            }

            public void SetLevelUi(int level) => Text_level.text = level.ToString();

            public void InitUi()
            {
                Text_level.text = 0.ToString();
                Display(true);
            }

            public void ResetUi()
            {
                Text_level.text = 0.ToString();
                Display(false);
            }
        }
    }

}