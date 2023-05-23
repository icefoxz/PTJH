using _GameClient.Models;
using Server.Configs.ChallengeStages;
using UnityEngine;

namespace Server.Controllers
{
    public class GameStageController : IGameController
    {
        private Config.GameStageCfg GameCfg => Game.Config.StageCfg;
        private int ChallengeProgressIndex { get; set; }
        private Faction Faction => Game.World.Faction;
        public ISingleCombatNpc[] GetChallenges() => GameCfg.Stages[ChallengeProgressIndex].Npcs;
        private BattleController BattleController => Game.Controllers.Get<BattleController>();
        private DiziController DiziController => Game.Controllers.Get<DiziController>();

        public void ChallengeStart(string guid, int npcIndex)
        {
            var stage = GameCfg.Stages[ChallengeProgressIndex];
            var dizi = Faction.GetDizi(guid);
            var npc = stage.GetNpc(npcIndex);
            var diziCombat = new DiziCombatUnit(0, dizi);
            var npcCombat = npc.GetDiziCombat();
            var battle = DiziBattle.Instance(new[] { diziCombat, npcCombat }, stage.RoundLimit);
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