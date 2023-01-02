using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _GameClient.Models;
using Server.Configs._script.Factions;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers;

public class DiziAdvManager
{
    private View_diziAdv DiziAdv { get; set; }
    private MainUi MainUi { get; set; }
    private DiziController DiziController { get; set; }
    public void Init()
    {
        DiziController = Game.Controllers.Get<DiziController>();
        MainUi = Game.MainUi;
        InitUi();
        RegEvents();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_AdvManagement, bag =>
        {
            DiziAdv.Set(bag);
            MainUi.MainPage.HideAll(MainPageLayout.Sections.Mid);
            DiziAdv.Display(true);
        });
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziAdv", v =>
        {
            DiziAdv = new View_diziAdv(v,()=>{});
            MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        });
    }

    private class View_diziAdv : UiBase
    {
        public enum Skills { Combat,Force,Dodge }
        public enum Conditions { Food, Energy, Silver }
        public enum Items { Weapon,Armor }

        private Button Btn_Switch { get; }
        private Element_con Food { get; }
        private Element_con State { get; }
        private Element_con Silver { get; }
        private Element_skill Combat { get; }
        private Element_skill Force { get; }
        private Element_skill Dodge { get; }
        private Element_item Weapon { get; }
        private Element_item Armor { get; }
        private View_advLayout AdvLayoutView { get; }
        public View_diziAdv(IView v, Action onSwitchAction) : base(v.GameObject, false)
        {
            Btn_Switch = v.GetObject<Button>("btn_switch");
            Btn_Switch.OnClickAdd(onSwitchAction);
            Food = new Element_con(v.GetObject<View>("element_conFood"));
            State = new Element_con(v.GetObject<View>("element_conState"));
            Silver = new Element_con(v.GetObject<View>("element_conSilver"));
            Combat = new Element_skill(v.GetObject<View>("element_skillCombat"));
            Force = new Element_skill(v.GetObject<View>("element_skillForce"));
            Dodge = new Element_skill(v.GetObject<View>("element_skillDodge"));
            Weapon = new Element_item(v.GetObject<View>("element_itemWeapon"));
            Armor = new Element_item(v.GetObject<View>("element_itemArmor"));
            AdvLayoutView = new View_advLayout(v.GetObject<View>("view_advLayout"));
        }
        public void Set(ObjectBag bag)
        {
            var dizi = bag.Get<DiziDto>(0);
            Combat.Set(dizi.CombatSkill.Name, dizi.CombatSkill.Level);
            Force.Set(dizi.ForceSkill.Name, dizi.ForceSkill.Level);
            Dodge.Set(dizi.DodgeSkill.Name, dizi.DodgeSkill.Level);
            XDebug.LogWarning($"{dizi.Name} 状态,装备未完成!");
        }
        #region element_con
        public void SetCon(Conditions con,string title,int value,int max)
        {
            var conUi = GetConditionUi(con);
            conUi.Set(title, value, max);
        }
        private Element_con GetConditionUi(Conditions con) =>
            con switch
            {
                Conditions.Food => Food,
                Conditions.Energy => State,
                Conditions.Silver => Silver,
                _ => throw new ArgumentOutOfRangeException(nameof(con), con, null)
            };

        #endregion

        #region element_skill
        public void SetSkill(Skills skill, string skillName, int level)
        {
            var skillUi = GetSkillUi(skill);
            skillUi.Set(skillName, level);
        }
        private Element_skill GetSkillUi(Skills skill)
        {
            var skillUi = skill switch
            {
                Skills.Combat => Combat,
                Skills.Force => Force,
                Skills.Dodge => Dodge,
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
            return skillUi;
        }

        #endregion

        #region element_item
        public void ClearItem(Items item) => ClearItem(GetItemUi(item));
        public void SetWeapon(Items item,string weaponName) => SetItem(GetItemUi(item), weaponName);
        private Element_item GetItemUi(Items item) => item switch
        {
            Items.Weapon => Weapon,
            Items.Armor => Armor,
            _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
        };
        private void SetItem(Element_item item,string title)
        {
            item.SetTitle(title);
            item.SetEmpty(false);
        }
        private void ClearItem(Element_item item) => item.SetEmpty(true);
        #endregion

        private class Element_skill : UiBase
        {
            private Image Img_ico { get; }
            private Text Text_level { get; }
            private Text Text_skillName { get; }
            public Element_skill(IView v) : base(v.GameObject, true)
            {
                Img_ico = v.GetObject<Image>("img_ico");
                Text_level = v.GetObject<Text>("text_level");
                Text_skillName = v.GetObject<Text>("text_skillName");
            }

            public void Set(string skillName, int level)
            {
                Text_level.text = level.ToString();
                Text_skillName.text = skillName;
            }

            public void SetImage(Sprite img) => Img_ico.sprite = img;
        }
        private class Element_item : UiBase
        {
            private Image Img_item { get; }
            private Text Text_title { get; }
            private Image Img_empty { get; }
            public Element_item(IView v) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_title = v.GetObject<Text>("text_title");
                Img_empty = v.GetObject<Image>("img_empty");
            }
            public void SetImage(Sprite img)=> Img_item.sprite = img;
            public void SetEmpty(bool empty)=> Img_empty.gameObject.SetActive(empty);
            public void SetTitle(string title) => Text_title.text = title;
        }
        private class Element_con : UiBase
        {
            private Scrollbar Scrbar_condition { get; }
            private Text Text_value { get; }
            private Text Text_max { get; }
            private Text Text_title { get; }
            public Element_con(IView v) : base(v.GameObject, true)
            {
                Scrbar_condition = v.GetObject<Scrollbar>("scrbar_condition");
                Text_value = v.GetObject<Text>("text_value");
                Text_max = v.GetObject<Text>("text_max");
                Text_title = v.GetObject<Text>("text_title");
            }
            public void Set(string title, int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
                Text_title.text = title;
            }
        }
        private class View_advLayout : UiBase
        {
            private ScrollRect Scroll_advLog { get; }
            private Prefab Prefab_Log { get; }
            private Button Btn_recall { get; }
            private Image Img_costIco { get; }
            private Text Text_cost { get; }
            private Element_equit Slot0 { get; }
            private Element_equit Slot1 { get; }
            private Element_equit Slot2 { get; }
            private Button Btn_advStart { get; }

            public View_advLayout(IView v/*, Action onRecallAction, Action onAdvStartAction*/) : base(v.GameObject, true)
            {
                Scroll_advLog = v.GetObject<ScrollRect>("scroll_advLog");
                Prefab_Log = new Prefab(v.GetObject<View>("prefab_log"));
                Btn_recall = v.GetObject<Button>("btn_recall");
                ///Btn_recall.OnClickAdd(() =>
                ///{
                ///    onRecallAction?.Invoke();
                ///});
                Img_costIco = v.GetObject<Image>("img_costIco");
                Text_cost = v.GetObject<Text>("text_cost");
                Slot0 = new Element_equit(v.GetObject<View>("element_equitSlot0"));
                Slot1 = new Element_equit(v.GetObject<View>("element_equitSlot1"));
                Slot2 = new Element_equit(v.GetObject<View>("element_equitSlot2"));
                Btn_advStart = v.GetObject<Button>("btn_advStart");
                ///Btn_advStart.OnClickAdd(() =>
                ///{
                ///    onAdvStartAction?.Invoke();
                ///});
            }
            public void Set(Sprite icon, int cost)
            {
                Img_costIco.sprite = icon;
                Text_cost.text = cost.ToString();
            }
            private class Prefab : UiBase
            {
                private Text Prefab_Log { get; }
                public Prefab(IView v) :base(v.GameObject, false)
                {
                    Prefab_Log = v.GetObject<Text>("prefab_log");
                }
                public void LogMessage(string message) => Prefab_Log.text = message;
            }

            private class Element_equit : UiBase
            {
                private Scrollbar Scrbar_item { get; }
                private Image Img_ico { get; }
                private Text Text_min { get; }
                private Text Text_sec { get; }
                
                public Element_equit(IView v) : base(v.GameObject, true)
                {
                    Scrbar_item = v.GetObject<Scrollbar>("scrbar_item");
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_min = v.GetObject<Text>("text_min");
                    Text_sec = v.GetObject<Text>("text_sec");
                }
                public void SetIcon(Sprite icon) => Img_ico.sprite = icon;
                public void SetTimer(int min, int sec)
                {
                    Text_min.text = min.ToString();
                    Text_sec.text = sec.ToString();
                }
            }
        }
    }
}
