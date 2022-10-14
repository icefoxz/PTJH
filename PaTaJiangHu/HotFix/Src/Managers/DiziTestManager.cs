using System;
using System.Linq;
using HotFix_Project.Models.Charators;
using HotFix_Project.Serialization.LitJson;
using HotFix_Project.Views.Bases;
using Server;
using Server.Controllers.Characters;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;
using Object = UnityEngine.Object;

namespace HotFix_Project.Managers;

public class DiziTestManager
{
    private DiziGenWindow GenWindow { get; set; }
    private StaminaCountWindow StaCountWindow { get; set; }
    public void Init()
    {
        InitUi();
        EventReg();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_diziGenerate", (go, v) =>
        {
            GenWindow = new DiziGenWindow(gameObject: go,
                textName: v.GetObject<Text>("text_name"),
                btnGenerate: v.GetObject<Button>("btn_generate"),
                btnRecruit: v.GetObject<Button>("btn_recruit"),
                transContent: v.GetObject<Transform>("trans_content"),
                prefabAttribute: v.GetObject<View>("prefab_attribute"),
                gradeViewContent: v.GetObject<Transform>("trans_gradeContent"),
                gradePrefab: v.GetObject<View>("prefab_grade"));
            GenWindow.SetButtons(ServiceCaller.Instance.OnGenerateDizi, () => GenWindow.Display(false));
        });
        Game.UiBuilder.Build("view_diziStamina", (go, v) =>
        {
            StaCountWindow = new StaminaCountWindow(go,
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
            StaCountWindow.SetButton(() => ServiceCaller.Instance.OnSetStamina(
                StaCountWindow.InputStamina,
                StaCountWindow.InputMinutes));
        });
    }

    private void EventReg()
    {
        Game.MessagingManager.RegEvent(EventString.Test_DiziGenerate, arg=> GenWindow.SetDizi(JsonMapper.ToObject<Dizi>(arg)));
        Game.MessagingManager.RegEvent(EventString.Test_DiziRecruit, _ => GenWindow.Display(true));
        Game.MessagingManager.RegEvent(EventString.Test_StaminaWindow, _ => StaCountWindow.Display(true));
    }
    private class DiziGenWindow : UiBase
    {
        private enum Grade
        {
            D,
            C,
            B,
            A,
            S
        }
        private Text Text_name { get; }
        private Button Btn_generate { get; }
        private Button Btn_recruit { get; }
        private Transform Trans_content { get; }
        private View Prefab_attribute { get; }
        private ListViewUi<PropUi> PropList { get; }
        private ListViewUi<GradeUi> GradeList { get; }

        private int SelectedGrade { get; set; }

        public DiziGenWindow(GameObject gameObject, Text textName, Button btnGenerate, Button btnRecruit,
            Transform transContent, View prefabAttribute,Transform gradeViewContent,View gradePrefab) 
            : base(gameObject, false)
        {
            Text_name = textName;
            Btn_generate = btnGenerate;
            Btn_recruit = btnRecruit;
            Trans_content = transContent;
            Prefab_attribute = prefabAttribute;
            PropList = new ListViewUi<PropUi>(Prefab_attribute, Trans_content.gameObject);
            GradeList = new ListViewUi<GradeUi>(gradePrefab, gradeViewContent.gameObject);
            var gradeMap = Enum.GetValues(typeof(Grade)).Cast<int>()
                .Select(g => (((Grade)g).ToString(), new Action(() => SelectGrade(g)))).ToArray();
            SetGrades(gradeMap);
        }

        private void SelectGrade(int grade)
        {
            SelectedGrade = grade;
        }

        private void SetGrades((string grade, Action onSelectedAction)[] gradeTuples)
        {
            foreach (var (grade, onSelectedAction) in gradeTuples)
                InstanceGradeUi(grade, onSelectedAction);
        }

        private void InstanceGradeUi(string grade, Action onSelectedAction)
        {
            GradeList.Instance(v =>
            {
                var gradeUi = new GradeUi(v.gameObject, v.GetObject<Text>("text_value") , ui =>
                {
                    onSelectedAction.Invoke();
                    RefreshSelected(ui);
                });
                gradeUi.SetValue(grade);
                return gradeUi;
            });
        }

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
        public void SetDizi(Dizi dizi)
        {
            Text_name.text = dizi.Name;
            PropList.ClearList(ui=>ui.Destroy());
            InstanceAttribTag().Set("血", dizi.Hp);
            InstanceAttribTag().Set("气", dizi.Tp);
            InstanceAttribTag().Set("内", dizi.Mp);
            InstanceAttribTag().Set("敏捷", dizi.Agility);
            InstanceAttribTag().Set("力量", dizi.Strength);
            InstanceAttribTag().Set("等级", dizi.Level);
            InstanceAttribTag().Set("执行力", dizi.Stamina);
            InstanceAttribTag().Set("品阶", dizi.Capable.Grade);
        }

        private PropUi InstanceAttribTag()
        {
            return PropList.Instance(v =>
            {
                var obj = new PropUi(v.gameObject,
                    v.GetObject<Text>("text_title"),
                    v.GetObject<Text>("text_value")
                );
                return obj;
            });
        }

        private class PropUi : UiBase
        {
            private Text Text_title { get; }
            private Text Text_value { get; }

            public PropUi(GameObject gameObject, Text textTitle, Text textValue) : base(gameObject, true)
            {
                Text_title = textTitle;
                Text_value = textValue;
            }

            public void Set(string title, object value)
            {
                Text_title.text = title;
                Text_value.text = value.ToString();
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

        public void Update(int currentStamina,TimeSpan countdown, TimeSpan minConfig, DateTime lastTick)
        {
            Text_timeUpdate.text = countdown.ToString("g");
            Text_config.text = $"{minConfig.Hours}(小时){minConfig.Minutes}(分钟){minConfig.Seconds}(秒)";
            Text_last.text = lastTick.ToString("T");
            Text_stamina.text = currentStamina.ToString();
        }
    }
}