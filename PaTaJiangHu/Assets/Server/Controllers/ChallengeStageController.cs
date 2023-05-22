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

        public void ChallengeStart(string guid, int npcIndex)
        {
            var dizi = Faction.GetDizi(guid);
            var challenge = Faction.Challenge;
            var stage = challenge.Stage;
            var battle = ChallengeStageCfg.InstanceBattle(dizi, stage.Id, challenge.Progress, npcIndex);
            BattleController.StartBattle(guid, battle, BattleEndUpdateChallenge);

            void BattleEndUpdateChallenge(DiziBattle bat)
            {
                if (!bat.IsPlayerWin) return;
                Faction.NextChallengeProgress();
                Game.MessagingManager.SendParams(EventString.Faction_Challenge_BattleEnd, bat.IsPlayerWin);
                Game.MessagingManager.SendParams(EventString.Faction_Challenge_Update);
            }
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
    }
}