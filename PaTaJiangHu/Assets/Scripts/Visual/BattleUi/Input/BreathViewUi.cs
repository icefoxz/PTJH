using System;
using System.Collections;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Input
{
    public class BreathViewUi : BaseUi
    {
        private enum ViewModes
        {
            Attack,
            AttackWithDodge,
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
        [SerializeField] private Transform _pool;
        [SerializeField] private StackingScroller _scrollingScroller;
        private IStackingScroller StackingScroller => _scrollingScroller;
        private int Busy { get; set; }
        private int Charge { get; set; }

        public IEnumerator SetAttack(int busy, int charge, string title,int breath)
        {
            SetBusyCharge(busy, charge);
            SetForm(_combatForm, title, breath);
            yield return SetMode(ViewModes.Attack, breath);
        }

        public IEnumerator SetAttackWithDodge(int busy, int charge, string combatTitle, int combatBreath, string dodgeTitle, int dodgeBreath)
        {
            SetBusyCharge(busy, charge);
            SetForm(_combatForm, combatTitle, combatBreath);
            SetForm(_dodgeForm, dodgeTitle, dodgeBreath);
            yield return SetMode(ViewModes.AttackWithDodge, combatBreath, dodgeBreath);
        }

        public IEnumerator SetExert(int busy, int charge, string exertTitle,int breath)
        {
            SetBusyCharge(busy, charge);
            SetForm(_forceForm, exertTitle, breath);
            yield return SetMode(ViewModes.Recover, breath);
        }

        public IEnumerator SetIdle(int busy, int charge)
        {
            SetBusyCharge(busy, charge);
            yield return SetMode(ViewModes.Idle, 0);
        }

        private void SetBusyCharge(int busy, int charge)
        {
            Busy = busy;
            Charge = charge;
        }

        private IEnumerator SetMode(ViewModes mode, int breath,int dodgeBreath = 0)
        {
            ResetElements();
            yield return BusyChargeBreathUpdate();
            Display(mode == ViewModes.Recover, _forceForm);
            Display(mode == ViewModes.AttackWithDodge, _dodgeForm);
            Display(mode is ViewModes.Attack or ViewModes.AttackWithDodge, _combatForm);
            Display(mode == ViewModes.Idle, _respite);
            var breaths = breath + dodgeBreath;
            switch (mode)
            {
                case ViewModes.Attack:
                    ResetTransformToPool(_dodgeForm.transform,_forceForm.transform);
                    yield return PlaceStakingScroller(_combatForm.transform);
                    break;
                case ViewModes.AttackWithDodge:
                    ResetTransformToPool(_forceForm.transform);
                    yield return PlaceStakingScroller(_dodgeForm.transform);
                    BreathUpdate(dodgeBreath + Busy - Charge);
                    yield return PlaceStakingScroller(_combatForm.transform);
                    break;
                case ViewModes.Recover:
                    ResetTransformToPool(_dodgeForm.transform, _combatForm.transform);
                    yield return PlaceStakingScroller(_forceForm.transform);
                    break;
                case ViewModes.Idle: break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            BreathUpdate(breaths + Busy - Charge);
        }

        private void ResetElements()
        {
            BreathUpdate(0);
            StackingScroller.ClearList();
            _combatForm.Hide();
            _forceForm.Hide();
            _dodgeForm.Hide();
            Display(false, _respite);
        }

        private static void SetForm(CombatFormUi displayForm,string title,int breath)
        {
            displayForm.Set(title, breath);
            displayForm.SetPanel(false);
            displayForm.Show();
        }

        private void BreathUpdate(int value) => _totalBreath.text = value.ToString();

        private IEnumerator BusyChargeBreathUpdate()
        {
            var breath = Busy - Charge;
            if (breath >= 0)
            {
                _busyValue.text = breath.ToString();
                if (breath == 0)
                {
                    Display(false, _busyObj, _chargeObj);
                    ResetTransformToPool(_busyObj.transform, _chargeObj.transform);
                }
                else
                {
                    Display(false, _chargeObj);
                    Display(true, _busyObj);
                    ResetTransformToPool(_chargeObj.transform);
                    yield return PlaceStakingScroller(_busyObj.transform);
                }
            }
            else
            {
                Display(false, _busyObj);
                Display(true, _chargeObj);
                _chargeValue.text = Math.Abs(breath).ToString();
                    ResetTransformToPool(_busyObj.transform);
                yield return PlaceStakingScroller(_chargeObj.transform);
            }
            BreathUpdate(breath);
        }

        private void ResetTransformToPool(params Transform[] tran) => tran.ForEach(t =>
        {
            t.SetParent(_pool);
            StackingScroller.RemoveTransform(t);
        });
        private IEnumerator PlaceStakingScroller(Transform tran)
        {
            yield return StackingScroller.PlaceAndWaitStacking(tran, 0.5f);
        }

        public override void ResetUi()
        {
            StackingScroller.ClearList();
            var zero = 0.ToString();
            ResetTransformToPool(_chargeObj.transform, 
                _busyObj.transform, 
                _combatForm.transform, 
                _forceForm.transform, 
                _dodgeForm.transform);
            _totalBreath.text = zero;
            _busyValue.text = zero;
            _dodgeForm.ResetUi();
            _combatForm.ResetUi();
            _forceForm.ResetUi();
            Display(false, _respite);
        }
    }
}