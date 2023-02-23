using System;
using _GameClient.Models;
using Server.Configs.BattleSimulation;
using Utls;

namespace Server.Controllers
{

    public class DiziIdleController : IGameController
    {
        private Config.DiziIdle IdleCfg => Game.Config.Idle;
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private ConditionPropertySo ConditionProperty => Game.Config.AdvCfg.ConditionProperty;
        private Faction Faction => Game.World.Faction;

        public async void QueryIdleStory(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            var lastUpdate = dizi.Idle.LastUpdate;
            var now = SysTime.UnixNow;
            var elapsedSecs = (int)TimeSpan.FromMilliseconds(now - lastUpdate).TotalSeconds;
            var story = IdleCfg.IdleMapSo.TryGetStory(elapsedSecs);
            if (story == null) return;
            var handler = new StoryHandler(story, new AdvEventMiddleware(BattleSimulation, ConditionProperty));
            await handler.Invoke(dizi, now, -1);//闲置状态没有里数概念,所以里数-1
            dizi.RegIdleStory(handler.AdvLog);
        }
    }
}