using System;
using AOT.Core;
using AOT.Core.Dizi;
using AOT.Utls;
using GameClient.Models;
using GameClient.SoScripts;
using GameClient.SoScripts.BattleSimulation;
using GameClient.System;

namespace GameClient.Controllers
{

    public class DiziIdleController : IGameController
    {
        private Config.DiziIdle IdleCfg => Game.Config.Idle;
        private Config.Recruit RecruitCfg => Game.Config.RecruitCfg;
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private Faction Faction => Game.World.Faction;

        public async void QueryIdleStory(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.State.Current != DiziStateHandler.States.Idle) return;
            var lostStrategy = IdleCfg.IdleMapSo.LostStrategy;
            var lastUpdate = dizi.State.Idle.LastUpdate;
            var now = SysTime.UnixNow;
            var elapsedSecs = (int)TimeSpan.FromMilliseconds(now - lastUpdate).TotalSeconds;
            var story = IdleCfg.IdleMapSo.TryGetStory(elapsedSecs);
            if (story == null) return;
            var handler = new StoryHandler(story,
                new AdvEventMiddleware(BattleSimulation, BattleCfg), lostStrategy);
            await handler.Invoke(dizi, now, -1);//闲置状态没有里数概念,所以里数-1
            if (handler.IsLost)
            {
                SetLost(guid, handler.ActivityLog);
                return;
            }

            if (lostStrategy?.IsInTerm(dizi) ?? false) //如果状态触发失踪
            {
                var luck = Sys.Luck;
                if (lostStrategy.IsTriggerConditionLost(luck)) //如果触发失踪
                {
                    SetLost(guid, handler.ActivityLog);
                    return;
                }
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

        private bool FactionTryTrade(int silver, int yuanBao)
        {
            if (Faction.Silver < silver)
            {
                SendEvent(EventString.Info_Trade_Failed_Silver, silver);
                return false;
            }

            if (Faction.YuanBao < yuanBao)
            {
                SendEvent(EventString.Info_Trade_Failed_YuanBao, silver);
                return false;
            }

            Faction.AddYuanBao(-yuanBao);
            Faction.AddSilver(-silver);
            return true;
        }

        private void SendEvent(string eventName, params object[] args) =>
            Game.MessagingManager.SendParams(eventName, args);

        public void RestoreDizi(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            var restoreCost = RecruitCfg.GradeCfg.GetRestoreCost((ColorGrade)dizi.Capable.Grade);
            if (FactionTryTrade(0, -restoreCost))
            {
                dizi.RestoreFromLost();
            }
        }
    }
}