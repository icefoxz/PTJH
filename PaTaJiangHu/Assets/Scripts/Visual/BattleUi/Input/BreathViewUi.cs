using System;
using BattleM;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class BreathViewUi : UiBase
    {
        private enum ViewModes
        {
            Attack,
            Recover,
            Idle
        }
        [SerializeField] private Text _totalBreath;
        [SerializeField] private Text _busyValue;
        [SerializeField] private Image _busyObj;
        [SerializeField] private Text _chargeValue;
        [SerializeField] private Image  _chargeObj;
        [SerializeField] private CombatFormUi _dodgeForm;
        [SerializeField] private CombatFormUi _combatForm;
        [SerializeField] private CombatFormUi _forceForm;
        [SerializeField] private Image _respite;
        private IDodgeForm DodgeForm { get; set; }
        private int Busy { get; set; }
        private int Charge { get; set; }

        public void Set(int busy,int charge ,IDodgeForm dodgeForm)
        {
            Busy = busy;
            Charge = charge;
            DodgeForm = dodgeForm;
        }


        public void SetCombat(ICombatForm form)
        {
            SetForm(form, _combatForm);
            SetForm<IForceForm>(null, _forceForm);
            SetForm(DodgeForm, _dodgeForm);
            UpdateBreath(ViewModes.Attack, form);
        }

        public void SetForce(IForceForm form)
        {
            SetForm<ICombatForm>(null, _combatForm);
            SetForm(form, _forceForm);
            SetForm<IDodgeForm>(null, _dodgeForm);
            UpdateBreath(ViewModes.Recover, form);
        }

        public void SetIdle()
        {
            SetForm<ICombatForm>(null, _combatForm);
            SetForm<IForceForm>(null, _forceForm);
            SetForm<IDodgeForm>(null, _dodgeForm);
            UpdateBreath<ICombatForm>(ViewModes.Idle, null);
        }

        public void ClearActionForms()
        {
            SetForm<ICombatForm>(null, _combatForm);
            SetForm<IForceForm>(null, _forceForm);
            SetForm(DodgeForm, _dodgeForm);
            UpdateBreath<ICombatForm>(ViewModes.Attack, null);
        }

        private static void SetForm<T>(T form, CombatFormUi displayForm)
            where T : IBreathNode,ISkillForm
        {
            if (form == null)
            {
                displayForm.Hide();
                return;
            }
            displayForm.Set(form.Name, form.Breath);
            displayForm.SetPanel(true);
            displayForm.Show();
        }

        private void UpdateBreath<T>(ViewModes mode,T form) where T : IBreathNode
        {
            if (Busy >= Charge)
            {
                var busy = Busy - Charge;
                _busyValue.text = Busy.ToString();
                if (busy == 0)
                {
                    Display(false, _busyObj, _chargeObj);
                }
                else
                {
                    _busyValue.text = busy.ToString();
                    Display(false, _chargeObj);
                    Display(true, _busyObj);
                }
            }
            else
            {
                var charge = Charge - Busy;
                Display(false, _busyObj);
                Display(true, _chargeObj);
                _chargeValue.text = charge.ToString();
            }
            var dodgeBreath = mode == ViewModes.Attack ? DodgeForm?.Breath ?? 0 : 0;
            var actionBreath = form?.Breath ?? 0;
            _totalBreath.text = (dodgeBreath + actionBreath + Busy - Charge).ToString();
            Display(mode == ViewModes.Idle, _respite);
        }


        public override void ResetUi()
        {
            var zero = 0.ToString();
            _totalBreath.text = zero;
            _busyValue.text = zero;
            _dodgeForm.ResetUi();
            _combatForm.ResetUi();
            _forceForm.ResetUi();
            Display(false, _respite);
        }
    }
}