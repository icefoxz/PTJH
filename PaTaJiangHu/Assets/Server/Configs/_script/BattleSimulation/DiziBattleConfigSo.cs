using System;
using System.Linq;
using Models;
using Server.Configs.SoUtls;
using UnityEngine;

namespace Server.Configs.BattleSimulation
{
    [CreateAssetMenu(fileName = "弟子战斗配置", menuName = "弟子/战斗配置")]
    internal class DiziBattleConfigSo : ScriptableObject
    {
        [SerializeField]private BeforeBattle 战斗前配置;
        [SerializeField]private AfterBattle 战斗后配置;

        public BeforeBattle Before => 战斗前配置;
        public AfterBattle After => 战斗后配置;

        public int GetConditionValue(float hpRatio)
        {
            var staRate= (int)(hpRatio * 100);
            if (staRate < 0) staRate = 0;
            return After.ConditionMaps.FirstOrDefault(c => c.IsInCondition(staRate))?.Value ?? 0;
        }
        //战斗后配置
        [Serializable]internal class AfterBattle
        {
            [SerializeField]private ValueMapping<int>[] 弟子状态扣除配置;
            public ValueMapping<int>[] ConditionMaps => 弟子状态扣除配置;
        }

        //战斗前配置
        [Serializable]internal class BeforeBattle
        {
            [SerializeField]private int 弟子体力扣除 = 5;
            public int StaminaCost => 弟子体力扣除;
        }
    }
}