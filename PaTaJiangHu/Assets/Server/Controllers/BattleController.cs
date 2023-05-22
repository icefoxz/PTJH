using System;
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

        public void StartBattle(string guid, DiziBattle battle, Action<DiziBattle> battleResultAction)
        {
            Game.CacheBattle(battle);
            GameLand.InitBattle(guid, battle);
            var diziFighter = battle.Fighters.First(f => f.Guid == guid);
            Game.MessagingManager.SendParams(EventString.Battle_Init, guid, diziFighter.InstanceId, battle.RoundLimit);
            var co = Game.CoService.RunCo(RunBattle(battle, battleResultAction), null, nameof(GameStageController));
            co.name = $"战斗:[{string.Join(',', battle.Fighters.Select(f => f.Name))}]";
        }

        IEnumerator RunBattle(DiziBattle bat,Action<DiziBattle> battleResultAction)
        {
            while (!bat.IsFinalized)
            {
                var roundInfo = bat.ExecuteRound();
                Game.MessagingManager.SendParams(EventString.Battle_RoundUpdate, bat.Rounds.Count,
                    bat.RoundLimit);
                yield return GameLand.PlayRound(roundInfo);
            }
            battleResultAction?.Invoke(bat);
            Game.MessagingManager.Send(EventString.Battle_End, bat.IsPlayerWin);
        }
        /// <summary>
        /// 清扫战场, 所有战斗后必须调用, 否则会战斗演示一直存在
        /// </summary>
        public void FinalizeBattle() => GameLand.FinalizeBattle();
    }
}