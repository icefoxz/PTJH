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
        [SerializeField] private PointerButton idleButton;

        private event Action<IForceForm> OnExertAction;
        private event Action<ICombatForm> OnAttackAction;

        public void Init(Action<ICombatForm> onAttackAction,
            Action<IForceForm> onExertAction,
            Action onIdleAction)
        {
            combatFormView.Init();
            forceFormView.Init();
            idleButton.onClick.AddListener(onIdleAction.Invoke);
            OnAttackAction = onAttackAction;
            OnExertAction = onExertAction;
        }

        public void SetIdle(UnityAction idleAction)
        {
            idleButton.OnPointerDownEvent.RemoveAllListeners();
            idleButton.OnPointerDownEvent.AddListener(_=>idleAction.Invoke());
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