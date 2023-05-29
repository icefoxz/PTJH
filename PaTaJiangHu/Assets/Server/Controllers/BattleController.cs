using System;
using System.Collections;
using System.Linq;
using _GameClient.Models;
using Models;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Random = UnityEngine.Random;

namespace Server.Controllers
{
    /// <summary>
    /// 战斗控制器 
    /// </summary>
    public class BattleController : IGameController
    {
        private IGame2DLand GameLand => Game.Game2DLand;
        private Faction Faction => Game.World.Faction;
        private BattleConfigSo BattleCfg => Game.Config.DiziCfg.BattleCfg;
        private DiziController DiziController => Game.Controllers.Get<DiziController>();

        public void StartBattle(string guid, DiziBattle battle, Action<DiziBattle> battleResultAction)
        {
            Game.BattleCache.SetBattle(battle);
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
            DiziBattleFinalize(bat);
            battleResultAction?.Invoke(bat);
            Game.MessagingManager.Send(EventString.Battle_End, bat.IsPlayerWin);
        }

        private void DiziBattleFinalize(DiziBattle bat)
        {
            foreach (var combatUnit in bat.Fighters)
            {
                var dizi = Faction.GetDizi(combatUnit.Guid);
                if (dizi == null) continue;
                SetDiziConditionAfterBattle(dizi, combatUnit.HpRatio);
            }
        }
        //设定弟子战斗后的状态
        private void SetDiziConditionAfterBattle(Dizi dizi, float hpRatio)
        {
            var conditionValue = BattleCfg.GetConditionValue(hpRatio);
            var cons = new int[4];
            for (var i = 0; i < cons.Length; i++)
            {
                var ran = Random.Range(0, conditionValue);
                cons[i] = ran;
                conditionValue -= ran;
                if (cons.Length - 1 == i && conditionValue > 0)
                    cons[i] += conditionValue;
                if (conditionValue <= 0) break;
            }

            cons = cons.OrderBy(_ => Random.Range(0, 1f)).ToArray();
            DiziController.AddDiziCon(dizi.Guid, IAdjustment.Types.Food, -cons[0]);
            DiziController.AddDiziCon(dizi.Guid, IAdjustment.Types.Emotion, -cons[1]);
            DiziController.AddDiziCon(dizi.Guid, IAdjustment.Types.Inner, -cons[2]);
            DiziController.AddDiziCon(dizi.Guid, IAdjustment.Types.Injury, -cons[3]);
        }

        /// <summary>
        /// 清扫战场, 所有战斗后必须调用, 否则会战斗演示一直存在
        /// </summary>
        public void FinalizeBattle() => GameLand.FinalizeBattle();
    }
}