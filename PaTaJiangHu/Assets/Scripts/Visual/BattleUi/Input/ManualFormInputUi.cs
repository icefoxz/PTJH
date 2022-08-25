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
        private enum Modes
        {
            Attack,
            Exert
        }
        [SerializeField] private SkillFormView skillFormView;
        [SerializeField] private SkillFormView dodgeFormView;
        [SerializeField] private Button CombatTabBtn;
        [SerializeField] private Button ExertTabBtn;
        [SerializeField] private CombatTab _combatTab;
        [SerializeField] private ExertTab _exertTab;
        [Serializable] private class CombatTab
        {
            [SerializeField] public Button combatButton;
            [SerializeField] public GameObject tabPanel;
            [SerializeField] public GameObject body;

            public void Init(UnityAction onAttackClick)
            {
                combatButton.onClick.AddListener(onAttackClick);
            }
            public void SetMode(Modes mode)
            {
                body.gameObject.SetActive(mode == Modes.Attack);
                tabPanel.gameObject.SetActive(mode != Modes.Attack);
            }
        }
        [Serializable] private class ExertTab
        {
            [SerializeField] public GameObject tabPanel;
            [SerializeField] private Button exertButton;
            [SerializeField] private Button idleButton;
            [SerializeField] private GameObject body;
            public void Init(UnityAction onAttackClick,UnityAction onIdleClick)
            {
                exertButton.onClick.AddListener(onAttackClick);
                idleButton.onClick.AddListener(onIdleClick);
            }
            public void SetMode(Modes mode)
            {
                body.gameObject.SetActive(mode == Modes.Exert);
                tabPanel.gameObject.SetActive(mode != Modes.Exert);
            }
        }

        private IDodgeForm SelectedDodge { get; set; }
        private ICombatForm SelectedCombat { get; set; }
        private IForceForm SelectedForce { get; set; }
        private ICombatUnit CombatUnit { get; set; }
        public void Init(Action<ICombatForm,IDodgeForm> onAttackAction,
            Action<IForceForm> onExertAction,
            Action onIdleAction)
        {
            skillFormView.Init();
            dodgeFormView.Init();
            _combatTab.Init(() =>
            {
                onAttackAction.Invoke(SelectedCombat, SelectedDodge);
                ResetUi();
            });
            _exertTab.Init(() =>
                {
                    onExertAction.Invoke(SelectedForce);
                    ResetUi();
                },
                () =>
                {
                    onIdleAction?.Invoke();
                    ResetUi();
                });
            CombatTabBtn.onClick.AddListener(() => SetCombat(CombatUnit));
            ExertTabBtn.onClick.AddListener(() => SetExert(CombatUnit));
        }

        private void SetMode(Modes mode)
        {
            dodgeFormView.ResetUi();
            skillFormView.ResetUi();
            _combatTab.SetMode(mode);
            _exertTab.SetMode(mode);
        }

        public void Set(ICombatUnit combatUnit)
        {
            CombatUnit = combatUnit;
            SetCombat(combatUnit);
            Show();
        }

        private void SetCombat(ICombatUnit unit)
        {
            SetMode(Modes.Attack);
            var combatSkill = unit.CombatSkill;
            var dodgeSkill = unit.DodgeSkill;
            if(!unit.IsTargetRange())
            {
                for (var i = 0; i < dodgeSkill.Forms.Count; i++)
                {
                    var form = dodgeSkill.Forms[i];
                    dodgeFormView.AddOption(ui =>
                    {
                        ui.Set(() =>
                        {
                            dodgeFormView.SetSelected(ui);
                            SelectedDodge = form;
                        }, form.Name, form.Breath.ToString(), form.Mp.ToString(), form.Qi.ToString());
                    });
                }

                dodgeFormView.Show();
            }

            for (var i = 0; i < combatSkill.Combats.Count; i++)
            {
                var form = combatSkill.Combats[i];
                var isReady = unit.IsCombatFormAvailable(form);
                skillFormView.AddOption(ui =>
                {
                    ui.Interaction(isReady);
                    if (isReady)
                        ui.Set(() =>
                            {
                                skillFormView.SetSelected(ui);
                                SelectedCombat = form;
                            }, form.Name, form.Breath.ToString(), form.Mp.ToString(), form.Qi.ToString(),
                            form.TarBusy.ToString(),
                            form.OffBusy.ToString());
                });
                skillFormView.Show();
            }
        }
        private void SetExert(ICombatUnit unit)
        {
            SetMode(Modes.Exert);
            for (var i = 0; i < unit.ForceSkill.Forms.Count; i++)
            {
                var form = unit.ForceSkill.Forms[i];
                skillFormView.AddOption(ui =>
                {
                    ui.Set(() =>
                    {
                        skillFormView.SetSelected(ui);
                        SelectedForce = form;
                    }, form.Name, form.Breath.ToString());
                });
                skillFormView.Show();
            }
        }

        public override void ResetUi()
        {
            SelectedForce = null;
            SelectedCombat = null;
            SelectedDodge = null;
            skillFormView.ResetUi();
            dodgeFormView.ResetUi();
        }
    }
}