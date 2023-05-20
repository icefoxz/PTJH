using System.Collections;
using System.Linq;
using _GameClient.Models;

namespace Server.Controllers
{
    /// <summary>
    /// 战斗控制器 
    /// </summary>
    public class BattleController : IGameController
    {
        private IGame2DLand GameLand => Game.Game2DLand;
        private Faction Faction => Game.World.Faction;

        public void StartBattle(string guid,DiziBattle battle)
        {
            Game.CacheBattle(battle);
            GameLand.InitBattle(guid, battle);
            var diziFighter = battle.Fighters.First(f => f.Guid == guid);
            Game.MessagingManager.SendParams(EventString.Battle_Init, guid, diziFighter.InstanceId, battle.RoundLimit);
            var co = Game.CoService.RunCo(RunBattle(battle), null, nameof(GameStageController));
            co.name = $"挑战战斗:[{string.Join(',', battle.Fighters.Select(f => f.Name))}]";

        }

        IEnumerator RunBattle(DiziBattle bat)
        {
            while (!bat.IsFinalized)
            {
                var roundInfo = bat.ExecuteRound();
                Game.MessagingManager.SendParams(EventString.Battle_RoundUpdate, bat.Rounds.Count,
                    bat.RoundLimit);
                yield return GameLand.PlayRound(roundInfo);
            }

            Game.MessagingManager.Send(EventString.Battle_End, bat.IsPlayerWin);
            UpdateChallenge(bat);
        }

        void UpdateChallenge(DiziBattle bat)
        {
            if (!bat.IsPlayerWin) return;
            Faction.NextChallengeProgress();
        }

        public void FinalizeBattle() => GameLand.FinalizeBattle();

    }
}