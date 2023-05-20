using _GameClient.Models;
using Server.Configs.ChallengeStages;

namespace Server.Controllers
{
    public class GameStageController : IGameController
    {
        private Config.GameStageCfg GameCfg => Game.Config.StageCfg;
        private int ChallengeProgressIndex { get; set; }
        private Faction Faction => Game.World.Faction;
        public ISingleStageNpc[] GetChallenges() => GameCfg.Stages[ChallengeProgressIndex].Npcs;
        private BattleController BattleController => Game.Controllers.Get<BattleController>();

        public void StartChallenge(string guid, int npcIndex)
        {
            var dizi = Faction.GetDizi(guid);
            var battle = GameCfg.Stages[ChallengeProgressIndex].InstanceBattle(npcIndex, dizi);
            Game.CacheBattle(battle);
            BattleController.StartBattle(guid, battle);
        }
    }
}