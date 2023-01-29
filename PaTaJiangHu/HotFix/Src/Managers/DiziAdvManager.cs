using HotFix_Project.Views.Bases;
using System;
using _GameClient.Models;
using HotFix_Project.Serialization;
using Server.Controllers;
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
    private DiziAdvController DiziAdvController { get; set; }
    
    public void Init()
    {
        DiziController = Game.Controllers.Get<DiziController>();
        DiziAdvController = Game.Controllers.Get<DiziAdvController>();
        MainUi = Game.MainUi;
        InitUi();
    }

    private void RegEvents()
    {
        Game.MessagingManager.RegEvent(EventString.Dizi_AdvManagement, bag =>
        {
            DiziAdv.Set(bag);
            MainUi.MainPage.HideAll(MainPageLayout.Sections.Mid);
            DiziAdv.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag => DiziAdv.Update());
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemEquipped, bag => DiziAdv.Update());
        Game.MessagingManager.RegEvent(EventString.Dizi_ItemUnEquipped, bag =>
        {
            XDebug.Log(bag.Data.Length.ToString());
            DiziAdv.Update();
        });
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziAdv", v =>
        {
            DiziAdv = new View_diziAdv(v, 
                (guid, itemType) => DiziController.ManageDiziEquipment(guid, itemType),
                guid => DiziAdvController.AdventureStart(guid),
                guid => DiziAdvController.AdventureRecall(guid),
                () => XDebug.LogWarning("切换弟子管理页面!, 功能未完!")
            );
            MainUi.MainPage.Set(v, MainPageLayout.Sections.Mid, true);
        },RegEvents);
    }

    private class View_diziAdv : UiBase
    {
        public enum Skills { Combat,Force,Dodge }
        public enum Conditions { Food, State, Silver, Injury, Inner }
        public enum Items { Weapon,Armor }

        private Button Btn_Switch { get; }
        private ElementManager ElementMgr { get; }
        private View_advLayout AdvLayoutView { get; }

        public View_diziAdv(IView v, Action<string,int> onItemSelectAction, Action<string> onAdvStartAction,Action<string> onAdvRecallAction, Action onSwitchAction) : base(v.GameObject, false)
        {
            Btn_Switch = v.GetObject<Button>("btn_switch");
            Btn_Switch.OnClickAdd(onSwitchAction);
            ElementMgr = new ElementManager(
                new Element_con(v.GetObject<View>("element_conFood")),
                new Element_con(v.GetObject<View>("element_conState")),
                new Element_con(v.GetObject<View>("element_conSilver")),
                new Element_con(v.GetObject<View>("element_conInjury")),
                new Element_con(v.GetObject<View>("element_conInner")),
                new Element_skill(v.GetObject<View>("element_skillCombat")),
                new Element_skill(v.GetObject<View>("element_skillForce")),
                new Element_skill(v.GetObject<View>("element_skillDodge")),
                new Element_item(v.GetObject<View>("element_itemWeapon"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 0)),
                new Element_item(v.GetObject<View>("element_itemArmor"),
                    () => onItemSelectAction?.Invoke(SelectedDizi?.Guid, 1))
            );
            AdvLayoutView = new View_advLayout(v.GetObject<View>("view_advLayout"),
                () => onAdvStartAction?.Invoke(SelectedDizi?.Guid),
                () => onAdvRecallAction?.Invoke(SelectedDizi?.Guid));
        }

        private Dizi SelectedDizi { get; set; }//cache
        public void Set(ObjectBag bag)
        {
            var guid = bag.Get<string>(0);
            var dizi = Game.World.Faction.GetDizi(guid);
            SelectedDizi = dizi;
            SetDizi(dizi);
        }
        public void Update()
        {
            if (SelectedDizi == null) return;
            SetDizi(SelectedDizi);
        }

        private void SetDizi(Dizi dizi)
        {
            SetDiziElements(dizi);
            if (dizi.Adventure is {State:AutoAdventure.States.Progress})
                AdvLayoutView.SetAdventure(dizi); //历练模式
            else
                AdvLayoutView.SetPrepareState(dizi); //准备模式
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
            ElementMgr.SetConValue(Conditions.State, dizi.Emotion.Value, dizi.Emotion.Max);
            ElementMgr.SetConValue(Conditions.Silver, dizi.Silver.Value, dizi.Silver.Max);
            ElementMgr.SetConValue(Conditions.Injury, dizi.Injury.Value, dizi.Injury.Max);
            ElementMgr.SetConValue(Conditions.Inner, dizi.Inner.Value, dizi.Inner.Max);
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
            private enum Modes
            {
                Prepare, Adventure
            }
            private Button Btn_recall { get; }
            private Image Img_costIco { get; }
            private Text Text_cost { get; }
            private Button Btn_advStart { get; }

            private ListViewUi<LogPrefab> LogView { get; }
            private ListViewUi<Prefab_rewardItem> RewardItemView { get; }
            private Element_equip[] ItemSlots { get; }

            public View_advLayout(IView v, Action onAdvStartAction, Action onRecallAction) : base(v.GameObject, true)
            {
                var scroll_advLog = v.GetObject<ScrollRect>("scroll_advLog");
                LogView = new ListViewUi<LogPrefab>(v.GetObject<View>("prefab_log"), scroll_advLog);
                RewardItemView = new ListViewUi<Prefab_rewardItem>(v, "prefab_rewardItem", "scroll_rewardItem");
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

            public void SetPrepareState(Dizi dizi)
            {
                foreach (var slot in ItemSlots) slot.SetTimer();
                SetMode(Modes.Prepare);
                SetRewardItems(dizi);
            }

            public void SetAdventure(Dizi dizi)
            {
                XDebug.LogWarning("弟子历练未设置!");
                SetMode(Modes.Adventure);
                SetRewardItems(dizi);
            }

            private void SetRewardItems(Dizi dizi)
            {
                RewardItemView.ClearList(p => p.Destroy());
                if (dizi.Adventure == null)
                {
                    for (var i = 0; i < dizi.Capable.Bag; i++) 
                        RewardItemView.Instance(v => new Prefab_rewardItem(v));
                }
            }

            private void SetMode(Modes mode)
            {
                Btn_advStart.gameObject.SetActive(mode == Modes.Prepare);
                Btn_recall.gameObject.SetActive(mode == Modes.Adventure);
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
                private enum Modes{Empty,Content,InUse}
                private Scrollbar Scrbar_item { get; }
                private Image Img_ico { get; }
                private Text Text_timerMin { get; }
                private Text Text_timerSec { get; }
                private GameObject Go_timer { get; }
                private GameObject Go_slidingArea { get; }
                
                public Element_equip(IView v) : base(v.GameObject, true)
                {
                    Scrbar_item = v.GetObject<Scrollbar>("scrbar_item");
                    Img_ico = v.GetObject<Image>("img_ico");
                    Text_timerMin = v.GetObject<Text>("text_timerMin");
                    Text_timerSec = v.GetObject<Text>("text_timerSec");
                    Go_timer = v.GetObject("go_timer");
                    Go_slidingArea = v.GetObject("go_slidingArea");
                    SetMode(Modes.Empty);
                }

                private void SetMode(Modes mode)
                {
                    Go_slidingArea.SetActive(mode == Modes.InUse);
                    Go_timer.SetActive(mode == Modes.InUse);
                    Img_ico.gameObject.SetActive(mode != Modes.Empty);
                }
                public void ClearItem() => SetMode(Modes.Empty);
                public void SetIcon(Sprite icon)
                {
                    Img_ico.sprite = icon;
                    SetMode(Modes.Content);
                }

                public void SetTimer(int min = -1, int sec = -1)
                {
                    var isDefault = min <= 0 && sec <= 0;
                    if (!isDefault) SetMode(Modes.InUse);
                    Text_timerMin.text = isDefault ? string.Empty : min.ToString();
                    Text_timerSec.text = isDefault ? string.Empty : sec.ToString();
                }
            }

            private class Prefab_rewardItem : UiBase
            {
                private enum Modes
                {
                    Empty,Item,Disable,
                }
                private Image Img_ico { get; }
                private Image Img_x { get; }

                public Prefab_rewardItem(IView v) : base(v, true)
                {
                    Img_ico = v.GetObject<Image>("img_ico");
                    Img_x = v.GetObject<Image>("img_x");
                    SetEmpty();
                }

                public void SetIco(Sprite ico)
                {
                    Img_ico.sprite = ico;
                    Set(Modes.Item);
                }
                public void SetEmpty() => Set(Modes.Empty);
                public void SetDisable() => Set(Modes.Disable);
                private void Set(Modes mode)
                {
                    Img_ico.gameObject.SetActive(mode == Modes.Item);
                    Img_x.gameObject.SetActive(mode == Modes.Disable);
                }
            }

        }

        private class ElementManager
        {
            private Element_con Food { get; }
            private Element_con State { get; }
            private Element_con Silver { get; }
            private Element_con Injury { get; }
            private Element_con Inner { get; }
            private Element_skill Combat { get; }
            private Element_skill Force { get; }
            private Element_skill Dodge { get; }
            private Element_item Weapon { get; }
            private Element_item Armor { get; }

            public ElementManager(Element_con food, Element_con state, Element_con silver, Element_con injury, Element_con inner, Element_skill combat,
                Element_skill force, Element_skill dodge, Element_item weapon, Element_item armor)
            {
                Food = food;
                State = state;
                Silver = silver;
                Injury = injury;
                Inner = inner;
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
                    Conditions.State => State,
                    Conditions.Silver => Silver,
                    Conditions.Injury => Injury,
                    Conditions.Inner => Inner,
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
