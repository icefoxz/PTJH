using System;
using System.Collections.Generic;
using System.Linq;
using Server.Configs.SoUtls;
using UnityEngine;
using Utls;

namespace Server.Configs.BattleSimulation
{
    public interface ISimulationOutcome
    {
        ISimulationRound[] Rounds { get; }
        int RemainingHp { get; }
        float RemainingRatio { get; }
        bool IsPlayerWin { get; }
        float PlayerOffend { get; }
        float EnemyOffend { get; }
        float PlayerDefend { get; }
        float EnemyDefend { get; }
        /// <summary>
        /// 配置结果, -1为战斗失败, 正数为扣除的体力值
        /// </summary>
        int Result { get; }
    }
    public interface ISimulationRound
    {
        float PlayerDefend { get; }
        float EnemyDefend { get; }
    }

    [CreateAssetMenu(fileName = "模拟战斗配置", menuName = "配置/简易战斗/模拟战斗配置")]
    internal class BattleSimulatorConfigSo : ScriptableObject
    {
        [SerializeField] private ValueMapping<int>[] _maps;
        private ValueMapping<int>[] Maps => _maps;
        private const int LimitedRound = 99;

        /// <summary>
        /// 模拟回合制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public ISimulationOutcome CountSimulationOutcome(ISimCombat player, ISimCombat enemy)
        {
            var playerHp = player.Defend;
            var enemyHp = enemy.Defend;
            var playerAtt = player.Offend <= 0 ? 1 : player.Offend;//攻击最小数为1
            var enemyAtt = enemy.Offend <= 0 ? 1 : enemy.Offend;
            var recursiveRound = 0;
            var balance = 0;
            var list = new List<ISimulationRound>();
            while (true)
            {
                playerHp -= enemyAtt;
                enemyHp -= playerAtt;
                list.Add(new Round(playerHp, enemyHp));
                if (enemyHp <= 0)
                {
                    //balance = (int)(1f * playerHp / player.Defend);
                    var remainingHp = (int)playerHp;
                    return new Outcome(list.ToArray(), remainingHp, playerHp / player.Defend, true, player.Offend,
                        enemy.Offend, player.Defend, enemy.Defend, GetValueFromRemaining(remainingHp));
                }

                if (playerHp <= 0)
                    return new Outcome(list.ToArray(), (int)enemyHp, enemyHp / enemy.Defend, false, player.Offend,
                        enemy.Offend, player.Defend, enemy.Defend);
#if UNITY_EDITOR
                if (recursiveRound >= LimitedRound)
                    throw new StackOverflowException($"战斗回合数超过 = {LimitedRound}");
#else
                if (recursiveRound >= 10)
                {
                    XDebug.LogWarning(
                        $"战斗回合超过 10 次!playerHp = {playerHp}, enemyHp = {enemyHp} \nPlayer: {player}\nenemy{enemy}");
                    var isPlayerWin = playerHp >= enemyHp;
                    var remainingHp = isPlayerWin ? (int)playerHp : (int)enemyHp;
                    var remainingRatio = isPlayerWin ? playerHp / player.Defend : enemyHp / enemy.Defend;
                    return new Outcome(list.ToArray(), remainingHp, remainingRatio, isPlayerWin, player.Offend,
                        enemy.Offend, player.Defend, enemy.Defend);
                }
#endif
                recursiveRound++;
            }
            throw new NotImplementedException();
        }
        private int GetValueFromRemaining(int hp)
        {
            if (hp <= 0)
            {
                XDebug.LogWarning($"弟子Hp异常 = {hp}!");
                hp = 0;
            }
            var m = Maps.Where(m => m.IsInCondition(hp))
                .RandomPick();
            return m.Value;
        }

        public record Round : ISimulationRound
        {
            public float PlayerDefend { get; }
            public float EnemyDefend { get; }

            public Round(float playerDefend, float enemyDefend)
            {
                PlayerDefend = playerDefend;
                EnemyDefend = enemyDefend;
            }
        }

        public record Outcome : ISimulationOutcome
        {
            public ISimulationRound[] Rounds { get; }
            public int RemainingHp { get; }
            public float RemainingRatio { get; }
            public bool IsPlayerWin { get; }
            public float PlayerOffend { get; }
            public float EnemyOffend { get; }
            public float PlayerDefend { get; }
            public float EnemyDefend { get; }
            public int Result { get; }

            public Outcome(ISimulationRound[] rounds, int remainingHp, float remainingRatio, bool isPlayerWin, float playerOffend, float enemyOffend, float playerDefend, float enemyDefend, int result = -1)
            {
                Rounds = rounds;
                RemainingHp = remainingHp;
                IsPlayerWin = isPlayerWin;
                PlayerOffend = playerOffend;
                EnemyOffend = enemyOffend;
                PlayerDefend = playerDefend;
                EnemyDefend = enemyDefend;
                RemainingRatio = remainingRatio;
                Result = result;
            }
        }
    }
}