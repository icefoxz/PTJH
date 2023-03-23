using System;
using System.Collections.Generic;
using System.Linq;
using NameM;
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
        /// <summary>
        /// 配置结果, -1为战斗失败, 正数为扣除的体力值
        /// </summary>
        int PlayerRemaining { get; }//玩家战后血量
        int EnemyRemaining { get; }//敌人战后血量
        string[] CombatMessages { get; }//战斗文本
    }

    [CreateAssetMenu(fileName = "模拟战斗配置", menuName = "配置/简易战斗/模拟战斗配置")]
    internal class BattleSimulatorConfigSo : ScriptableObject
    {
        [FormerlySerializedAs("_maps")][SerializeField] private ValueMapping<int>[] 体力扣除配置;
        [SerializeField] private int 回合限制 = 20;
        [SerializeField] private BattleSimulatorMessageSo 文本配置;
        private BattleSimulatorMessageSo BattleMessageSo => 文本配置;
        private ValueMapping<int>[] Maps => 体力扣除配置;
        private int RoundLimit => 回合限制;

        private const int LimitedRound = 99;

        /// <summary>
        /// 模拟回合制
        /// </summary>
        /// <param name="player"></param>
        /// <param name="enemy"></param>
        /// <returns></returns>
        public ISimulationOutcome CountSimulationOutcome(ISimCombat player, ISimCombat enemy)
        {
            var combatUnits = new List<CombatUnit>();
            var buffMgr = new BuffManager(combatUnits);
            var playerCombat = new CombatUnit(0, player.Name, player.Defend, player.Offend, 2, buffMgr);
            var enemyCombat = new CombatUnit(1, enemy.Name, enemy.Defend, enemy.Offend, 1, buffMgr);
            combatUnits.Add(playerCombat);
            combatUnits.Add(enemyCombat);
            var rounds = new List<RoundInfo>();
            while (playerCombat.IsAlive && enemyCombat.IsAlive)
            {
                var r = new Round(combatUnits, buffMgr);
                rounds.Add(r.Execute());
            }
            var isPlayerWin = playerCombat.IsAlive;
            var roundCount = rounds.Count;
            var combatMessages = BattleMessageSo.GetSimulationMessages(roundCount,isPlayerWin,player,enemy,playerCombat.Hp);
            return new Outcome(roundCount, isPlayerWin, player.Offend, enemy.Offend, player.Defend, enemy.Defend,
                playerCombat.Hp, enemyCombat.Hp, combatMessages);
        }
        //public ISimulationOutcome CountSimulationOutcome(ISimCombat player, ISimCombat enemy)
        //{
        //    var playerHp = player.Defend;
        //    var enemyHp = enemy.Defend;
        //    var playerAtt = player.Offend <= 0 ? 1 : player.Offend;//攻击最小数为1
        //    var enemyAtt = enemy.Offend <= 0 ? 1 : enemy.Offend;
        //    var isPlayerWin = false;
        //    var round = 1;
        //    for (var i = 0; i < RoundLimit; i++)
        //    {
        //        enemyHp -= playerAtt; //玩家先攻击
        //        if (enemyHp <= 0)
        //        {
        //            isPlayerWin = true;
        //            break;
        //        }

        //        playerHp -= enemyAtt; //敌人后攻击
        //        if (playerHp > 0)
        //        {
        //            round++;
        //            continue;
        //        }

        //        break;
        //    }

        //    var playerHpRatio = playerHp == 0 ? 0 : 1f * playerHp / player.Defend;
        //    var messages = BattleMessageSo.GetSimulationMessages(round, isPlayerWin, player, enemy, playerHp);
        //    return new Outcome(round, playerHpRatio, isPlayerWin, player.Offend,
        //        enemy.Offend, player.Defend, enemy.Defend, playerHp, enemyHp, messages);
        //}
        public int GetDeductionValueFromRemaining(int hp)
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

        public int GetPower(int strength, int agility, int hp, int mp, int weaponDamage = 0,
            int armorAddHp = 0) => (strength + agility) * (hp + mp) / 1000;

        public ISimCombat GetSimulation(string simName, int strength, int agility, int hp, int mp, int weaponDamage,
            int armorAddHp) => new SimCombat(name: simName,
            power: GetPower(strength, agility, hp, mp, weaponDamage, armorAddHp),
            strength: strength, agility: agility, hp: hp, mp: mp, weaponDamage, armorAddHp);

        private record SimCombat : ISimCombat
        {
            public SimCombat(string name,int power, int strength, int agility, int hp, int mp, int weapon, int armor)
            {
                Name = name;
                Power = power;
                Strength = strength;
                Agility = agility;
                Weapon = weapon;
                Armor = armor;
                Hp = hp;
                Mp = mp;
            }

            public string Name { get; }
            public int Power { get; }
            public int Offend => Strength + Agility + Weapon;
            public int Defend => Hp + Mp + Armor;
            public int Strength { get; }
            public int Agility { get; }
            public int Hp { get; }
            public int Mp { get; }
            public int Weapon { get; }
            public int Armor { get; }

            public void Deconstruct(out string Name, out int Power, out float Strength, out float Agility, out float Weapon, out float Armor)
            {
                Name = this.Name;
                Power = this.Power;
                Strength = this.Strength;
                Agility = this.Agility;
                Weapon = this.Weapon;
                Armor = this.Armor;
            }
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