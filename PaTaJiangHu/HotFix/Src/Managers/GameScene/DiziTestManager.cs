using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs;
using Server.Configs.Characters;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;
using Object = UnityEngine.Object;

namespace HotFix_Project.Managers.GameScene;

public class DiziTestManager
{
    private DiziGenWindow GenWindow { get; set; }
    private StaminaCountWindow StaCountWindow { get; set; }
    private MedicineTestWindow MedicineTest { get; set; }
    private ITestDiziController Controller { get; set; }
    public void Init()
    {
        Controller = TestCaller.Instance.InstanceDiziController();
        InitUi();
        EventStaminaReg();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("test_diziGenerate", v =>
        {
            GenWindow = new DiziGenWindow(v, level => Controller.OnDiziLevel(level));
            GenWindow.SetButtons(Controller.OnGenerateDizi, () => GenWindow.Display(false));
        },EventGenWindowReg);
        Game.UiBuilder.Build("test_diziStamina", v =>
        {
            StaCountWindow = new StaminaCountWindow(v.GameObject,
                v.GetObject<InputField>("input_minutes"),
                v.GetObject<InputField>("input_stamina"),
                v.GetObject<Button>("btn_set"),
                v.GetObject<Text>("text_config"),
                v.GetObject<Text>("text_last"),
                v.GetObject<Text>("text_stamina"),
                v.GetObject<Text>("text_timeUpdate")
            );
            StaminaTimer.Instance.StaminaUpdate +=
                () =>
                {
                    var st = StaminaTimer.Instance;
                    StaCountWindow.Update(
                        st.CurrentStamina,
                        st.Countdown,
                        st.MinPerStamina,
                        SysTime.LocalFromUnixTicks(st.LastTicks));
                };
            StaCountWindow.SetButton(() => Controller.OnSetStamina(
                StaCountWindow.InputStamina,
                StaCountWindow.InputMinutes));
        },EventStaminaReg);
    }

    private void EventGenWindowReg()
    {
        Game.MessagingManager.RegEvent(EventString.Test_DiziGenerate, bag => GenWindow.SetDizi(bag.Get<Dizi>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_DiziRecruit, bag =>
        {
            GenWindow.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_DiziLeveling,
            bag => GenWindow.SetDiziGrow(bag.Get<Dizi>(0)));
    }

    private void EventStaminaReg()
    {
        Game.MessagingManager.RegEvent(EventString.Test_StaminaWindow, bag =>
        {
            StaCountWindow.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_MedicineWindow, bag => MedicineTest.UpdateMedicines(bag));
        Game.MessagingManager.RegEvent(EventString.Test_StatusUpdate, bag =>
        {
            var status = bag.Get<TestStatus>(0);
            MedicineTest.UpdateCon(MedicineTestWindow.Cons.Hp, status.Hp);
            MedicineTest.UpdateCon(MedicineTestWindow.Cons.Mp, status.Mp);
            MedicineTest.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Test_UpdateHp,
            bag => MedicineTest.UpdateCon(MedicineTestWindow.Cons.Hp, bag.Get<ConValue>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_UpdateMp,
            bag => MedicineTest.UpdateCon(MedicineTestWindow.Cons.Mp, bag.Get<ConValue>(0)));
    }

    private class TestStatus
    {
        public ConValue Hp { get; set; }
        public ConValue Mp { get; set; }
        public bool IsExhausted { get; set; }
    }
    private class MedicineUi
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    private class DiziGenWindow : UiBase
    {
        public enum Grades
        {
            F,
            E,
            D,
            C,
            B,
            A,
            S,
        }
        public static string GetColorTitle(Grades grade) => grade switch
        {
            Grades.F => "白",
            Grades.E => "绿",
            Grades.D => "篮",
            Grades.C => "紫",
            Grades.B => "橙",
            Grades.A => "红",
            Grades.S => "金",
            _ => throw new ArgumentOutOfRangeException(nameof(grade), grade, null)
        };

        private Text Text_name { get; }
        private Button Btn_generate { get; }
        private Button Btn_recruit { get; }
        private Transform Trans_content { get; }
        private View Prefab_attribute { get; }

        private ListViewUi<PropUi> PropList { get; }
        private ListViewUi<GradeUi> GradeList { get; }
        private LevelingUi Leveling { get; }

        private int SelectedGrade { get; set; }
        private int Level { get; set; } = 1;

        public DiziGenWindow(IView v,Action<int> leveling) : base(v.GameObject, false)
        {
            Text_name = v.GetObject<Text>("text_name");
            Btn_generate = v.GetObject<Button>("btn_generate");
            Btn_recruit = v.GetObject<Button>("btn_recruit");
            Trans_content = v.GetObject<Transform>("trans_content");
            Prefab_attribute = v.GetObject<View>("prefab_attribute");
            var gradeViewContent = v.GetObject<Transform>("trans_gradeContent");
            var gradePrefab = v.GetObject<View>("prefab_grade");
            PropList = new ListViewUi<PropUi>(Prefab_attribute, Trans_content.gameObject);
            GradeList = new ListViewUi<GradeUi>(gradePrefab, gradeViewContent.gameObject);
            Leveling = new LevelingUi(v.GetObject<View>("view_leveling"), 
                () => leveling(LevelUp()),
                () => leveling(LevelDown()));
            var gradeMap = Enum.GetValues(typeof(Grades)).Cast<Grades>()
                .Select(g => (GetColorTitle(g).ToString(), new Action(() => SelectGrade((int)g)))).ToArray();
            SetGrades(gradeMap);
        }

        private int LevelDown()
        {
            Level--;
            if (Level <= 1)
                Level = 1;
            Leveling.SetLevelUi(Level);
            return Level;
        }

        private int LevelUp()
        {
            Leveling.SetLevelUi(++Level);
            return Level;
        }

        private void SelectGrade(int grade) => SelectedGrade = grade;

        private void SetGrades((string grade, Action onSelectedAction)[] gradeTuples)
        {
            foreach (var (grade, onSelectedAction) in gradeTuples)
                InstanceGradeUi(grade, onSelectedAction);
        }

        private void InstanceGradeUi(string grade, Action onSelectedAction) =>
            GradeList.Instance(v =>
            {
                var gradeUi = new GradeUi(v.gameObject, v.GetObject<Text>("text_value"), ui =>
                {
                    onSelectedAction.Invoke();
                    RefreshSelected(ui);
                });
                gradeUi.SetValue(grade);
                return gradeUi;
            });

        private void RefreshSelected(GradeUi btn)
        {
            foreach (var ui in GradeList.List)
                ui.SetSelected(btn == ui);
        }

        public void SetButtons(Action<int> onGenerateAction, Action onRecruitAction)
        {
            Btn_generate.OnClickAdd(() => onGenerateAction(SelectedGrade));
            Btn_recruit.OnClickAdd(onRecruitAction);
        }

        public void SetDiziGrow(Dizi dizi)
        {
            GraUi.SetGrow(dizi.Capable.Grade);
            LevelUi.SetGrow(dizi.Level);
            HpUi.SetGrow(dizi.Hp);
            MpUi.SetGrow(dizi.Mp);
            StrUi.SetGrow(dizi.Strength);
            AgiUi.SetGrow(dizi.Agility);
            StaUi.SetGrow(dizi.Stamina);
        }

        public void SetDizi(Dizi dizi)
        {
            Text_name.text = dizi.Name;
            PropList.ClearList(ui => ui.Destroy());
            GraUi = InstanceAttribTag();
            LevelUi = InstanceAttribTag();
            HpUi = InstanceAttribTag();
            MpUi = InstanceAttribTag();
            StrUi = InstanceAttribTag();
            AgiUi = InstanceAttribTag();
            StaUi = InstanceAttribTag();
            GraUi.Set("品阶", dizi.Capable.Grade);
            LevelUi.Set("等级", dizi.Level);
            HpUi.Set("血", dizi.Hp);
            MpUi.Set("内", dizi.Mp);
            StrUi.Set("力量", dizi.Strength);
            AgiUi.Set("敏捷", dizi.Agility);
            StaUi.Set("体力", dizi.Stamina);
        }

        private PropUi StaUi { get; set; }
        private PropUi AgiUi { get; set; }
        private PropUi StrUi { get; set; }
        private PropUi MpUi { get; set; }
        private PropUi HpUi { get; set; }
        private PropUi LevelUi { get; set; }
        private PropUi GraUi { get; set; }

        private PropUi InstanceAttribTag() => PropList.Instance(v => new PropUi(v));

        private class PropUi : UiBase
        {
            private Text Text_title { get; }
            private Text Text_value { get; }
            private Text Text_pointer { get; }
            private Text Text_grow { get; }

            public PropUi(IView v) : base(v.GameObject, true)
            {
                Text_title = v.GetObject<Text>("text_title");
                Text_value = v.GetObject<Text>("text_value");
                Text_pointer = v.GetObject<Text>("text_pointer");
                Text_grow = v.GetObject<Text>("text_grow");
            }

            public void Set(string title, object value, bool reset = true)
            {
                Text_title.text = title;
                Text_value.text = value.ToString();
                if (reset) SetGrow(null);
            }
            public void SetGrow(object value)
            {
                var text = value?.ToString();
                Text_pointer.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
                Text_grow.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
                Text_grow.text = text;
            }

            public void Destroy()
            {
                Display(false);
                Object.Destroy(gameObject);
            }
        }
        private class GradeUi : UiBase
        {
            private Button button { get; }
            private Text Text_value { get; }
            private Outline outline { get; }
            public GradeUi(GameObject gameObject, Text textValue, Action<GradeUi> onclickAction) : base(gameObject, true)
            {
                button = gameObject.GetComponent<Button>();
                outline = gameObject.GetComponent<Outline>();
                Text_value = textValue;
                button.OnClickAdd(() => onclickAction.Invoke(this));
            }

            public void SetSelected(bool selected) => outline.enabled = selected;
            public void SetValue(string value) => Text_value.text = value;
        }

        private class LevelingUi : UiBase
        {
            private Button Btn_AddLevel { get; }
            private Button Btn_SubLevel { get; }
            private Text Text_Level { get; }

            public LevelingUi(IView v, Action upLevelAction, Action downLevelAction) : base(v.GameObject, true)
            {
                Btn_AddLevel = v.GetObject<Button>("btn_addLevel");
                Btn_AddLevel.OnClickAdd(upLevelAction);
                Btn_SubLevel = v.GetObject<Button>("btn_subLevel");
                Btn_SubLevel.OnClickAdd(downLevelAction);
                Text_Level = v.GetObject<Text>("text_level");
            }

            public void SetLevelUi(int level)
            {
                Text_Level.text = level.ToString();
                Debug.Log($"Level Set = {level}");
            }
        }
    }
    private class StaminaCountWindow : UiBase
    {
        private InputField Input_minutes { get; }
        private InputField Input_stamina { get; }
        private Button Btn_set { get; }
        private Text Text_config { get; }
        private Text Text_last { get; }
        private Text Text_stamina { get; }
        private Text Text_timeUpdate { get; }

        public StaminaCountWindow(GameObject go, InputField inputMinutes, InputField inputStamina, Button btnSet, Text textConfig, Text textLast, Text textStamina, Text textTimeUpdate) : base(go, false)
        {
            Input_minutes = inputMinutes;
            Input_stamina = inputStamina;
            Btn_set = btnSet;
            Text_config = textConfig;
            Text_last = textLast;
            Text_stamina = textStamina;
            Text_timeUpdate = textTimeUpdate;
        }

        public int InputMinutes => int.TryParse(Input_minutes.text, out var value) ? value : 0;
        public int InputStamina => int.TryParse(Input_stamina.text, out var value) ? value : 0;

        public void SetButton(Action onclickAction) => Btn_set.OnClickAdd(onclickAction);

        public void Update(int currentStamina, TimeSpan countdown, TimeSpan minConfig, DateTime lastTick)
        {
            Text_timeUpdate.text = countdown.ToString("g");
            Text_config.text = $"{minConfig.Hours}(小时){minConfig.Minutes}(分钟){minConfig.Seconds}(秒)";
            Text_last.text = lastTick.ToString("T");
            Text_stamina.text = currentStamina.ToString();
        }
    }
    private class MedicineTestWindow : UiBase
    {
        private Transform Trans_con { get; }
        private View Prefab_con { get; }
        private ScrollRect Scroll_items { get; }
        private View Prefab_item { get; }
        private Button BtnUse { get; }

        public MedicineTestWindow(View v, Action<int> onUseMedicineAction) : base(v.gameObject, false)
        {
            Trans_con = v.GetObject<Transform>("trans_con");
            Prefab_con = v.GetObject<View>("prefab_con");
            Scroll_items = v.GetObject<ScrollRect>("scroll_items");
            Prefab_item = v.GetObject<View>("prefab_item");
            BtnUse = v.GetObject<Button>("btn_use");
            BtnUse.OnClickAdd(() => onUseMedicineAction(SelectedId));
            ConListView = new ListViewUi<ConditionSetter>(Prefab_con, Trans_con.gameObject);
            MedicineList = new ListViewUi<MedicineUi>(Prefab_item, Scroll_items.content.gameObject);
            InitConListView();
        }

        private class ConditionSetter
        {
            private InputField Input { get; }
            private Button Btn_setValue { get; }
            private Button Btn_setMax { get; }
            private Button Btn_setFix { get; }
            private Text Text_title { get; }
            private Text Text_value { get; }

            public ConditionSetter(View prefab)
            {
                Input = prefab.GetObject<InputField>("input_con");
                Btn_setValue = prefab.GetObject<Button>("btn_setValue");
                Btn_setMax = prefab.GetObject<Button>("btn_setMax");
                Btn_setFix = prefab.GetObject<Button>("btn_setFix");
                Text_title = prefab.GetObject<Text>("text_title");
                Text_value = prefab.GetObject<Text>("text_value");
            }

            public void Set(Action<int> setValueAction, Action<int> setMaxAction, Action<int> setFixAction)
            {
                Btn_setFix.OnClickAdd(() => setFixAction?.Invoke(GetInputValue()));
                Btn_setMax.OnClickAdd(() => setMaxAction?.Invoke(GetInputValue()));
                Btn_setValue.OnClickAdd(() => setValueAction?.Invoke(GetInputValue()));
            }

            private int GetInputValue() => int.TryParse(Input.text, out var value) ? value : default;

            public void UpdateText(string title, string conditionText)
            {
                Text_title.text = title;
                Text_value.text = conditionText;
            }
        }
        public enum Cons
        {
            Hp,
            Mp,
        }
        private ListViewUi<ConditionSetter> ConListView { get; }
        private Dictionary<Cons, ConditionSetter> ConMap { get; } = new Dictionary<Cons, ConditionSetter>();
        private ConValue Hp { get; set; }
        private ConValue Mp { get; set; }
        private void InitConListView()
        {
            var cons = new[] { Cons.Hp, Cons.Mp };
            foreach (var con in cons)
            {
                var setter = ConListView.Instance(v => new ConditionSetter(v));
                ConMap.Add(con, setter);
            }
        }
        public void Set(Cons con,
            Action<int> setValueAction,
            Action<int> setMaxAction,
            Action<int> setFixAction) =>
            ConMap[con].Set(i =>
            {
                setValueAction(i);
                var c = GetCon(con);
                UpdateConText(con, c.Fix, value: i, c.Max, c.MaxRatio);
            }, i =>
            {
                setMaxAction(i);
                var c = GetCon(con);
                UpdateConText(con, c.Fix, c.Value, max: i, c.MaxRatio);
            }, i =>
            {
                setFixAction(i);
                var c = GetCon(con);
                UpdateConText(con, fix: i, c.Value, c.Max, c.MaxRatio);
            });
        private ConValue GetCon(Cons con)
        {
            return con switch
            {
                Cons.Hp => Hp,
                Cons.Mp => Mp,
                _ => throw new ArgumentOutOfRangeException(nameof(con), con, null)
            };
        }
        public void UpdateCon(Cons con, ConValue c)
        {
            switch (con)
            {
                case Cons.Hp: Hp = c; break;
                case Cons.Mp: Mp = c; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(con), con, null);
            }
            var max = c.Max;
            var value = c.Value;
            var fix = c.Fix;
            UpdateConText(con, fix, value, max, 1f * max / fix);
        }

        private void UpdateConText(Cons con, int fix, int value, int max, double ratio)
        {
            var ratioText = (int)(ratio * 100);
            ConMap[con].UpdateText($"{con}({fix})", $"{value}/{max}({ratioText}%)");
        }

        private int SelectedId { get; set; }
        public void UpdateMedicines(ObjectBag bag)
        {
            var list = bag.Get<DiziTestManager.MedicineUi[]>(0).ToArray();
            MedicineList.ClearList(ui =>
            {
                ui.Display(false);
                ui.Destroy();
            });
            foreach (var m in list)
                MedicineList.Instance(v => new MedicineUi(v, m.Name, ui =>
                {
                    SelectedId = m.Id;
                    foreach (var medicineUi in MedicineList.List)
                        medicineUi.SetSelected(ui == medicineUi);
                }));
        }

        private ListViewUi<MedicineUi> MedicineList { get; }

        private class MedicineUi : UiBase
        {
            private Text Text_title { get; }
            private Image Img_selected { get; }
            private Button Btn { get; }

            public MedicineUi(View v, string title, Action<MedicineUi> onclickAction)
                : base(v.gameObject, true)
            {
                Text_title = v.GetObject<Text>("text_title");
                Img_selected = v.GetObject<Image>("img_selected");
                Text_title.text = title;
                Btn = v.gameObject.GetComponent<Button>();
                Btn.OnClickAdd(() => onclickAction(this));
                Display(true);
            }

            public void SetSelected(bool selected) => Img_selected.gameObject.SetActive(selected);
        }
    }
    internal class Dizi
    {
        public string Name { get; private set; }
        public int Strength { get; private set; }
        public int Agility { get; private set; }
        public int Hp { get; private set; }
        public int Mp { get; private set; }
        public int Level { get; private set; }
        public int Stamina { get; private set; }
        public Capable Capable { get; private set; }
        public Dictionary<int, int> Condition { get; private set; }
    }

    internal class Capable
    {
        /// <summary>
        /// 品级
        /// </summary>
        public int Grade { get; private set; }
        /// <summary>
        /// 轻功格
        /// </summary>
        public int DodgeSlot { get; private set; }
        /// <summary>
        /// 武功格
        /// </summary>
        public int MartialSlot { get; private set; }
        /// <summary>
        /// 背包格
        /// </summary>
        public int InventorySlot { get; private set; }
    }

}