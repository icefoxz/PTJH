using BattleM;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Status
{
    public class CombatStatusUi : UiBase
    {
        [SerializeField] private Text _nameText;
        [SerializeField] private ConValueSliderUi _hpSliderUi;
        [SerializeField] private Text _hpText;
        [SerializeField] private ConValueSliderUi _mpSliderUi;
        [SerializeField] private Text _mpText;
        //[SerializeField] private Slider _breathSlider;
        //[SerializeField] private Text _breathText;
        //[SerializeField] private Image loseImage;

        public void Set(string title, ICombatStatus stat)
        {
            var hp = stat.Hp;
            var mp = stat.Mp;
            _nameText.text = title;
            _hpSliderUi.Set(hp);
            _mpSliderUi.Set(mp);
            SetConText(_hpText, hp);
            SetConText(_mpText, mp);
            //SetBreath(breath, maxBreath);
        }

        private static void SetConText(Text com, IConditionValue con) => com.text = $"{con?.Value}/{con?.Max}";
        //public void SetLosePanel(bool isLose) => loseImage.gameObject.SetActive(isLose);
        public override void ResetUi()
        {
            _nameText.text = string.Empty;
            //SetBreath(1, 1);
            _hpSliderUi.ResetUi();
            SetConText(_hpText, default);
            _mpSliderUi.ResetUi();
            SetConText(_mpText, default);
            //SetLosePanel(false);
        }

        //public void SetBreath(int breath, int maxBreath)
        //{
        //    if (maxBreath == 0) maxBreath = 1;
        //    _breathSlider.value = 1f * breath / maxBreath;
        //    _breathText.text = $"({breath})";
        //}
        public void UpdateSlider(int hp, int fixHp, int mp, int fixMp)
        {
            _hpSliderUi.SetValue(hp, fixHp);
            _mpSliderUi.SetValue(mp, fixMp);
        }

        public void UpdateText(IConditionValue hp, IConditionValue mp)
        {
            SetConText(_hpText, hp);
            SetConText(_mpText, mp);
        }

        public void UpdateStatus(IConditionValue hp, IConditionValue mp)
        {
            _hpSliderUi.Set(hp);
            _mpSliderUi.Set(mp);
            SetConText(_hpText, hp);
            SetConText(_mpText, mp);
        }
    }
}