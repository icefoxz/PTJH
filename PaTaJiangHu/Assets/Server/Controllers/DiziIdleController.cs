using System;
using _GameClient.Models;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Utls;

namespace Server.Controllers
{

    public class DiziIdleController : IGameController
    {
        private Config.DiziIdle IdleCfg => Game.Config.Idle;
        private Config.Recruit RecruitCfg => Game.Config.RecruitCfg;
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
            if (handler.IsLost)
            {
                SetLost(guid, handler.ActivityLog);
                return;
            }
            dizi.RegIdleStory(handler.ActivityLog);
        }

        //闲置状态失踪方法
        private void SetLost(string guid, DiziActivityLog lastLog)
        {
            var dizi = Faction.GetDizi(guid);
            var now = SysTime.UnixNow;
            dizi.StopIdle();
            dizi.StartLostState(now, lastLog);
        }

        public void RestoreDizi(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            var restoreCost = RecruitCfg.GradeCfg.GetRestoreCost((GradeConfigSo.Grades)dizi.Capable.Grade);
            if (Faction.YuanBao < restoreCost)
            {
                XDebug.Log($"门派元宝:{Faction.YuanBao}, 不足!需要支付{restoreCost}以召唤回{dizi}.");
                return;
            }
            Faction.AddYuanBao(-restoreCost);
            dizi.RestoreFromLost();
        }
    }
}