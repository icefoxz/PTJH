using System.Linq;
using AOT.Core;
using GameClient.Args;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;

namespace GameClient.Controllers
{
    /// <summary>
    /// 挑战控制器
    /// </summary>
    public class ChallengeStageController : IGameController
    {
        private ChallengeCfgSo ChallengeStageCfg => Game.Config.ChallengeCfg;
        private Faction Faction => Game.World.Faction;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private BattleController BattleController => Game.Controllers.Get<BattleController>();
        private DiziController DiziController => Game.Controllers.Get<DiziController>();
        private RewardController RewardController => Game.Controllers.Get<RewardController>();

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
            var stage = Faction.GetChallengeStage();
            var npc = ChallengeStageCfg.InstanceBattle(stage.Id, Faction.ChallengeStageProgress, npcIndex);
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = npc.GetDiziCombat();
            Game.BattleCache.SetAvatars(new (CombatUnit, Sprite)[] { (npcCombat, npc.Icon) });
            var battle = DiziBattle.Instance(BattleCfg, new[] { diziCombat, npcCombat });
            BattleController.StartBattle(guid, battle, BattleEndUpdateChallenge);

            void BattleEndUpdateChallenge(DiziBattle bat)
            {
                if (!bat.IsPlayerWin) return;
                DiziController.DiziExpAdd(dizi.Guid, npc.DiziReward.Exp);
                if (npc.Chest != null) Faction.AddChest(npc.Chest);
                Faction.NextChallengeProgress();
                if (Faction.Challenge.IsFinish) ChallengeLevelUp();
                Game.MessagingManager.SendParams(EventString.Faction_Challenge_BattleEnd, bat.IsPlayerWin);
            }
        }

        public void ChallengeAbandon()
        {
            ChallengeLevelDown();
            Faction.RemoveChallenge();
        }

        public void GetReward()
        {
            var chest = Faction.ChallengeChests.First();
            Faction.RemoveChest(chest);
            RewardController.SetReward(chest, true);
        }

        private void ChallengeLevelDown()
        {
            var nextAbandonCount = Faction.Challenge.AbandonCount + 1;
            if (nextAbandonCount < ChallengeStageCfg.DowngradeAbandonStreak)
            {
                Faction.SetAbandonCount(nextAbandonCount);
                Faction.SetPassCount(0);
            }
            else Faction.LevelDown();
        }
        private void ChallengeLevelUp()
        {
            var passCount = Faction.Challenge.PassCount + 1;
            if (passCount < ChallengeStageCfg.UpgradePassStreak)
            {
                Faction.SetPassCount(passCount);
                Faction.SetAbandonCount(0);
            }
            else Faction.LevelUp();
        }
    }
}