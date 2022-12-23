using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Input
{
    public class ManualAutoSwitch : BaseUi
    {
        [SerializeField] private Text _autoText;
        [SerializeField] private Text _manualText;
        [SerializeField] private Button _button;
        private bool IsManual { get; set; }

        public void Init(UnityAction<bool> onclickAction,bool presetManual = false)
        {
            IsManual = presetManual;
            _button.onClick.AddListener(() =>
            {
                IsManual = !IsManual;
                onclickAction?.Invoke(IsManual);
                UpdateUi(IsManual);
            });
            UpdateUi(IsManual);
        }


        private void UpdateUi(bool isManual)
        {
            _autoText.gameObject.SetActive(!isManual);
            _manualText.gameObject.SetActive(isManual);
        }
        public override void ResetUi() { }
    }
}