using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Input
{
    public class CombatFormUi : BaseUi
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _breathText;
        [SerializeField] private Image _panel;

        public void Set(string title,int breath)
        {
            _titleText.text = title;
            _breathText.text = breath.ToString();
        }

        public void SetPanel(bool display) => Display(display, _panel);
        public override void ResetUi()
        {
            _titleText.text = string.Empty;
            _breathText.text = string.Empty;
            Display(false, _panel);
            Hide();
        }
    }
}