using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Systems.Messaging;
using AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.SoScripts.Items;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.Demo_v1
{
    internal class Demo_Win_SkillComprehend : WinUiManagerBase
    {
        private Win_skillComprehend win_skillComprehend { get; set; }
        private Demo_v1Agent Agent { get; }
        private SkillController SkillController => Game.Controllers.Get<SkillController>();
        private Faction Faction => Game.World.Faction;

        public Demo_Win_SkillComprehend(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            Agent = uiAgent;
        }

        protected override string ViewName => "demo_win_skillComprehend";

        private Dizi SelectedDizi { get; set; }
        private ISkill Skill { get; set; }

        protected override void Build(IView view)
        {
            win_skillComprehend = new Win_skillComprehend(view, arg => { OnSkillComprehend(arg); }, () =>
            {
                Hide();
                Agent.HideWindows();
            });
        }

        public void Set(Dizi dizi, ISkill skill, int level = 0)
        {
            SelectedDizi = dizi;
            Skill = skill;
            var nextLevel = level + 1;
            var comprehend = skill.Book.GetLevelMap(nextLevel);
            win_skillComprehend.SetSkill(skill, level, comprehend.BookCost);
            var items = Faction.GetAllComprehendItems(nextLevel);
            win_skillComprehend.SetAdditionItems(items);
        }

        public void Set(Dizi dizi, SkillType type, int index)
        {
            var skill = dizi.Skill.GetSkill(type, index);
            var level = dizi.Skill.GetLevel(skill);
            Set(dizi, skill, level);
        }

        private void OnSkillComprehend((int id, int amount)[] obj)
        {
            SkillController.Comprehend(SelectedDizi.Guid, Skill, obj);
            Hide();
            Agent.HideWindows();
        }

        protected override void RegEvents()
        {
        }

        private class Win_skillComprehend : UiBase
        {
            private Button btn_close { get; }
            private Image img_skillIcon { get; }
            private Text text_skillName { get; }
            private Text text_currentLevel { get; }
            private Text text_nextLevel { get; }
            private Image img_itemIcon { get; }
            private Text text_itemName { get; }
            private Text text_itemCount { get; }
            private Text text_x { get; }
            private Text text_minute { get; }
            private Text text_sec { get; }
            private Text text_successRate { get; }
            private Button btn_comprehend { get; }
            private View_additionItem view_additionItem { get; }
            private Color DefaultTextColor { get; set; }

            public Win_skillComprehend(IView v, Action<(int id, int amount)[]> onComprehendAction, Action onCloseAction)
                : base(v, false)
            {
                btn_close = v.GetObject<Button>("btn_close");
                img_skillIcon = v.GetObject<Image>("img_skillIcon");
                text_skillName = v.GetObject<Text>("text_skillName");
                text_currentLevel = v.GetObject<Text>("text_currentLevel");
                text_nextLevel = v.GetObject<Text>("text_nextLevel");
                img_itemIcon = v.GetObject<Image>("img_itemIcon");
                text_itemName = v.GetObject<Text>("text_itemName");
                text_x = v.GetObject<Text>("text_x");
                DefaultTextColor = text_itemName.color;
                text_itemCount = v.GetObject<Text>("text_itemCount");
                text_minute = v.GetObject<Text>("text_minute");
                text_sec = v.GetObject<Text>("text_sec");
                text_successRate = v.GetObject<Text>("text_successRate");
                btn_comprehend = v.GetObject<Button>("btn_comprehend");
                view_additionItem = new View_additionItem(v.GetObject<View>("view_additionItem"));
                btn_comprehend.OnClickAdd(() =>
                {
                    onComprehendAction?.Invoke(view_additionItem.SelectedItems);
                    view_additionItem.ClearItems();
                });
                btn_close.OnClickAdd(onCloseAction);
            }

            public void SetSkill(ISkill skill, int currentLevel, int bookCost)
            {
                var book = skill.Book;
                var nextLevel = currentLevel + 1;
                var upgrade = book.GetLevelMap(nextLevel);
                img_skillIcon.sprite = skill.Icon;
                text_skillName.text = skill.Name;
                text_currentLevel.text = currentLevel.ToString();
                img_itemIcon.sprite = book.Icon;
                text_itemName.text = book.Name;
                text_itemCount.text = bookCost.ToString();
                if (upgrade == null)
                {
                    text_nextLevel.text = "Max";
                    text_successRate.text = "100%";
                    text_minute.text = "0";
                    text_sec.text = "0";
                    SetComprehensible(false);
                    return;
                }

                text_nextLevel.text = nextLevel.ToString();
                text_successRate.text = $"{upgrade.Rate:F1}%";
                var timeSpan = TimeSpan.FromMinutes(upgrade.MinCost);
                text_minute.text = $"{(int)timeSpan.TotalMinutes}";
                text_sec.text = $"{timeSpan.Seconds}";
                SetComprehensible(bookCost >= upgrade.BookCost);
            }

            public void SetAdditionItems(IComprehendItem[] arg) => view_additionItem.SetItems(arg);

            private void SetComprehensible(bool isComprehensible)
            {
                text_itemCount.color = isComprehensible ? DefaultTextColor : Color.red;
                text_x.color = isComprehensible ? DefaultTextColor : Color.red;
                text_itemName.color = isComprehensible ? DefaultTextColor : Color.red;
                btn_comprehend.interactable = isComprehensible;
            }

            private class View_additionItem : UiBase
            {
                private ListViewUi<Prefab_item> ItemView { get; }
                private Dictionary<Prefab_item, bool> ItemMap { get; }

                public (int id, int amount)[] SelectedItems
                {
                    get
                    {
                        var list = new List<(int Key, int)>();
                        foreach (var kv1 in ItemMap.Where(kv => kv.Value).GroupBy(kv => kv.Key.Id))
                        {
                            list.Add((kv1.Key, kv1.Count()));
                        }

                        return list.ToArray();
                    }
                }

                public View_additionItem(IView v) : base(v, true)
                {
                    ItemView = new ListViewUi<Prefab_item>(v, "prefab_item", "scroll_item");
                    ItemMap = new Dictionary<Prefab_item, bool>();
                }

                public void SetItems(IComprehendItem[] arg)
                {
                    ClearItems();
                    foreach (var a in arg)
                    {
                        var item = a;
                        var ui = ItemView.Instance(v => new Prefab_item(v, item.Id));
                        ItemMap.Add(ui, false);
                        ui.Set(item.Name, item.Icon, () =>
                        {
                            ItemMap[ui] = !ItemMap[ui];
                            ui.SetSelected(ItemMap[ui]);
                        });
                    }
                }

                public void ClearItems()
                {
                    ItemMap.Clear();
                    ItemView.ClearList(ui => ui.Destroy());
                }

                private class Prefab_item : UiBase
                {
                    private Image img_icon { get; }
                    private Text text_name { get; }
                    private Image img_selected { get; }
                    private Button btn_item { get; }
                    public int Id { get; }

                    public Prefab_item(IView v, int id) : base(v, true)
                    {
                        Id = id;
                        img_icon = v.GetObject<Image>("img_icon");
                        text_name = v.GetObject<Text>("text_name");
                        img_selected = v.GetObject<Image>("img_selected");
                        btn_item = v.GetObject<Button>("btn_item");
                    }

                    public void Set(string name, Sprite sprite, Action onClickAction)
                    {
                        img_icon.sprite = sprite;
                        text_name.text = name;
                        btn_item.OnClickAdd(onClickAction);
                    }

                    public void SetSelected(bool selected)
                    {
                        img_selected.gameObject.SetActive(selected);
                    }
                }
            }
        }
    }
}