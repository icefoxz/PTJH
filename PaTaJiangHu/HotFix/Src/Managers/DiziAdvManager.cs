using HotFix_Project.Views.Bases;
using System;
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
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => DiziAdv.Update());
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag => DiziAdv.Update());
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziAdv", v =>
        {
            DiziAdv = new View_diziAdv(v, () => { },
                (guid, itemType) => DiziController.ManageDiziEquipment(guid, itemType));
            MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        });
    }

    private class View_diziAdv : UiBase
    {
        public enum Skills { Combat,Force,Dodge }
        public enum Conditions { Food, Energy, Silver }
        public enum Items { Weapon,Armor }

        private Button Btn_Switch { get; }
        private ElementManager ElementMgr { get; }
        private View_advLayout AdvLayoutView { get; }

        public View_diziAdv(IView v, Action onSwitchAction, Action<string,int> onItemSelectAction) : base(v.GameObject, false)
        {
            Btn_Switch = v.GetObject<Button>("btn_switch");
            Btn_Switch.OnClickAdd(onSwitchAction);
            ElementMgr = new ElementManager(
                new Element_con(v.GetObject<View>("element_conFood")),
                new Element_con(v.GetObject<View>("element_conState")),
                new Element_con(v.GetObject<View>("element_conSilver")),
                new Element_skill(v.GetObject<View>("element_skillCombat")),
                new Element_skill(v.GetObject<View>("element_skillForce")),
                new Element_skill(v.GetObject<View>("element_skillDodge")),
                new Element_item(v.GetObject<View>("element_itemWeapon"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 0)),
                new Element_item(v.GetObject<View>("element_itemArmor"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 1))
            );
            //AdvLayoutView = new View_advLayout(v.GetObject<View>("view_advLayout"));
        }

        private Dizi SelectedDizi { get; set; }//cache
        public void Set(ObjectBag bag)
        {
            var guid = bag.Get<string>(0);
            var dizi = Game.World.Faction.DiziMap[guid];
            SelectedDizi = dizi;
            SetDizi(dizi);
        }
        public void Update()
        {
            SetDizi(SelectedDizi);
        }

        private void SetDizi(Dizi dizi)
        {
            SetDiziElements(dizi);
            var isInAdventure = dizi.Adventure == null;
            //AdvLayoutView.Set();
            XDebug.LogWarning($"{dizi.Name} 历练板块未完成!");
        }


        private void SetDiziElements(Dizi dizi)
        {
            ElementMgr.SetSkill(Skills.Combat, dizi.CombatSkill.Name, dizi.CombatSkill.Level);
            ElementMgr.SetSkill(Skills.Force, dizi.ForceSkill.Name, dizi.ForceSkill.Level);
            ElementMgr.SetSkill(Skills.Dodge, dizi.DodgeSkill.Name, dizi.DodgeSkill.Level);
            if (dizi.Weapon == null) ElementMgr.ClearItem(Items.Weapon);
            else ElementMgr.SetItem(Items.Weapon, dizi.Weapon.Name);
            if (dizi.Armor == null) ElementMgr.ClearItem(Items.Armor);
            else ElementMgr.SetItem(Items.Armor, dizi.Armor.Name);
            ElementMgr.SetConValue(Conditions.Food, dizi.Food.Value, dizi.Food.Max);
            ElementMgr.SetConValue(Conditions.Energy, dizi.Energy.Value, dizi.Energy.Max);
            ElementMgr.SetConValue(Conditions.Silver, dizi.Silver.Value, dizi.Silver.Max);
        }

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
            private Button ElementButton { get; }

            public Element_item(IView v, Action onClickAction) : base(v.GameObject, true)
            {
                Img_item = v.GetObject<Image>("img_item");
                Text_title = v.GetObject<Text>("text_title");
                Img_empty = v.GetObject<Image>("img_empty");
                ElementButton = v.GameObject.GetComponent<Button>();
                ElementButton.OnClickAdd(onClickAction);

            }
            public void SetImage(Sprite img)
            {
                Img_item.sprite = img;
                Img_item.gameObject.SetActive(true);
            }

            public void SetEmpty(bool empty)
            {
                Img_empty.gameObject.SetActive(empty);
                Img_item.gameObject.SetActive(!empty);
                Text_title.gameObject.SetActive(!empty);
            }

            public void SetTitle(string title)
            {
                Text_title.text = title;
                Text_title.gameObject.SetActive(true);
            }
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
            public void SetTitle(string title)
            {
                Text_title.text = title;
            }
            public void SetValue(int value, int max)
            {
                Text_value.text = value.ToString();
                Text_max.text = max.ToString();
                Scrbar_condition.size = 1f * value / max;
            }
        }
        private class View_advLayout : UiBase
        {
            private Button Btn_recall { get; }
            private Image Img_costIco { get; }
            private Text Text_cost { get; }
            private Button Btn_advStart { get; }

            private ListViewUi<LogPrefab> LogView { get; }
            private Element_equip[] ItemSlots { get; }

            public View_advLayout(IView v, Action onAdvStartAction, Action onRecallAction) : base(v.GameObject, true)
            {
                var scroll_advLog = v.GetObject<ScrollRect>("scroll_advLog");
                LogView = new ListViewUi<LogPrefab>(v.GetObject<View>("prefab_log"), scroll_advLog);
                Img_costIco = v.GetObject<Image>("img_costIco");
                Text_cost = v.GetObject<Text>("text_cost");
                var slot0 = new Element_equip(v.GetObject<View>("element_equipSlot0"));
                var slot1 = new Element_equip(v.GetObject<View>("element_equipSlot1"));
                var slot2 = new Element_equip(v.GetObject<View>("element_equipSlot2"));
                ItemSlots = new[] { slot0, slot1, slot2 };
                Btn_recall = v.GetObject<Button>("btn_recall");
                Btn_advStart = v.GetObject<Button>("btn_advStart");
                Btn_recall.OnClickAdd(() => onRecallAction?.Invoke());
                Btn_advStart.OnClickAdd(() => onAdvStartAction?.Invoke());
            }
            public void SetCost(Sprite icon, int cost)
            {
                Img_costIco.sprite = icon;
                Text_cost.text = cost.ToString();
            }

            public void SetPrepareState()
            {
                Btn_advStart.gameObject.SetActive(true);
                foreach (var slot in ItemSlots)
                {
                    slot.SetTimer(0,0);
                }
            }

            private class LogPrefab : UiBase
            {
                private Text Prefab_Log { get; }
                public LogPrefab(IView v) :base(v.GameObject, false)
                {
                    Prefab_Log = v.GetObject<Text>("prefab_log");
                }
                public void LogMessage(string message) => Prefab_Log.text = message;
            }

            private class Element_equip : UiBase
            {
                private Scrollbar Scrbar_item { get; }
                private Image Img_ico { get; }
                private Text Text_min { get; }
                private Text Text_sec { get; }
                
                public Element_equip(IView v) : base(v.GameObject, true)
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

        private class ElementManager
        {
            private Element_con Food { get; }
            private Element_con State { get; }
            private Element_con Silver { get; }
            private Element_skill Combat { get; }
            private Element_skill Force { get; }
            private Element_skill Dodge { get; }
            private Element_item Weapon { get; }
            private Element_item Armor { get; }

            public ElementManager(Element_con food, Element_con state, Element_con silver, Element_skill combat,
                Element_skill force, Element_skill dodge, Element_item weapon, Element_item armor)
            {
                Food = food;
                State = state;
                Silver = silver;
                Combat = combat;
                Force = force;
                Dodge = dodge;
                Weapon = weapon;
                Armor = armor;
            }

            #region element_con

            public void SetConValue(Conditions con, int value, int max)
            {
                var conUi = GetConditionUi(con);
                conUi.SetValue(value, max);
            }
            public void SetConTitle(Conditions con, string title)
            {
                var conUi = GetConditionUi(con);
                conUi.SetTitle(title);
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
            public void SetItem(Items item, string weaponName) => SetItem(GetItemUi(item), weaponName);

            private Element_item GetItemUi(Items item) => item switch
            {
                Items.Weapon => Weapon,
                Items.Armor => Armor,
                _ => throw new ArgumentOutOfRangeException(nameof(item), item, null)
            };

            private void SetItem(Element_item item, string title)
            {
                item.SetTitle(title);
                item.SetEmpty(false);
            }

            private void ClearItem(Element_item item) => item.SetEmpty(true);

            #endregion
        }
    }
}
