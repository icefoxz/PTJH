using BattleM;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi
{
    public class CombatStatusUi : UiBase
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private ConValueSliderUi _hpSliderUi;
        [SerializeField] private Text _hpText;
        [SerializeField] private ConValueSliderUi _tpSliderUi;
        [SerializeField] private Text _tpText;
        [SerializeField] private ConValueSliderUi _mpSliderUi;
        [SerializeField] private Text _mpText;
        [SerializeField] private Slider _breathSlider;
        [SerializeField] private Text _breathText;
        [SerializeField] private Image loseImage;

        public void Set(string title, ICombatStatus stat, int breath,
            int maxBreath)
        {
            var hp = stat.Hp;
            var tp = stat.Tp;
            var mp = stat.Mp;
            _nameText.text = title;
            _hpSliderUi.Set(hp);
            SetConText(_hpText, hp);
            _tpSliderUi.Set(tp);
            SetConText(_tpText, tp);
            _mpSliderUi.Set(mp);
            SetConText(_mpText, mp);
            SetBreath(breath, maxBreath);
        }

        private static void SetConText(Text com, IConditionValue con) => com.text = $"【{con?.Value}/{con?.Max}】";
        public void SetLosePanel(bool isLose) => loseImage.gameObject.SetActive(isLose);
        public override void ResetUi()
        {
            _nameText.text = string.Empty;
            SetBreath(1, 1);
            _hpSliderUi.ResetUi();
            SetConText(_hpText, default);
            _tpSliderUi.ResetUi();
            SetConText(_tpText, default);
            _mpSliderUi.ResetUi();
            SetConText(_mpText, default);
            SetLosePanel(false);
        }

        public void SetBreath(int breath, int maxBreath)
        {
            if (maxBreath == 0) maxBreath = 1;
            _breathSlider.value = 1f * breath / maxBreath;
            _breathText.text = $"({breath})";
        }

        public void UpdateStatus(IConditionValue hp, IConditionValue tp, IConditionValue mp)
        {
            _hpSliderUi.Set(hp);
            SetConText(_hpText, hp);
            _tpSliderUi.Set(tp);
            SetConText(_tpText, tp);
            _mpSliderUi.Set(mp);
            SetConText(_mpText, mp);
        }
    }
}