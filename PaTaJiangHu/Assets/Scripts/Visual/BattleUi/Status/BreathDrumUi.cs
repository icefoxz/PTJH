using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Status
{
    public class BreathDrumUi : UiBase
    {
        [SerializeField] private Slider leftSlider;
        [SerializeField] private Text leftText;
        [SerializeField] private Slider rightSlider;
        [SerializeField] private Text rightText;
        [SerializeField] private Image _drumImage;
        [SerializeField] private float _moveSecs = 1f;
        public void UpdateBreath(int left, int right, int maxBreath)
        {
            if (maxBreath <= 0)
            {
                leftSlider.value = 1;
                rightSlider.value = 1;
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

        public void PlayDrum(UnityAction afterComplete)
        {
            StopAllCoroutines();
            StartCoroutine(PlaySlider(afterComplete));
        }

        public IEnumerator PlaySlider(UnityAction afterComplete)
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
            yield return tween.Play().OnComplete(()=>
            {
                _drumImage.transform.DOShakeScale(0.5f);
                afterComplete.Invoke();
            }).WaitForCompletion();
        }

        public override void ResetUi()
        {
        }
    }
}