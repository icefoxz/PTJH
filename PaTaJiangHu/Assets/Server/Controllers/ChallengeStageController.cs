using System;
using System.Linq;
using _GameClient.Models;
using Server.Configs.ChallengeStages;

namespace Server.Controllers
{
    public class ChallengeStageController : IGameController
    {
        private Config.ChallengeStageCfg ChallengeCfg => Game.Config.ChallengeCfg;
        private int ProgressIndex { get; set; }
        private Faction Faction => Game.World.Faction;
        public IChallengeStageNpc[] GetChallenges() => ChallengeCfg.Stages[ProgressIndex].Npcs;

        public void StartChallenge(string guid, int npcIndex)
        {
            var dizi = Faction.GetDizi(guid);
            var battle = ChallengeCfg.Stages[ProgressIndex].Challenge(npcIndex, dizi);
            var gameLand = Game.Game2DLand;
            gameLand.PlayBattle(battle, () =>
            {
                if (battle.IsPlayerWin)
                {
                    ProgressIndex++;
                    Game.MessagingManager.SendParams(EventString.Faction_Challenge_Update);
                }
            });
        }
    }
}