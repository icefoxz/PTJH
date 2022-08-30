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
            Exert,
            Idle
        }
        [SerializeField] private Text _totalBreath;
        [SerializeField] private Text _busyValue;
        [SerializeField] private CombatFormUi _dodgeForm;
        [SerializeField] private CombatFormUi _combatForm;
        [SerializeField] private CombatFormUi _forceForm;
        private IDodgeForm DodgeForm { get; set; }
        private int Busy { get; set; }

        public void Set(int busy, IDodgeForm dodgeForm)
        {
            Busy = busy;
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
            UpdateBreath(ViewModes.Exert, form);
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
            _busyValue.text = Busy.ToString();
            var dodgeBreath = mode == ViewModes.Attack ? DodgeForm?.Breath ?? 0 : 0;
            var actionBreath = form?.Breath ?? 0;
            _totalBreath.text = (dodgeBreath + actionBreath + Busy).ToString();
        }


        public override void ResetUi()
        {
            var zero = 0.ToString();
            _totalBreath.text = zero;
            _busyValue.text = zero;
            _dodgeForm.ResetUi();
            _combatForm.ResetUi();
            _forceForm.ResetUi();
        }
    }
}