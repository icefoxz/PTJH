using _GameClient.Models;
using Server.Configs.ChallengeStages;
using UnityEngine;
using Utls;

namespace Server.Controllers
{
    public class GameStageController : IGameController
    {
        private Config.GameStageCfg GameCfg => Game.Config.StageCfg;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private int ChallengeProgressIndex { get; set; }
        private Faction Faction => Game.World.Faction;
        private BattleController BattleController => Game.Controllers.Get<BattleController>();
        private DiziController DiziController => Game.Controllers.Get<DiziController>();
        private StaminaController StaminaController => Game.Controllers.Get<StaminaController>();

        public ISingleCombatNpc[] GetChallenges() => GameCfg.Stages[ChallengeProgressIndex].Npcs;

        public void ChallengeStart(string guid, int npcIndex)
        {
            var stage = GameCfg.Stages[ChallengeProgressIndex];
            var dizi = Faction.GetDizi(guid);
            if (!StaminaController.TryConsumeForBattle(dizi.Guid))
            {
                XDebug.Log($"{dizi}体力不足以战斗!");
                return;
            }

            var npc = stage.GetNpc(npcIndex);
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = npc.GetDiziCombat();
            var battle = DiziBattle.Instance(BattleCfg, new[] { diziCombat, npcCombat }, stage.RoundLimit);
            Game.BattleCache.SetAvatars(new (CombatUnit, Sprite)[] { (npcCombat, npc.Icon) });
            Game.BattleCache.SetBattle(battle);
            BattleController.StartBattle(guid, battle, OnBattleEnd);

            void OnBattleEnd(DiziBattle bat)
            {
                if(bat.IsPlayerWin) ChallengeProgressIndex++;
                DiziController.DiziExpAdd(dizi.Guid, npc.DiziReward.Exp);
                Game.MessagingManager.SendParams(EventString.Faction_Challenge_BattleEnd, bat.IsPlayerWin);
            }
        }
    }
}