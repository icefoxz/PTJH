using System;
using System.Collections.Generic;
using System.Linq;
using GameClient.SoScripts.Battles;
using GameClient.SoScripts.BattleSimulation;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "id_模拟战斗事件名", menuName = "状态玩法/事件/模拟战斗事件")]
    internal class BattleSimulationEventSo : AdvEventSoBase
    {
        [SerializeField] private string 事件名 = "战斗";
        [SerializeField] private string 战斗前文本;
        [SerializeField] private BattleResult 战果;
        [SerializeField] private CombatNpcSo _npc;

        private CombatNpcSo Npc => _npc;
        private BattleResult Battle => 战果;
        private string IntroLog => 战斗前文本;

        public override string Name => 事件名;

        protected override IAdvEvent OnEventInvoke(IAdvEventArg arg)
        {
            var simOutcome = arg.SimOutcome;
            var logs = GetResult(simOutcome, arg.DiziName, _npc.Name);
            ProcessLogs(logs);
            var nextEvent = AllEvents[simOutcome.IsPlayerWin ? 0 : 1];
            return nextEvent;
        }

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