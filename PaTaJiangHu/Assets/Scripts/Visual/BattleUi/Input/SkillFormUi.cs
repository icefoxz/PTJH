using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class SkillFormUi : UiBase
    {
        [SerializeField] private PointerButton _button;
        [SerializeField] private Text _title;
        [SerializeField] private Text _mp;
        [SerializeField] private Text _breath;
        [SerializeField] private Text _opBusy;
        [SerializeField] private Text _tarBusy;
        [SerializeField] private Outline _outLine;

        public void Init(UnityAction onPointerDown)
        {
            _button.OnPointerDownEvent.RemoveAllListeners();
            _button.OnPointerDownEvent.AddListener(_ => onPointerDown.Invoke());
        }
        public void Set(UnityAction onclickAction, string title, string breath, string mp = null, string tp = null,
            string tarBusy = null, string opBusy = null)
        {
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(onclickAction);
            _title.text = title;
            _breath.text = Bracket(breath);
            _mp.text = string.IsNullOrWhiteSpace(mp) ? string.Empty : Bracket(mp);
            _opBusy.text = string.IsNullOrWhiteSpace(opBusy) ? string.Empty : Bracket(opBusy);
            _tarBusy.text = string.IsNullOrWhiteSpace(tarBusy) ? string.Empty : Bracket(tarBusy);
            _outLine.enabled = false;
            Show();

            string Bracket(string text) => $"({text})";
        }

        public void SetSelected(bool isSelected) => _outLine.enabled = isSelected;
        public override void ResetUi()
        {
            _button.interactable = true;
            _outLine.enabled = false;
            _button.onClick.RemoveAllListeners();
            _title.text = string.Empty;
            _mp.text = string.Empty;
            _breath.text = string.Empty;
            _opBusy.text = string.Empty;
            _tarBusy.text = string.Empty;
            Hide();
        }

        public void Interaction(bool enable) => _button.interactable = enable;
    }
}