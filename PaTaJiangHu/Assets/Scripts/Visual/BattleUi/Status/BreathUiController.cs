using System;
using System.Collections;
using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;
using Visual.BattleUi.Input;

namespace Visual.BattleUi.Status
{
    public class BreathUiController : UiBase
    {
        [SerializeField] private Slider leftSlider;
        [SerializeField] private Text leftText;
        [SerializeField] private Slider rightSlider;
        [SerializeField] private Text rightText;
        [SerializeField] private Image _drumImage;
        [SerializeField] private float _moveSecs = 1f;
        [SerializeField] private BreathViewUi _leftView;
        [SerializeField] private BreathViewUi _rightView;

        public IEnumerator SetLeft(CombatPlans plan, int busy, int charge, 
            BreathRecord attack, BreathRecord exert, BreathRecord placing) => 
            SetPlan(_leftView, plan, busy, charge, 
            DataConvertOrDefault(attack),
            DataConvertOrDefault(exert),
            DataConvertOrDefault(placing));
        public IEnumerator SetLeft(IBreathBar breathBar)
        {
            yield return SetPlan(_leftView, breathBar.Plan, breathBar.TotalBusies, breathBar.TotalCharged,
                DataConvertOrDefault(breathBar.Combat),
                DataConvertOrDefault(breathBar.Force),
                DataConvertOrDefault(breathBar.Dodge));
        }
        public IEnumerator SetRight(CombatPlans plan, int busy, int charge,
            BreathRecord attack, BreathRecord exert, BreathRecord placing) =>
            SetPlan(_rightView, plan, busy, charge,
                DataConvertOrDefault(attack),
                DataConvertOrDefault(exert),
                DataConvertOrDefault(placing));
        public IEnumerator SetRight(IBreathBar breathBar)
        {
            yield return SetPlan(_rightView, breathBar.Plan, breathBar.TotalBusies, breathBar.TotalCharged,
                DataConvertOrDefault(breathBar.Combat),
                DataConvertOrDefault(breathBar.Force),
                DataConvertOrDefault(breathBar.Dodge));
        }

        private static (string Name, int Value) DataConvertOrDefault(BreathRecord rec) => 
            rec == null ? default : (rec.Name, rec.Value);
        private static (string Name, int Value) DataConvertOrDefault<T>(T rec) where T : IBreathNode, ISkillName =>
            rec == null ? default : (rec.Name, rec.Breath);

        private IEnumerator SetPlan(BreathViewUi view, CombatPlans plan,
            int totalBusies, int totalCharged,
            (string Name, int Value) attack, (string Name, int Value) exert, (string Name, int Value) placing)
        {
            switch (plan)
            {
                case CombatPlans.Attack:
                    if (placing == default)
                        yield return view.SetAttack(totalBusies, totalCharged, attack.Name, attack.Value);
                    else
                        yield return view.SetAttackWithDodge(totalBusies, totalCharged, attack.Name, attack.Value,
                            placing.Name,
                            placing.Value);
                    break;
                case CombatPlans.RecoverHp:
                case CombatPlans.RecoverTp:
                    yield return view.SetExert(totalBusies, totalCharged, exert.Name, exert.Value);
                    break;
                case CombatPlans.Wait:
                case CombatPlans.Surrender:
                    yield return view.SetIdle(totalBusies, totalCharged);
                    break;
                case CombatPlans.Exert:
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void SetDrum(int breath, int maxBreath, bool isLeft)
        {
            if (maxBreath <= 0)
            {
                leftSlider.value = 0;
                rightSlider.value = 0;
                leftText.text = string.Empty;
                rightText.text = string.Empty;
                return;
            }
            SetSlider(isLeft ? leftSlider : rightSlider, breath, maxBreath);
            SetText(isLeft ? leftText : rightText, breath);
            void SetText(Text text, int value) => text.text = value.ToString();
            void SetSlider(Slider slider, int value, int max) => slider.value = 1f * value / max;
        }

        public void UpdateDrum(int left, int right, int maxBreath)
        {
            if (maxBreath <= 0)
            {
                leftSlider.value = 0;
                rightSlider.value = 0;
                leftText.text = string.Empty;
                rightText.text = string.Empty;
                return;
            }
            leftSlider.value = 1f * left / maxBreath;
            rightSlider.value = 1f * right / maxBreath;
            leftText.text = left.ToString();
            rightText.text = right.ToString();
            //_map[combatId].SetBreath(breath, maxBreath);
        }

        public IEnumerator PlaySlider()
        {
            var tween = DOTween.Sequence().Pause();
            var (far, close) = leftSlider.value > rightSlider.value
                ? (leftSlider, rightSlider)
                : (rightSlider, leftSlider);
            /* 1.找出最靠近0的值
             * 2.最远的-最近的，并停留在那儿
             * 3.找出
             */
            var farStop = far.value - close.value;
            tween.Join(far.DOValue(far.value - farStop, _moveSecs)).Join(close.DOValue(0, _moveSecs));
            yield return tween.Play().OnComplete(()=> _drumImage.transform.DOShakeScale(0.5f)).WaitForCompletion();
        }

        public override void ResetUi()
        {
            _leftView.ResetUi();
            _rightView.ResetUi();
        }
    }
}