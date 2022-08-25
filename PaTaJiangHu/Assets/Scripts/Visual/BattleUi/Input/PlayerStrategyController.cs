using System;
using BattleM;
using UnityEngine;
using UnityEngine.Events;
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
        [SerializeField] private ManualAutoSwitch _strategySwitch;
        [SerializeField] private ManualFormInputUi _manualInput;
        [SerializeField] private StrategyBarController _strategyBar;
        private ICombatUnit CombatUnit { get; set; }
        public void Init((CombatUnit.Strategies strategy,UnityAction action)[]strategies, 
            Action<ICombatForm,IDodgeForm> onAttackAction,
            Action<IForceForm> onExertAction,
            Action onIdleAction,
            Action<bool> onManualAutoSwitchAction)
        {
            _strategySwitch.Init(isManual=>
            {
                OnStrategySwitch(isManual);
                onManualAutoSwitchAction.Invoke(isManual);
            });
            _manualInput.Init(onAttackAction.Invoke, onExertAction.Invoke, onIdleAction.Invoke);
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
                _manualInput.Set(CombatUnit);
                _strategyBar.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        public override void ResetUi()
        {
            throw new NotImplementedException();
        }

        public void SetPlayer(ICombatUnit player)
        {
            CombatUnit = player;
            _manualInput.Set(CombatUnit);
        }
    }
}