using System;
using BattleM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Visual.BaseUi;

namespace Visual.BattleUi.Input
{
    public class PlayerStrategyController : UiBase
    {
        private enum Modes
        {
            Auto,
            Manual
        }
        [SerializeField] private ManualFormInputUi _manualInput;
        [SerializeField] private StrategyBarController _strategyBar;
        private ICombatUnit CombatUnit { get; set; }

        public void Init((CombatUnit.Strategies strategy,UnityAction action)[]strategies, 
            UnityAction<ICombatForm> onAttackAction,
            UnityAction<IForceForm> onExertAction,
            UnityAction onRecHpAction,
            UnityAction onRecTpAction)
        {
            _manualInput.Init(onAttackAction.Invoke, onExertAction.Invoke, onRecHpAction.Invoke,
                onRecTpAction.Invoke);
            _strategyBar.Init();
            foreach (var (strategy, action) in strategies)
            {
                _strategyBar.AddOption(GetText(strategy), ui =>
                {
                    _strategyBar.SetSelected(ui);
                    action?.Invoke();
                });
            }
        }

        private string GetText(CombatUnit.Strategies strategy)
        {
            return strategy switch
            {
                BattleM.CombatUnit.Strategies.Steady => "稳扎\n稳打",
                BattleM.CombatUnit.Strategies.Hazard => "以伤\n换伤",
                BattleM.CombatUnit.Strategies.Defend => "死守\n保全",
                BattleM.CombatUnit.Strategies.RunAway => "逃跑\n求生",
                BattleM.CombatUnit.Strategies.DeathFight => "死战\n到底",
                _ => throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null)
            };
        }

        private void OnStrategySwitch(bool isManual) => SetMode(isManual ? Modes.Manual : Modes.Auto);

        private void SetMode(Modes mode)
        {
            switch (mode)
            {
                case Modes.Auto:
                    _manualInput.ResetUi();
                    _manualInput.Hide();
                    _strategyBar.Show();
                    break;
                case Modes.Manual:
                    UpdateFormUis();
                    _strategyBar.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
        private event UnityAction OnPointerUp;
        private event UnityAction<IForceForm> OnForcePointerDown;
        private event UnityAction<ICombatForm> OnCombatPointerDown;
        //private event UnityAction OnIdlePointerDown;
        public void SetPlayer(ICombatUnit player, 
            UnityAction<ICombatForm> onCombatPointerDown,
            UnityAction<IForceForm> onForcePointerDown,
            UnityAction onPointerCancel)
        {
            OnCombatPointerDown = combat =>
            {
                IsListeningCancel = true;
                onCombatPointerDown.Invoke(combat);
            };
            OnForcePointerDown = force =>
            {
                IsListeningCancel = true;
                onForcePointerDown.Invoke(force);
            };
            //OnIdlePointerDown = () =>
            //{
            //    IsListeningCancel = true;
            //    onIdle.Invoke();
            //};
            OnPointerUp = () =>
            {
                IsListeningCancel = false;
                onPointerCancel.Invoke();
            };
            CombatUnit = player;
            UpdateFormUis();
        }

        private bool IsListeningCancel { get; set; }

        private void UpdateFormUis()
        {
            _manualInput.SetCombat(CombatUnit, OnCombatPointerDown);
            _manualInput.SetForce(CombatUnit, OnForcePointerDown);
            //_manualInput.SetIdle(OnIdlePointerDown);
        }
        public override void ResetUi()
        {
            throw new NotImplementedException();
        }

        void Update()
        {
            if (UnityEngine.Input.GetMouseButtonUp(0) && IsListeningCancel)
            {
                OnPointerUp?.Invoke();
            }
        }
    }
}