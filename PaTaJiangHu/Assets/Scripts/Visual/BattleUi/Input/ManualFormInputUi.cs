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

        private event UnityAction<IForceForm> OnExertAction;
        private event UnityAction<ICombatForm> OnAttackAction;

        public void Init(UnityAction<ICombatForm> onAttackAction,
            UnityAction<IForceForm> onExertAction,
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
            var combatSkill = unit.CombatSkill;
            for (var i = 0; i < combatSkill.Combats.Count; i++)
            {
                var form = combatSkill.Combats[i];
                var isReady = unit.IsCombatFormAvailable(form);
                combatFormView.AddOption(ui =>
                {
                    ui.Init(() => onPointerDown.Invoke(form));
                    ui.Interaction(isReady);
                    if (isReady)
                        ui.Set(() =>
                            {
                                combatFormView.SetSelected(ui);
                                OnAttackAction?.Invoke(form);
                            }, form.Name, form.Breath.ToString(), form.Mp.ToString(), form.Qi.ToString(),
                            form.TarBusy.ToString(),
                            form.OffBusy.ToString());
                });
                combatFormView.Show();
            }

            Show();
        }

        public void SetForce(ICombatUnit unit, UnityAction<IForceForm> onPointerDown)
        {
            for (var i = 0; i < unit.ForceSkill.Forms.Count; i++)
            {
                var form = unit.ForceSkill.Forms[i];
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
            }

            Show();
        }

        public override void ResetUi()
        {
            combatFormView.ResetUi();
            forceFormView.ResetUi();
        }

    }
}