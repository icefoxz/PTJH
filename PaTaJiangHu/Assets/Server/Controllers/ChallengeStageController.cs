using System.Linq;
using _GameClient.Models;

namespace Server.Controllers
{
    /// <summary>
    /// 挑战控制器
    /// </summary>
    public class ChallengeStageController : IGameController
    {
        private ChallengeCfgSo ChallengeStageCfg => Game.Config.ChallengeCfg;
        private Faction Faction => Game.World.Faction;
        private BattleController BattleController => Game.Controllers.Get<BattleController>();

        public IChallengeStage RequestNewChallenge()
        {
            var stage = ChallengeStageCfg.GetRandomStage(Faction.ChallengeLevel);
            Faction.SetChallenge(stage);
            Game.MessagingManager.SendParams(EventString.Faction_Challenge_Update);
            return stage;
        }

        public void StartChallenge(string guid, int npcIndex)
        {
            var dizi = Faction.GetDizi(guid);
            var challenge = Faction.Challenge;
            var battle = ChallengeStageCfg.InstanceBattle(dizi, challenge.Id, challenge.Progress, npcIndex);
            BattleController.StartBattle(guid, battle);
        }

        public void RequestChallengeGiveup()
        {
            Faction.RemoveChallenge();
            Game.MessagingManager.SendParams(EventString.Faction_Challenge_Update);
        }

        public void GetReward()
        {
            var challenge = Faction.Challenge;
            Game.World.RewardBoard.SetReward(challenge.Chests.First());
        }

        public IChallengeStage GetCurrentChallengeStage()
        {
            if(Faction.Challenge == null) return null;
            var stage = ChallengeStageCfg.GetStage(Faction.Challenge.Id);
            return stage;
        }
    }
}