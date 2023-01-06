using System;
using System.Collections.Generic;
using System.Linq;
using Server.Configs.BattleSimulation;
using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_模拟战斗事件名", menuName = "事件/历练/模拟战斗事件")]
    internal class BattleSimulationEventSo : AdvEventSoBase
    {
        [SerializeField] private AdvEventSoBase[] _allEvents;

        [SerializeField] private string 战斗前文本;

        private string IntroLog => 战斗前文本;
        public override event Action<string[]> OnLogsTrigger;

        public override string Name { get; } = "战斗";

        public override void EventInvoke(IAdvEventArg arg)
        {
            var logs = GetResult(arg.SimOutcome);
            OnLogsTrigger?.Invoke(logs);
            var nextEvent = AllEvents[arg.Result];
            OnNextEvent?.Invoke(nextEvent);
        }

        public override event Action<IAdvEvent> OnNextEvent;

        public override IAdvEvent[] AllEvents => _allEvents;
        public override AdvTypes AdvType => AdvTypes.Simulation;
        

        private string[] GetResult(ISimulationOutcome outCome)
        {
            var logs = new List<string> { IntroLog };
            logs.AddRange(GenerateBattleLog(outCome));
            return logs.ToArray();
        }

        private string[] GenerateBattleLog(ISimulationOutcome outcome)
        {
            var charName = "{0}";
            var npcName = "{1}";
            var roundLog = outcome.Rounds.Select((s, i) => GenRoundLog(s, i, charName, npcName)).ToList();
            (string winner, string loser) = outcome.IsPlayerWin ? (charName, npcName) : (npcName, charName);
            roundLog.Add($"{winner}打败{loser}!");
            return roundLog.ToArray();
        }

        private string GenRoundLog(ISimulationRound sim,int index,string charName,string npcName)
        {
            var round = index + 1;
            var isPlayerAdvantage = sim.PlayerDefend > sim.EnemyDefend;
            (string adv, string tar) = isPlayerAdvantage ? (charName, npcName) : (npcName, charName);
            return $"【回合{round}】:" + $"{adv}抢得先机猛攻{tar}!";
        }

    }
}