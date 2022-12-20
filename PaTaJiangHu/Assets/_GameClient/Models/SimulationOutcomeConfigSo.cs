using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using UnityEngine;
using Utls;

namespace _Game.Models
{
    public interface ISimulationOutcome
    {
        ISimulationRound[] Rounds { get; }
        int RemainingHp { get; }
        bool IsPlayerWin { get; }
        float PlayerOffend { get; }
        float EnemyOffend { get; }
        float PlayerDefend { get; }
        float EnemyDefend { get; }
    }
    public interface ISimulationRound
    {
        float PlayerDefend { get; }
        float EnemyDefend { get; }
    }

    [CreateAssetMenu(fileName = "体力扣除配置", menuName = "配置/简易战斗/体力扣除配置")]
    internal class SimulationOutcomeConfigSo : ScriptableObject
    {
        [SerializeField] private Map[] _maps;
        private Map[] Maps => _maps;
        private const int StackOverFlow = 9999;

        public int GetValue(int balance)
        {
            var m = Maps.Where(m => m.IsInCondition(balance))
                .RandomPick();
            return m.Value;
        }
        /// <summary>
        /// 模拟回合制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public ISimulationOutcome CountSimulationOutcome(ISimulation player, ISimulation enemy)
        {
            var playerHp = player.Defend;
            var playerAtt = player.Offend;
            var enemyHp = enemy.Defend;
            var enemyAtt = enemy.Offend;
            var recursive = 0;
            var balance = 0;
            var list = new List<ISimulationRound>();
            while (true)
            {
                playerHp -= enemyAtt;
                enemyHp -= playerAtt;
                list.Add(new Round(playerAtt, playerHp, enemyAtt, enemyHp));
                if (enemyHp <= 0)
                {
                    //balance = (int)(1f * playerHp / player.Defend);
                    return new Outcome(list.ToArray(), (int)playerHp, true, playerAtt, enemyAtt, player.Defend, enemy.Defend);
                }

                if (playerHp <= 0)
                    return new Outcome(list.ToArray(), (int)enemyHp, false, playerAtt, enemyAtt, player.Defend, enemy.Defend);

                if (recursive >= StackOverFlow)
                    throw new StackOverflowException();
                recursive++;
            }

            throw new NotImplementedException();
        }

        [Serializable] private class Map
        {
            private bool AutoName()
            {
                _name = $"{From}%~{To}%:{Value}";
                return true;
            }

            [ConditionalField(true, nameof(AutoName))][ReadOnly][SerializeField] private string _name;
            [SerializeField] private int 从;
            [SerializeField] private int 至;
            [SerializeField] private int 值;

            private int From => 从;
            private int To => 至;
            public int Value => 值;

            public bool IsInCondition(int value) => value >= From && value <= To;
        }


        public record Round : ISimulationRound
        {
            public float PlayerOffend { get; }
            public float EnemyOffend { get; }
            public float PlayerDefend { get; }
            public float EnemyDefend { get; }

            public Round(float playerOffend, float playerDefend, float enemyOffend, float enemyDefend)
            {
                PlayerOffend = playerOffend;
                EnemyOffend = enemyOffend;
                PlayerDefend = playerDefend;
                EnemyDefend = enemyDefend;
            }
        }

        public record Outcome : ISimulationOutcome
        {
            public ISimulationRound[] Rounds { get; }
            public int RemainingHp { get; }
            public bool IsPlayerWin { get; }
            public float PlayerOffend { get; }
            public float EnemyOffend { get; }
            public float PlayerDefend { get; }
            public float EnemyDefend { get; }

            public Outcome(ISimulationRound[] rounds, int remainingHp, bool isPlayerWin, float playerOffend, float enemyOffend, float playerDefend, float enemyDefend)
            {
                Rounds = rounds;
                RemainingHp = remainingHp;
                IsPlayerWin = isPlayerWin;
                PlayerOffend = playerOffend;
                EnemyOffend = enemyOffend;
                PlayerDefend = playerDefend;
                EnemyDefend = enemyDefend;
            }
        }
    }
}