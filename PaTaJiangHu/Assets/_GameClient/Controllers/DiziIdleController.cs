using System;
using AOT.Core;
using AOT.Utls;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.SoScripts;
using GameClient.SoScripts.BattleSimulation;
using GameClient.System;

namespace GameClient.Controllers
{
    public class DiziIdleController : IGameController
    {
        private Config.DiziIdle IdleCfg => Game.Config.Idle;
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private Idle_ActivityManager Idle => State.Idle;
        private GameWorld.DiziState State => Game.World.State;
        private DiziLostController LostController => Game.Controllers.Get<DiziLostController>();

        public void IdleStart(string guid)
        {
            var dizi = State.RemoveStateless(guid);
            Idle.ActivityStart(dizi, IdleCfg.IdleMapSo, SysTime.UnixNow);
        }

        public async void QueryIdleStory(string guid)
        {
            var activity = Idle.GetActivity(guid);
            var dizi = activity.Dizi;
            var lostStrategy = IdleCfg.IdleMapSo.LostStrategy;
            var lastUpdate = activity.LastUpdate;
            var now = SysTime.UnixNow;
            var elapsedSecs = (int)TimeSpan.FromMilliseconds(now - lastUpdate).TotalSeconds;
            var map = IdleCfg.IdleMapSo;
            var story = map.TryGetStory(elapsedSecs);
            if (story == null) return;
            XDebug.Log($"弟子[{dizi.Name}], 执行闲置故事[{story.name}].");
            var storyHandler = new StoryHandler(story,
                new AdvEventMiddleware(BattleSimulation, BattleCfg), lostStrategy);
            await storyHandler.Invoke(dizi, now, -1, string.Join('-', map.Name, story.Name));//闲置状态没有里数概念,所以里数-1
            if (storyHandler.IsLost)
            {
                SetLost(guid, storyHandler.ActivityLog);
                return;
            }

            if (lostStrategy?.IsInTerm(dizi) ?? false) //如果状态触发失踪
            {
                var luck = Sys.Luck;
                if (lostStrategy.IsTriggerConditionLost(luck)) //如果触发失踪
                {
                    SetLost(guid, storyHandler.ActivityLog);
                    return;
                }
            }

            Idle.ActivityUpdate(dizi, storyHandler.ActivityLog);
        }

        //闲置状态失踪方法
        private void SetLost(string guid, DiziActivityLog lastLog)
        {
            var dizi = Idle.GetActivity(guid).Dizi;
            StopIdle(guid);
            LostController.LostStart(dizi.Guid, lastLog);
        }

        public void StopIdle(string guid)
        {
            var dizi = Idle.GetActivity(guid).Dizi;
            var endTime = SysTime.UnixNow;
            Idle.ActivityEnd(dizi, endTime);
            State.AddStateless(dizi);
        }
    }
}