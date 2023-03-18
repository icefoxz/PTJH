using System;
using System.Collections.Generic;
using System.Linq;
using Server.Configs.BattleSimulation;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_模拟战斗事件名", menuName = "事件/模拟战斗事件")]
    internal class BattleSimulationEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "战斗";
        [SerializeField] private string 战斗前文本;
        [SerializeField] private BattleResult 战果;
        [SerializeField] private CombatNpcSo _npc;

        private CombatNpcSo Npc => _npc;
        private BattleResult Battle => 战果;
        private string IntroLog => 战斗前文本;
        public override event Action<string[]> OnLogsTrigger;

        public override string Name => 事件名;

        public override void EventInvoke(IAdvEventArg arg)
        {
            var simOutcome = arg.SimOutcome;
            var logs = GetResult(simOutcome, arg.DiziName, _npc.Name);
            OnLogsTrigger?.Invoke(logs);
            var nextEvent = AllEvents[simOutcome.IsPlayerWin ? 0 : 1];
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;

        public override IAdvEvent[] AllEvents => Battle.GetAllEvents();
        public override AdvTypes AdvType => AdvTypes.Simulation;

        public ISimCombat GetNpc(BattleSimulatorConfigSo so)
        {
            if (Npc == null)
                throw new NullReferenceException($"{Name}.{name}找不到NPC = null, 请确保npc已配置了!");
            return Npc.GetSimCombat(so);
        }

        private string[] GetResult(ISimulationOutcome outCome,string diziName,string npcName)
        {
            var logs = new List<string> { string.Format(IntroLog, diziName, npcName) };
            logs.AddRange(GenerateBattleLog(outCome, diziName, npcName));
            return logs.ToArray();
        }

        private string[] GenerateBattleLog(ISimulationOutcome outcome, string diziName, string npcName)
        {
            var roundLog = outcome.CombatMessages.ToList();
            (string winner, string loser) = outcome.IsPlayerWin ? (diziName, npcName) : (npcName, diziName);
            roundLog.Add($"{winner}打败{loser}!");
            return roundLog.ToArray();
        }

        //private string GenRoundLog(ISimulationRound sim,int index,string diziName,string npcName)
        //{
        //    var round = index + 1;
        //    var isPlayerAdvantage = sim.PlayerDefend > sim.EnemyDefend;
        //    (string adv, string tar) = isPlayerAdvantage ? (diziName, npcName) : (npcName, diziName);
        //    if (round % 2 == 0)
        //    {
        //        return $"【回合{round}】:" + $"{tar}趁{adv}的招数使老, 反击!";
        //    }
        //    return $"【回合{round}】:" + $"{adv}抢得先机猛攻{tar}!";
        //}

        [Serializable]
        private class BattleResult
        {
            [SerializeField] private AdvEventSoBase 胜;
            [SerializeField] private AdvEventSoBase 败;

            public AdvEventSoBase Win => 胜;
            public AdvEventSoBase Lose => 败;

            public IAdvEvent[] GetAllEvents() => new IAdvEvent[] { Win, Lose };
        }
    }
}