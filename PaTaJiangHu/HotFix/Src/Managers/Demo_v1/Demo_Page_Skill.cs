using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Models;
using Server.Configs.Skills;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

internal class Demo_Page_Skill : UiManagerBase
{
    private Demo_v1Agent Agent { get; set; }
    private View_diziInfo view_diziInfo { get; set; }
    private View_diziProps view_diziProps { get; set; }
    private View_tags view_tags { get; set; }
    private View_skillDev view_skillDev { get; set; }
    private View_DiziList view_diziList { get; set; }

    public Demo_Page_Skill(Demo_v1Agent uiAgent) : base(uiAgent)
    {
        Agent = uiAgent;
    }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Page;
    protected override string ViewName => "demo_page_skill";
    protected override bool IsDynamicPixel => true;

    protected override void Build(IView v)
    {
        view_diziInfo = new View_diziInfo(v.GetObject<View>("view_diziInfo"), true);
        view_diziProps = new View_diziProps(v.GetObject<View>("view_diziProps"), true);
        view_tags = new View_tags(v.GetObject<View>("view_tags"), true);
        view_skillDev = new View_skillDev(v.GetObject<View>("view_skillDev"),
            (s, i) =>
            {
                SkillSelected(s, i);
            },
            (guid, skill, index) =>
            {
                Agent.SetBookComprehend(guid, skill, index);
            },
            Evolve,
            SkillUses,
            SkillForget,
            () => Agent.SetSkillComprehend(_selectedDizi.Guid, _selectedSkill, _selectedIndex),
            true);
        view_diziList = new View_DiziList(v.GetObject<View>("view_diziList"),
            guid => { Agent.SetDiziView(guid, Demo_v1Agent.Pages.Skills); });
    }

    private SkillType _selectedSkill;
    private int _selectedIndex;
    private Dizi _selectedDizi;

    private void SkillSelected(SkillType skill, int index)
    {
        _selectedSkill = skill;
        _selectedIndex = skill switch
        {
            SkillType.Combat => ResolveIndex(_selectedDizi.Skill.CombatSkills.Count, index),
            SkillType.Force => ResolveIndex(_selectedDizi.Skill.ForceSkills.Count, index),
            SkillType.Dodge => ResolveIndex(_selectedDizi.Skill.DodgeSkills.Count, index),
            _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
        };
        view_skillDev.UpdateSkill(_selectedDizi, _selectedSkill, _selectedIndex);

        int ResolveIndex(int count, int i) => count > i ? i : -1;
    }

    private void SkillForget()
    {
        var controller = Game.Controllers.Get<SkillController>();
        controller.ForgetSkill(_selectedDizi.Guid, _selectedSkill, _selectedIndex);
        Debug.Log($"SkillForget: {_selectedSkill} {_selectedIndex}");
    }

    private void SkillUses()
    {
        var skillController = Game.Controllers.Get<SkillController>();
        skillController.UseSkill(_selectedDizi.Guid, _selectedSkill, _selectedIndex);
    }

    private void Evolve()
    {
        Debug.LogError($"不支持进阶: {_selectedSkill}   {_selectedIndex}!");
    }

    public void SetDizi(Dizi dizi)
    {
        _selectedSkill = SkillType.Force;
        _selectedIndex = 0;
        _selectedDizi = dizi;
        UpdateSelected();
    }

    protected override void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_Skill_LevelUp, _ => UpdateSelected());
        Game.MessagingManager.RegEvent(EventString.Dizi_Skill_Update, _ => UpdateSelected());
    }

    private void UpdateSelected()
    {
        ResolveSelected();
        view_diziInfo.Set(_selectedDizi);
        view_diziProps.Set(_selectedDizi);
        view_tags.Set(Array.Empty<(string, Action)>());
        view_skillDev.UpdateSkill(_selectedDizi, _selectedSkill, _selectedIndex);
        view_diziList.UpdateList();
    }

    private void ResolveSelected()
    {
        var isSkillAvailable = _selectedSkill switch
        {
            SkillType.Combat => _selectedDizi.Skill.CombatSkills.Count > _selectedIndex,
            SkillType.Force => _selectedDizi.Skill.ForceSkills.Count > _selectedIndex,
            SkillType.Dodge => _selectedDizi.Skill.DodgeSkills.Count > _selectedIndex,
            _ => throw new ArgumentOutOfRangeException()
        };
        _selectedIndex = isSkillAvailable ? _selectedIndex : -1;
    }

    public override void Show() => View.Show();

    public override void Hide() => View.Hide();

    private class View_diziInfo : UiBase
    {
        private Image img_diziIcon { get; }
        private Text text_diziName { get; }
        private Text text_diziLevel { get; }
        private Element_prop element_propStrength { get; }
        private Element_prop element_propAgility { get; }
        private Element_prop element_propHp { get; }
        private Element_prop element_propMp { get; }

        public View_diziInfo(IView v, bool display) : base(v, display)
        {
            img_diziIcon = v.GetObject<Image>("img_diziIcon");
            text_diziName = v.GetObject<Text>("text_diziName");
            text_diziLevel = v.GetObject<Text>("text_diziLevel");
            element_propStrength = new Element_prop(v.GetObject<View>("element_propStrength"), true);
            element_propAgility = new Element_prop(v.GetObject<View>("element_propAgility"), true);
            element_propHp = new Element_prop(v.GetObject<View>("element_propHp"), true);
            element_propMp = new Element_prop(v.GetObject<View>("element_propMp"), true);
        }

        public void Set(Dizi dizi)
        {
            //img_diziIcon.sprite = 
            text_diziName.text = dizi.Name;
            text_diziLevel.text = dizi.Level.ToString();
            element_propStrength.Set(dizi.StrengthProp.LeveledValue, dizi.StrengthProp.SkillBonus());
            element_propAgility.Set(dizi.AgilityProp.LeveledValue, dizi.AgilityProp.SkillBonus());
            element_propHp.Set(dizi.HpProp.LeveledValue, dizi.HpProp.SkillBonus());
            element_propMp.Set(dizi.MpProp.LeveledValue, dizi.MpProp.SkillBonus());
        }
        private class Element_prop : UiBase
        {
            private Text text_selfValue { get; }
            private Text text_skillValue { get; }
            public Element_prop(IView v, bool display) : base(v, display)
            {
                text_selfValue = v.GetObject<Text>("text_selfValue");
                text_skillValue = v.GetObject<Text>("text_skillValue");
            }

            public void Set(int selfValue, int skillValue)
            {
                text_selfValue.text = selfValue.ToString();
                text_skillValue.text = skillValue.ToString();
            }
        }
    }

    private class View_diziProps : UiBase
    {
        private ListBoardUi<Prefab_propInfo> ListViewLeft { get; }
        private ListBoardUi<Prefab_propInfo> ListViewMid { get; }
        private ListBoardUi<Prefab_propInfo> ListViewRight { get; }
        public View_diziProps(IView v, bool display) : base(v, display)
        {
            ListViewLeft = new ListBoardUi<Prefab_propInfo>(v, "prefab_propInfo", "obj_listLeft");
            ListViewMid = new ListBoardUi<Prefab_propInfo>(v, "prefab_propInfo", "obj_listMid");
            ListViewRight = new ListBoardUi<Prefab_propInfo>(v, "prefab_propInfo", "obj_listRight");
        }

        public void Set(Dizi dizi)
        {
            var combatSet = dizi.GetCombatSet();
            var criDmgRatio = combatSet.GetCriticalDamageRatio(CombatArgs.Instance(dizi, dizi));
            var criRate = combatSet.GetCriticalRate(CombatArgs.Instance(dizi, dizi));
            var hrdDmgRatio = combatSet.GetHardDamageRatio(CombatArgs.Instance(dizi, dizi));
            var hrdRate = combatSet.GetHardRate(CombatArgs.Instance(dizi, dizi));
            var dodRate = combatSet.GetDodgeRate(CombatArgs.Instance(dizi, dizi));
            ListViewLeft.ClearList(u => u.Destroy());
            ListViewMid.ClearList(u => u.Destroy());
            ListViewRight.ClearList(u => u.Destroy());
            SetList(ListViewLeft, "重击率", $"{hrdRate:F1}%");
            SetList(ListViewLeft, "重击伤害倍数", $"{1 + hrdDmgRatio}");
            SetList(ListViewMid, "会心率", $"{criRate:F1}%");
            SetList(ListViewMid, "会心伤害倍数", $"{1 + criDmgRatio}");
            SetList(ListViewRight, "闪避率", $"{dodRate:F1}%");
        }

        private void SetList(ListBoardUi<Prefab_propInfo> list,string label,string value)
        {
            var ui = list.Instance(v => new Prefab_propInfo(v, true));
            ui.Set(label, value);
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
            }
        }
    }

    private class View_tags : UiBase
    {
        private ListViewUi<Prefab_tag> ListView { get; }
        public View_tags(IView v, bool display) : base(v, display)
        {
            ListView = new ListViewUi<Prefab_tag>(v, "prefab_tag", "scroll_tags");
        }

        public void Set((string tag,Action onclickAction)[] tags)
        {
            ListView.ClearList(ui => ui.Destroy());
            foreach (var (tag, action) in tags) ListView.Instance(v => new Prefab_tag(v, tag, action, true));
        }
        private class Prefab_tag : UiBase
        {
            public Prefab_tag(IView v,string label,Action onclickAction ,bool display) : base(v, display)
            {
                var text_label = v.GetObject<Text>("text_label");
                var btn_click = v.GetObject<Button>("btn_click");
                text_label.text = label;
                btn_click.OnClickAdd(onclickAction);
            }
        }
    }

    private class View_skillDev : UiBase
    {
        private View_skillManagement view_skillManagement { get; }
        private View_skillInfo view_skillInfo { get; }
        private View_buttons view_buttons { get; }

        public View_skillDev(IView v,
            Action<SkillType,int> onSkillSelectedAction,
            Action<string,SkillType,int> onEmptySkillAction,
            Action onUpgradeAction,
            Action onUseAction,
            Action onForgetAction,
            Action onComprehendAction,
            bool display) : base(v, display)
        {
            view_skillManagement = new View_skillManagement(v.GetObject<View>("view_skillManagement"),
                (s, i) => onSkillSelectedAction?.Invoke(s, i), 
                onEmptySkillAction,
                display);
            view_skillInfo = new View_skillInfo(v.GetObject<View>("view_skillInfo"), display);
            view_buttons = new View_buttons(v: v.GetObject<View>("view_buttons"),
                onUpgradeAction: () => onUpgradeAction?.Invoke(),
                onUseAction: () => onUseAction?.Invoke(),
                onForgetAction: () => onForgetAction?.Invoke(),
                onComprehendAction: () => onComprehendAction?.Invoke(),
                display: display);
        }

        public void UpdateSkill(Dizi dizi, SkillType skillType, int index)
        {
            view_skillManagement.SetForce(dizi, skillType == SkillType.Force ? index : -1);
            view_skillManagement.SetCombat(dizi, skillType == SkillType.Combat ? index : -1);
            view_skillManagement.SetDodge(dizi, skillType == SkillType.Dodge ? index : -1);
            if (index < 0)
            {
                view_skillInfo.Display(false);
                view_buttons.Set(false, false, false, false);
                return;
            }
            var arg = skillType switch
            {
                SkillType.Combat => (dizi.Skill.CombatSkills[index].Skill, dizi.Skill.CombatSkills[index].Level),
                SkillType.Force => (dizi.Skill.ForceSkills[index].Skill, dizi.Skill.ForceSkills[index].Level),
                SkillType.Dodge => (dizi.Skill.DodgeSkills[index].Skill, dizi.Skill.DodgeSkills[index].Level),
                _ => throw new ArgumentOutOfRangeException(nameof(skillType), skillType, null)
            };
            view_skillInfo.SetSkill(arg.Skill, arg.Level);
            UpdateViewButtons(dizi.Skill, skillType, index);
        }

        private void UpdateViewButtons(DiziSkill diziSkill,SkillType skillType,int index)
        {
            var isUseable = false;
            var isUpgradable = false;

            switch (skillType)
            {
                case SkillType.Force:
                {
                    var skill = diziSkill.GetSkill(skillType, index);
                    isUseable = !diziSkill.Force?.IsThis(skill)?? true;
                    isUpgradable = diziSkill.GetLevel(skill) < skill.MaxLevel();
                    break;
                }
                case SkillType.Combat:
                {
                    var skill = diziSkill.GetSkill(skillType, index);
                    isUseable = !diziSkill.Combat?.IsThis(skill) ?? true;
                    isUpgradable = diziSkill.GetLevel(skill) < skill.MaxLevel();
                    break;
                }
                case SkillType.Dodge:
                {
                    var skill = diziSkill.GetSkill(skillType, index);
                    isUseable = !diziSkill.Dodge?.IsThis(skill) ?? true;
                    isUpgradable = diziSkill.GetLevel(skill) < skill.MaxLevel();
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            view_buttons.Set(isUseable, isUpgradable, true, isUpgradable);
        }

        private class View_skillManagement : UiBase
        {
            private ListViewUi<Prefab_skill> ForceList { get; }
            private ListViewUi<Prefab_skill> CombatList { get; }
            private ListViewUi<Prefab_skill> DodgeList { get; }
            private event Action<SkillType,int> OnSkillSelected;
            private event Action<string,SkillType,int> OnEmptyAction;

            public View_skillManagement(IView v,
                Action<SkillType,int> onSkillSelectedAction,
                Action<string, SkillType, int> onEmptySkillAction,
                bool display) : base(v, display)
            {
                OnSkillSelected = onSkillSelectedAction;
                OnEmptyAction = onEmptySkillAction;
                ForceList = new ListViewUi<Prefab_skill>(v, "prefab_skill", "scroll_force");
                CombatList = new ListViewUi<Prefab_skill>(v, "prefab_skill", "scroll_combat");
                DodgeList = new ListViewUi<Prefab_skill>(v, "prefab_skill", "scroll_dodge");
            }

            public void SetDodge(Dizi dizi, int defaultIndex = -1)
            {
                DodgeList.ClearList(ui => ui.Destroy());
                for (var i = 0; i < dizi.Capable.DodgeSlot; i++)
                {
                    var index = i;
                    var f = dizi.Skill.DodgeSkills.Count > i ? dizi.Skill.DodgeSkills[i] : null;
                    var ui = DodgeList.Instance(v =>
                        new Prefab_skill(v, () => OnEmptyAction?.Invoke(dizi.Guid, SkillType.Dodge, index), true));
                    ui.SetAction(() =>
                    {
                        OnSkillSelected?.Invoke(SkillType.Dodge,index);
                        ui.SetSelected(true);
                    });
                    if (f != null)
                    {
                        ui.SetContent(f.Level, f.Skill.Name);
                        ui.SetIcon(f.Skill.Icon);
                        ui.SetSelected(i == defaultIndex);
                        ui.SetInUse(f == dizi.Skill.Dodge);
                    }
                    else ui.SetEmpty();
                }
            }

            public void SetCombat(Dizi dizi, int defaultIndex = -1)
            {
                CombatList.ClearList(ui => ui.Destroy());
                for (var i = 0; i < dizi.Capable.CombatSlot ; i++)
                {
                    var index = i;
                    var f = dizi.Skill.CombatSkills.Count > i ? dizi.Skill.CombatSkills[i] : null;
                    var ui = CombatList.Instance(v =>
                        new Prefab_skill(v, () => OnEmptyAction?.Invoke(dizi.Guid, SkillType.Combat, index), true));
                    ui.SetAction(() =>
                    {
                        OnSkillSelected?.Invoke(SkillType.Combat, index);
                        ui.SetSelected(true);
                    });
                    if (f != null)
                    {
                        ui.SetContent(f.Level, f.Skill.Name);
                        ui.SetIcon(f.Skill.Icon);
                        ui.SetSelected(i == defaultIndex);
                        ui.SetInUse(f == dizi.Skill.Combat);
                    }
                    else ui.SetEmpty();
                }
            }

            public void SetForce(Dizi dizi, int defaultIndex = -1)
            {
                ForceList.ClearList(ui => ui.Destroy());
                for (var i = 0; i < dizi.Capable.ForceSlot; i++)
                {
                    var index = i;
                    var f = dizi.Skill.ForceSkills.Count > i ? dizi.Skill.ForceSkills[i] : null;
                    var ui = ForceList.Instance(v =>
                        new Prefab_skill(v, () => OnEmptyAction?.Invoke(dizi.Guid, SkillType.Force, index), true));
                    ui.SetAction(() =>
                    {
                        OnSkillSelected?.Invoke(SkillType.Force, index);
                        ui.SetSelected(true);
                    });
                    if (f != null)
                    {
                        ui.SetContent(f.Level, f.Skill.Name);
                        ui.SetIcon(f.Skill.Icon);
                        ui.SetSelected(i == defaultIndex);
                        ui.SetInUse(f == dizi.Skill.Force);
                    }
                    else ui.SetEmpty();
                }
            }

            private class Prefab_skill : UiBase
            {
                private Text text_level { get; }
                private Image img_ico { get; }
                private Text text_name { get; }
                private GameObject obj_content { get; }
                private Button btn_skill { get; }
                private Button btn_empty { get; }
                private Image img_selected { get; }
                private Image img_inUse { get; }

                public Prefab_skill(IView v,Action onEmptyAction ,bool display) : base(v, display)
                {
                    text_level = v.GetObject<Text>("text_level");
                    img_ico = v.GetObject<Image>("img_ico");
                    text_name = v.GetObject<Text>("text_name");
                    obj_content = v.GetObject("obj_content");
                    btn_empty = v.GetObject<Button>("btn_empty");
                    img_selected = v.GetObject<Image>("img_selected");
                    img_inUse = v.GetObject<Image>("img_inUse");
                    btn_skill = v.GetObject<Button>("btn_skill");
                    btn_empty.OnClickAdd(onEmptyAction);
                }

                public void SetAction(Action action) => btn_skill.OnClickAdd(action);

                public void SetIcon(Sprite ico) => img_ico.sprite = ico;

                public void SetSelected(bool selected) => img_selected.gameObject.SetActive(selected);

                public void SetContent(int level, string name)
                {
                    text_level.text = level.ToString();
                    text_name.text = name;
                    obj_content.SetActive(true);
                    btn_empty.gameObject.SetActive(false);
                }

                public void SetEmpty()
                {
                    btn_empty.gameObject.SetActive(true);
                    obj_content.SetActive(false);
                    SetSelected(false);
                }

                public void SetInUse(bool inUsed) => img_inUse.gameObject.SetActive(inUsed);
            }
        }

        private class View_skillInfo : UiBase
        {
            private Element_level element_levelCurrent { get; }
            private Element_level element_levelNext { get; }

            public View_skillInfo(IView v, bool display) : base(v, display)
            {
                element_levelCurrent = new Element_level(v.GetObject<View>("element_levelCurrent"), display);
                element_levelNext = new Element_level(v.GetObject<View>("element_levelNext"), display);
            }

            public void SetSkill(ISkill skill, int level)
            {
                element_levelCurrent.SetSkill(skill, level, true);
                if (skill.MaxLevel() > level)
                    element_levelNext.SetSkill(skill, level + 1, false);
                else element_levelNext.Display(false);
                Display(true);
            }

            private class Element_level : UiBase
            {
                private View_skillTitle view_skillTitle { get; }
                private Element_prop element_prop_0 { get; }
                private Element_prop element_prop_1 { get; }
                private Element_prop element_prop_2 { get; }
                private Element_prop element_prop_3 { get; }
                private Element_prop element_prop_4 { get; }
                private Element_prop element_prop_5 { get; }
                private Element_attrib element_attrib_0 { get; }
                private Element_attrib element_attrib_1 { get; }
                private Element_attrib element_attrib_2 { get; }
                private Element_attrib element_attrib_3 { get; }
                private Element_attrib element_attrib_4 { get; }
                private Element_attrib element_attrib_5 { get; }
                private View_info view_info { get; }

                private List<Element_prop> Props { get; }
                private List<Element_attrib> Attribs { get; }

                public Element_level(IView v, bool display) : base(v, display)
                {
                    view_skillTitle = new View_skillTitle(v.GetObject<View>("view_skillTitle"), display);
                    element_prop_0 = new Element_prop(v.GetObject<View>("element_prop_0"), display);
                    element_prop_1 = new Element_prop(v.GetObject<View>("element_prop_1"), display);
                    element_prop_2 = new Element_prop(v.GetObject<View>("element_prop_2"), display);
                    element_prop_3 = new Element_prop(v.GetObject<View>("element_prop_3"), display);
                    element_prop_4 = new Element_prop(v.GetObject<View>("element_prop_4"), display);
                    element_prop_5 = new Element_prop(v.GetObject<View>("element_prop_5"), display);
                    element_attrib_0 = new Element_attrib(v.GetObject<View>("element_attrib_0"), display);
                    element_attrib_1 = new Element_attrib(v.GetObject<View>("element_attrib_1"), display);
                    element_attrib_2 = new Element_attrib(v.GetObject<View>("element_attrib_2"), display);
                    element_attrib_3 = new Element_attrib(v.GetObject<View>("element_attrib_3"), display);
                    element_attrib_4 = new Element_attrib(v.GetObject<View>("element_attrib_4"), display);
                    element_attrib_5 = new Element_attrib(v.GetObject<View>("element_attrib_5"), display);
                    view_info = new View_info(v.GetObject<View>("view_info"), display);
                    Props = new List<Element_prop>
                    {
                        element_prop_0,
                        element_prop_1,
                        element_prop_2,
                        element_prop_3,
                        element_prop_4,
                        element_prop_5,
                    };
                    Attribs = new List<Element_attrib>
                    {
                        element_attrib_0,
                        element_attrib_1,
                        element_attrib_2,
                        element_attrib_3,
                        element_attrib_4,
                        element_attrib_5,
                    };
                }

                public void SetSkill(ISkill skill, int level, bool setInfo)
                {
                    view_skillTitle.Set(skill.Name, level);

                    var props = skill.GetProps(level);
                    for (var i = 0; i < Props.Count; i++)
                    {
                        if (i < props.Length)
                        {
                            Props[i].Set(props[i].Name, props[i].Value, string.Empty);
                            Props[i].Display(true);
                        }
                        else Props[i].Display(false);
                    }

                    var attribs = skill.GetAttributes(level);
                    for (var i = 0; i < Attribs.Count; i++)
                    {
                        if (i < attribs.Length)
                        {
                            Attribs[i].Set("特性：", attribs[i].Name, attribs[i].Intro);
                            Attribs[i].Display(true);
                        }
                        else Attribs[i].Display(false);
                    }

                    if (setInfo) view_info.Set(string.Empty, skill.Name, skill.About);
                    else view_info.Display(false);
                    Game.CoService.RunCo(RefreshAfterFrame());

                    IEnumerator RefreshAfterFrame()
                    {
                        Display(false);
                        yield return new WaitForEndOfFrame();
                        Display(true);
                    }
                }


                private class View_skillTitle : UiBase
                {
                    private Text text_name { get; }
                    private Text text_level { get; }

                    public View_skillTitle(IView v, bool display) : base(v, display)
                    {
                        text_name = v.GetObject<Text>("text_name");
                        text_level = v.GetObject<Text>("text_level");
                    }

                    public void Set(string name, int level)
                    {
                        text_name.text = name;
                        text_level.text = level.ToString();
                    }
                }

                private class Element_prop : UiBase
                {
                    private Text text_label { get; }
                    private Text text_value { get; }
                    private Text text_remark { get; }

                    public Element_prop(IView v, bool display) : base(v, display)
                    {
                        text_label = v.GetObject<Text>("text_label");
                        text_value = v.GetObject<Text>("text_value");
                        text_remark = v.GetObject<Text>("text_remark");
                    }

                    public void Set(string label, string value, string remark)
                    {
                        text_label.text = label;
                        text_value.text = value;
                        text_remark.text = remark;
                    }
                }

                private class Element_attrib : UiBase
                {
                    private Text text_label { get; }
                    private Text text_value { get; }
                    private Text text_remark { get; }

                    public Element_attrib(IView v, bool display) : base(v, display)
                    {
                        text_label = v.GetObject<Text>("text_label");
                        text_value = v.GetObject<Text>("text_value");
                        text_remark = v.GetObject<Text>("text_remark");
                    }

                    public void Set(string label, string value, string remark)
                    {
                        text_label.text = label;
                        text_value.text = value;
                        text_remark.text = remark;
                    }
                }

                private class View_info : UiBase
                {
                    private Text text_label { get; }
                    private Text text_title { get; }
                    private Text text_about { get; }

                    public View_info(IView v, bool display) : base(v, display)
                    {
                        text_label = v.GetObject<Text>("text_label");
                        text_title = v.GetObject<Text>("text_title");
                        text_about = v.GetObject<Text>("text_about");
                    }

                    public void Set(string label, string title, string info)
                    {
                        text_label.text = label;
                        text_title.text = title;
                        text_about.text = info;
                        Display(true);
                    }
                }
            }
        }

        private class View_buttons : UiBase
        {
            private Button btn_use { get; }
            private Button btn_upgrade { get; }
            private Button btn_forget { get; }
            private Button btn_comprehend { get; }

            public View_buttons(IView v,
                Action onUpgradeAction,
                Action onUseAction,
                Action onForgetAction,
                Action onComprehendAction, bool display) : base(v, display)
            {
                btn_use = v.GetObject<Button>("btn_use");
                btn_upgrade = v.GetObject<Button>("btn_upgrade");
                btn_forget = v.GetObject<Button>("btn_forget");
                btn_comprehend = v.GetObject<Button>("btn_comprehend");
                btn_use.OnClickAdd(onUseAction);
                btn_upgrade.OnClickAdd(onUpgradeAction);
                btn_forget.OnClickAdd(onForgetAction);
                btn_comprehend.OnClickAdd(onComprehendAction);
            }

            public void Set(bool isUseable, bool isUpgradable, bool isForgettable, bool isComprehensible)
            {
                btn_use.gameObject.SetActive(isUseable);
                btn_upgrade.gameObject.SetActive(isUpgradable);
                btn_forget.gameObject.SetActive(isForgettable);
                btn_comprehend.gameObject.SetActive(isComprehensible);
            }
        }
    }

    private class View_DiziList : UiBase
    {
        private ScrollRect Scroll_dizi { get; }
        private ListViewUi<DiziPrefab> DiziList { get; }
        private Element Elm_all { get; }
        private Element Elm_idle { get; }
        private Element Elm_production { get; }
        private Element Elm_adventure { get; }
        private Element Elm_lost { get; }
        private event Action<string> OnDiziSelected;
        private string SelectedDiziGuid { get; set; }
        private Element[] AllFilter { get; }

        //cache 当前弟子列表
        private Dizi[] CurrentList { get; set; }

        public View_DiziList(IView v, Action<string> onDiziClicked) : base(v, true)
        {
            OnDiziSelected = onDiziClicked;
            Scroll_dizi = v.GetObject<ScrollRect>("scroll_dizi");
            DiziList = new ListViewUi<DiziPrefab>(v.GetObject<View>("prefab_dizi"), Scroll_dizi);
            Elm_all = new Element(v.GetObject<View>("element_all"), () =>
            {
                SetFilterSelected(Elm_all);
                UpdateList(GetAllDizi());
            });
            Elm_idle = new Element(v.GetObject<View>("element_idle"), () =>
            {
                SetFilterSelected(Elm_idle);
                UpdateList(GetIdleDizi());
            });
            Elm_production = new Element(v.GetObject<View>("element_production"), () =>
            {
                SetFilterSelected(Elm_production);
                UpdateList(GetProductionDizi());
            });
            Elm_adventure = new Element(v.GetObject<View>("element_adventure"), () =>
            {
                SetFilterSelected(Elm_adventure);
                UpdateList(GetAdventureDizi());
            });
            Elm_lost = new Element(v.GetObject<View>("element_lost"), () =>
            {
                SetFilterSelected(Elm_lost);
                UpdateList(GetLostDizi());
            });
            AllFilter = new[]
            {
                    Elm_all,
                    Elm_idle,
                    Elm_adventure,
                    Elm_production,
                    Elm_lost
                };
        }

        private Dizi[] GetLostDizi() => Game.World.Faction.DiziList
            .Where(d => d.State.Current == DiziStateHandler.States.Lost)
            .ToArray();
        private Dizi[] GetProductionDizi() => Game.World.Faction.DiziList
            .Where(d => d.State.Current == DiziStateHandler.States.AdvProduction)
            .ToArray();
        private Dizi[] GetAdventureDizi() => Game.World.Faction.DiziList
            .Where(d => d.State.Current == DiziStateHandler.States.AdvProgress ||
                        d.State.Current == DiziStateHandler.States.AdvReturning)
            .ToArray();
        private Dizi[] GetAllDizi() => Game.World.Faction.DiziList.ToArray();

        private Dizi[] GetIdleDizi() => Game.World.Faction.DiziList
            .Where(d => d.State.Current == DiziStateHandler.States.AdvWaiting ||
                        d.State.Current == DiziStateHandler.States.Idle)
            .ToArray();

        private void SetFilterSelected(Element element)
        {
            foreach (var e in AllFilter) e.SetSelected(e == element);
        }

        public void SetElements()
        {
            var faction = Game.World.Faction;
            var list = faction.DiziList.ToList();
            Elm_all.SetValue(list.Count, faction.MaxDizi);
            Elm_idle.SetValue(GetIdleDizi().Length, faction.MaxDizi);
            Elm_production.SetValue(GetProductionDizi().Length, faction.MaxDizi);
            Elm_adventure.SetValue(GetAdventureDizi().Length, faction.MaxDizi);
            Elm_lost.SetValue(GetLostDizi().Length, faction.MaxDizi);
        }

        public void UpdateList(Dizi[] list = null)
        {
            if (list == null)
                list = Game.World.Faction.DiziList.ToArray();
            CurrentList = list;
            ClearList();
            for (var i = 0; i < list.Length; i++)
            {
                var dizi = list[i];
                var guid = dizi.Guid;
                var index = i;
                var ui = DiziList.Instance(v => new DiziPrefab(v, () =>
                {
                    OnDiziSelected?.Invoke(CurrentList[index].Guid);
                    SetSelected(guid);
                }));
                ui.Init(dizi);
            }
        }
        private void SetSelected(string diziGuid)
        {
            SelectedDiziGuid = Game.World.Faction.GetDizi(diziGuid)?.Guid
                               ?? string.Empty;//如果弟子不在了,是不可选中的
            foreach (var ui in DiziList.List)
                ui.SetSelected(ui.DiziGuid == SelectedDiziGuid);
        }

        private void ClearList()
        {
            DiziList.ClearList(ui => ui.Destroy());
            SelectedDiziGuid = null;
        }

        private class DiziPrefab : UiBase
        {
            private Image Img_avatar { get; }
            private Text Text_name { get; }
            private Button Btn_dizi { get; }
            private Image Img_select { get; }
            public string DiziGuid { get; private set; }

            public DiziPrefab(IView v, Action onClickAction) : base(v, true)
            {
                Img_avatar = v.GetObject<Image>("img_avatar");
                Text_name = v.GetObject<Text>("text_name");
                Btn_dizi = v.GetObject<Button>("btn_dizi");
                Img_select = v.GetObject<Image>("img_select");
                Btn_dizi.OnClickAdd(onClickAction);
            }
            public void SetIcon(Sprite ico) => Img_avatar.sprite = ico;
            public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
            public void Init(Dizi dizi)
            {
                Text_name.text = dizi.Name;
                DiziGuid = dizi.Guid;
                Text_name.color = Game.GetColorFromGrade(dizi.Grade);
                SetSelected(false);
                Display(true);
            }
        }

        private class Element : UiBase
        {
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Button Btn_filter { get; }
            private Image Img_select { get; }

            public Element(IView v, Action onFilterAction) : base(v, true)
            {
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Btn_filter = v.GetObject<Button>("btn_filter");
                Btn_filter.OnClickAdd(onFilterAction);
                Img_select = v.GetObject<Image>("img_select");
            }

            public void SetValue(int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
            }

            public void SetSelected(bool selected) => Img_select.gameObject.SetActive(selected);
        }
    }
}