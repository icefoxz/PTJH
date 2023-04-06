using System;
using System.Collections;
using System.Linq;
using _GameClient.Models;
using Server.Configs.ChallengeStages;

namespace Server.Controllers
{
    public class ChallengeStageController : IGameController
    {
        private Config.ChallengeStageCfg ChallengeCfg => Game.Config.ChallengeCfg;
        private int ChallengeProgressIndex { get; set; }
        private Faction Faction => Game.World.Faction;
        public IChallengeStageNpc[] GetChallenges() => ChallengeCfg.Stages[ChallengeProgressIndex].Npcs;
        private IGame2DLand GameLand { get; } = Game.Game2DLand;

        public void StartChallenge(string guid, int npcIndex)
        {
            var dizi = Faction.GetDizi(guid);
            var battle = ChallengeCfg.Stages[ChallengeProgressIndex].Instance(npcIndex, dizi);
            GameLand.InitBattle(battle);
            var co = Game.CoService.RunCo(RunBattle(battle),null,nameof(ChallengeStageController));
            co.name = $"挑战战斗:[{string.Join(',', battle.Fighters.Select(f => f.Name))}]";
        }

        private IEnumerator RunBattle(DiziBattle battle)
        {
            while (!battle.IsFinalized)
                yield return GameLand.PlayRound(battle.ExecuteRound());
            FinalizeChallenge(battle);
        }

        private void FinalizeChallenge(DiziBattle battle)
        {
            if (!battle.IsPlayerWin) return;
            ChallengeProgressIndex++;
            Game.MessagingManager.SendParams(EventString.Faction_Challenge_Update);
        }

    }
}