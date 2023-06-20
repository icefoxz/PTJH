using System.Collections.Generic;
using System.Linq;
using Server.Configs.SoUtls;
using UnityEngine;
using UnityEngine.Serialization;
using Utls;

namespace Server.Configs.BattleSimulation
{
    public interface ISimulationOutcome
    {
        int Rounds { get; }//回合数
        bool IsPlayerWin { get; }//是否玩家赢
        int PlayerOffend { get; }//玩家攻击
        int PlayerDefend { get; }//玩家血量
        int EnemyOffend { get; }//敌人攻击
        int EnemyDefend { get; }//敌人血量
        int PlayerRemaining { get; }//玩家战后血量
        int EnemyRemaining { get; }//敌人战后血量
        string[] CombatMessages { get; }//战斗文本
    }

    [CreateAssetMenu(fileName = "模拟战斗配置", menuName = "状态玩法/简易战斗/模拟战斗配置")]
    internal class BattleSimulatorConfigSo : ScriptableObject
    {
        [FormerlySerializedAs("_maps")][SerializeField] private ValueMapping<int>[] 体力扣除配置;
        [SerializeField] private int 回合限制 = 20;
        [SerializeField] private BattleSimulatorMessageSo 文本配置;
        private BattleSimulatorMessageSo BattleMessageSo => 文本配置;
        private ValueMapping<int>[] Maps => 体力扣除配置;
        private int RoundLimit => 回合限制;

        /// <summary>
        /// 模拟回合制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public ISimulationOutcome CountSimulationOutcome(ISimCombat player, ISimCombat enemy, Config.BattleConfig config)
        {
            var playerCombat = new DiziCombatUnit(player, 0);
            var enemyCombat = new DiziCombatUnit(enemy, 1);
            var battle = DiziBattle.AutoCount(config, playerCombat, enemyCombat, RoundLimit);
            var roundCount = battle.Rounds.Count;
            var combatMessages = BattleMessageSo.GetSimulationMessages(roundCount,battle.IsPlayerWin,player,enemy,playerCombat.Hp.Value);
            return new Outcome(roundCount, battle.IsPlayerWin, player.Damage, enemy.Damage, player.MaxHp, enemy.MaxHp,
                playerCombat.Hp.Value, enemyCombat.Hp.Value, combatMessages);
        }

        public int GetPower(float strength, float agility, float hp, float mp) => (int)((strength + agility) * (hp + mp) / 1000);

        public ISimCombat GetSimulation(int teamId, string simName, float strength, float agility, float hp, float mp)
            => new SimCombat(teamId, name: simName, power: GetPower(strength, agility, hp, mp), 
                strength: strength, agility: agility, hp: hp, mp: mp);

        private record SimCombat : ISimCombat
        {
            public SimCombat(int teamId,string name, int power, float strength, float agility, float hp, float mp)
            {
                Name = name;
                Power = power;
                Strength = strength;
                Agility = agility;
                TeamId = teamId;
                Hp = hp;
                Mp = mp;
            }

            public string Name { get; }
            public int Power { get; }
            public int Damage => (int)(Strength + Agility);
            public int MaxHp => (int)(Hp + Mp);
            public float Strength { get; }
            public float Agility { get; }
            public float Hp { get; }
            public int Speed => (int)Agility;
            public int TeamId { get; }
            public float Mp { get; }
        }

        public record Outcome : ISimulationOutcome
        {
            public int Rounds { get; }
            public bool IsPlayerWin { get; }
            public int PlayerOffend { get; }
            public int PlayerDefend { get; }
            public int EnemyOffend { get; }
            public int EnemyDefend { get; }
            public int PlayerRemaining { get; }
            public int EnemyRemaining { get; }
            public string[] CombatMessages { get; }

            public Outcome(int rounds, bool isPlayerWin, int playerOffend, int enemyOffend,
                int playerDefend, int enemyDefend, int playerRemaining, int enemyRemaining, string[] combatMessages)
            {
                Rounds = rounds;
                IsPlayerWin = isPlayerWin;
                PlayerOffend = playerOffend;
                EnemyOffend = enemyOffend;
                PlayerDefend = playerDefend;
                EnemyDefend = enemyDefend;
                PlayerRemaining = playerRemaining;
                CombatMessages = combatMessages;
                EnemyRemaining = enemyRemaining;
            }
        }
    }
}