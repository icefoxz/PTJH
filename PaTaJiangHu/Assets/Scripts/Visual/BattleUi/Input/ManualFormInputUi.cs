using System;
using BattleM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class ManualFormInputUi : UiBase
    {
        [SerializeField] private SkillFormView combatFormView;
        [SerializeField] private SkillFormView forceFormView;
        [SerializeField] private Button recHpButton;
        [SerializeField] private Button recTpButton;

        private event UnityAction<IForce> OnExertAction;
        private event UnityAction<ICombatForm> OnAttackAction;

        public void Init(UnityAction<ICombatForm> onAttackAction,
            UnityAction<IForce> onExertAction,
            UnityAction onRecHpAction, UnityAction onRecTpAction)
        {
            combatFormView.Init();
            forceFormView.Init();
            recHpButton.onClick.AddListener(onRecHpAction);
            recTpButton.onClick.AddListener(onRecTpAction);
            OnAttackAction = onAttackAction;
            OnExertAction = onExertAction;
        }

        public void SetCombat(ICombatUnit unit, UnityAction<ICombatForm> onPointerDown)
        {
            var forms = unit.CombatForms;
            //for (var i = 0; i < forms.Length; i++)
            //{
            //    var form = forms[i];
            //    var isReady = unit.IsCombatFormAvailable(form);
            //    combatFormView.AddOption(ui =>
            //    {
            //        ui.Init(() => onPointerDown.Invoke(form));
            //        ui.Interaction(isReady);
            //        if (isReady)
            //            ui.Set(() =>
            //                {
            //                    combatFormView.SetSelected(ui);
            //                    OnAttackAction?.Invoke(form);
            //                }, form.Name, form.Breath.ToString(), form.Mp.ToString(), form.Tp.ToString(),
            //                form.TarBusy.ToString(),
            //                form.OffBusy.ToString());
            //    });
            //    combatFormView.Show();
            //}

            Show();
        }

        public void SetForce(ICombatUnit unit, UnityAction<IForce> onPointerDown)
        {
            var form = unit.Force;
            forceFormView.AddOption(ui =>
            {
                ui.Init(() => onPointerDown.Invoke(form));
                ui.Set(() =>
                {
                    forceFormView.SetSelected(ui);
                    OnExertAction?.Invoke(form);
                }, form.Name, form.Breath.ToString());
            });
            forceFormView.Show();
            Show();
        }

        public override void ResetUi()
        {
            combatFormView.ResetUi();
            forceFormView.ResetUi();
        }

    }
}