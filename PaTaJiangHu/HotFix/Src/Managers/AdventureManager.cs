using System;
using System.Collections.Generic;
using System.Linq;
using HotFix_Project.Views.Bases;
using Server;
using Server.Controllers.Adventures;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Object = UnityEngine.Object;

namespace HotFix_Project.Managers
{
    /// <summary>
    /// 冒险任务管理器，主要管理UI与本地数据的交互
    /// </summary>
    internal class AdventureManager
    {
        private AdventureUi AdWindow { get; set; }
        private Event1Ui E1Window { get; set; }
        private Event2Ui E2Window { get; set; }
        private Adventure Current { get; set; }

        public void Init()
        {
            InitAdventure();
            EventReg();
        }
        private void InitAdventure()
        {
            Game.UiBuilder.Build("view_adventure", (go, v) =>
            {
                AdWindow = new AdventureUi(go,
                    v.GetObject<Button>("btn_next"),
                    v.GetObject<Text>("text_progress"),
                    v.GetObject<ScrollRect>("scroll_units"),
                    v.GetObject<View>("prefab_unit"),
                    v.GetObject<Text>("text_intro"));
                AdWindow.SetNext(AdventureNext);
            });
            Game.UiBuilder.Build("view_event1", (go, v) =>
            {
                E1Window = new Event1Ui(go,
                    v.GetObject<Button>("btn_invoke"));
                E1Window.Display(false);
                E1Window.SetInvokeAction(() =>
                {
                    AdWindow.Display(true);
                    E1Window.Display(false);
                });
            });
            Game.UiBuilder.Build("view_event2", (go, v) =>
            {
                E2Window = new Event2Ui(go,
                    v.GetObject<Button>("btn_select0"),
                    v.GetObject<Button>("btn_select1"),
                    v.GetObject<Button>("btn_select2"));
                E2Window.Display(false);
                E2Window.SetSelect0(() =>
                {
                    AdWindow.Display(true);
                    E2Window.Display(false);
                });
                E2Window.SetSelect1(() =>
                {
                    AdWindow.Display(true);
                    E2Window.Display(false);
                });
                E2Window.SetSelect2(() =>
                {
                    AdWindow.Display(true);
                    E2Window.Display(false);
                });
            });
        }
        private void EventReg()
        {
            Game.MessagingManager.RegEvent(EventString.Adventure_Start, param =>
            {
                Current = new Adventure();
                Current.LoadParam(param);
                AdWindow.Set(Current);
                AdWindow.Display(true);
            });
            Game.MessagingManager.RegEvent(EventString.Adventure_Event, param =>
            {
                Current.LoadParam(param);
                AdWindow.Set(Current);
                var e = Current.Events.LastOrDefault();
                if (e != null)
                {
                    AdWindow.Display(false);
                    switch (e.Id)
                    {
                        case 0:
                            E1Window.Display(true);
                            break;
                        case 1:
                            E2Window.Display(true);
                            break;
                    }
                }
            });
        }
        private void AdventureNext()
        {
            AdWindow.Display(false);
            ServiceCaller.Instance.AdventureNext(Current);
            if (AdWindow.Progress % 2 == 0)
                E1Window.Display(true);
            else E2Window.Display(true);
        }
        private class Event2Ui : UiBase
        {
            private Button Btn_select0 { get; }
            private Button Btn_select1 { get; }
            private Button Btn_select2 { get; }

            public Event2Ui(GameObject gameObject, Button btnSelect0, Button btnSelect1, Button btnSelect2) : base(
                gameObject)
            {
                Btn_select0 = btnSelect0;
                Btn_select1 = btnSelect1;
                Btn_select2 = btnSelect2;
            }

            public void SetSelect0(Action action) => Btn_select0.OnClickAdd(action);
            public void SetSelect1(Action action) => Btn_select1.OnClickAdd(action);
            public void SetSelect2(Action action) => Btn_select2.OnClickAdd(action);
        }

        private class Event1Ui : UiBase
        {
            private Button Btn_invoke { get; }

            public Event1Ui(GameObject gameObject, Button btnInvoke) : base(gameObject)
            {
                Btn_invoke = btnInvoke;
            }

            public void SetInvokeAction(Action action) => Btn_invoke.OnClickAdd(action);

        }

        private class AdventureUi : UiBase
        {
            private Button Btn_next { get; }
            private Text Text_progress { get; }
            private Text Text_intro { get; }
            private ScrollRect Scroll_units { get; }
            private View Prefab_unit { get; }
            public int Progress { get; set; }
            private List<UnitUi> List { get; } = new List<UnitUi>();
            public Adventure Adventure { get; private set; }

            public AdventureUi(GameObject gameObject, Button btnNext, Text textProgress, ScrollRect scrollUnits,
                View prefabUnit, Text textIntro) : base(gameObject)
            {
                Btn_next = btnNext;
                Text_progress = textProgress;
                Scroll_units = scrollUnits;
                Prefab_unit = prefabUnit;
                Text_intro = textIntro;
                foreach (var view in Scroll_units.content.GetComponentsInChildren<View>())
                    view.gameObject.SetActive(false);
            }

            public void Set(Adventure adv)
            {
                Adventure = adv;
                UpdateProgress(adv.Progress);
                Text_intro.text = $"这是冒险任务[{adv.Id}]!";
                SetUnits(adv.Units);
            }


            public void SetNext(Action action) => Btn_next.OnClickAdd(action);

            private void UpdateProgress(int progress)
            {
                Progress = progress;
                Text_progress.text = Progress.ToString();
            }

            private void SetUnits(AdvUnit[] units)
            {
                ClearList();
                foreach (var unit in units) InstanceUnitPrefab(unit);
            }

            private void ClearList()
            {
                foreach (var ui in List)
                {
                    ui.Display(false);
                    ui.Destroy();
                }
                List.Clear();
            }

            private void InstanceUnitPrefab(AdvUnit unit)
            {
                var view = Object.Instantiate(Prefab_unit, Scroll_units.content);
                var ui = new UnitUi(view);
                ui.Set(unit, OnSelectedUnit);
                ui.Display(true);
                List.Add(ui);
            }

            private void OnSelectedUnit(AdvUnit obj)
            {
                Debug.Log($"{obj.Name} 已选！");
            }

            private class UnitUi : UiBase
            {
                private Button Btn_unit { get; }
                private Text Text_name { get; }
                private Text Text_hp { get; }

                public UnitUi(View view) : base(view.gameObject)
                {
                    Btn_unit = view.GetObject<Button>("btn_unit");
                    Text_name = view.GetObject<Text>("text_name");
                    Text_hp = view.GetObject<Text>("text_hp");
                }

                public void Set(AdvUnit advUnit, Action<AdvUnit> onclickAction)
                {
                    Text_name.text = advUnit.Name;
                    var hp = advUnit.Status.Hp;
                    Text_hp.text = $"{hp.Value}/{hp.Max}";
                    Btn_unit.OnClickAdd(() => onclickAction(advUnit));
                }

                public void Destroy() => Object.Destroy(gameObject);
            }
        }
    }
}