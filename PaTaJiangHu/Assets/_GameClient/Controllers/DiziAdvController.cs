using AOT.Core;
using AOT.Utls;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.SoScripts;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;
using GameClient.System;
using System;
using System.Linq;
using GameClient.Test;

namespace GameClient.Controllers
{
    /// <summary>
    /// 弟子历练控制器
    /// </summary>
    public class DiziAdvController : IGameController
    {
        /*注意,为了游戏方便, 这里的 '里' 这个单位用 'mile' 命名, 但与现实中的英里单位没关系*/
        private int EventLogSecs => AdventureCfg.EventLogSecs;//信息弹出秒数间隔
        private int MinuteInMile => AdventureCfg.MinuteInMile;//多少分钟 = 1里
        private AdventureConfigSo AdventureCfg => Game.Config.AdvCfg.AdventureCfg;
        private AdventureMapSo[] AdvMaps => AdventureCfg.AdvMaps;//回城秒数
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private Faction Faction => Game.World.Faction;
        private RewardController RewardController => Game.Controllers.Get<RewardController>();
        private DiziIdleController IdleController => Game.Controllers.Get<DiziIdleController>();
        private DiziLostController LostController => Game.Controllers.Get<DiziLostController>();

        private GameWorld.DiziState State => Game.World.State;
        private Adventure_ActivityManager Adventure => State.Adventure;
        private AdventureActivityHandler AdventureHandler { get; }
        public DiziAdvController()
        {
            AdventureHandler = new AdventureActivityHandler(AdventureCfg, BattleCfg, BattleSimulation);
        }

        /// <summary>
        /// 获取所有可历练的地图,
        /// 0 = 历练, 1 = 生产
        /// </summary>
        /// <returns></returns>
        public IAutoAdvMap[] AutoAdvMaps(int mapType) => mapType switch
        {
            0 => AdventureCfg.AdvMaps,
            1 => AdventureCfg.ProductionMaps,
            _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
        };

        /// <summary>
        /// 设定弟子历练装备
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="itemIndex"></param>
        /// <param name="slot"></param>
        public void SetDiziAdvItem(string guid, int itemIndex, int slot)
        {
            var dizi = Faction.GetDizi(guid);
            var item = Faction.GetAdventureItems()[itemIndex];
            Faction.RemoveGameItem(item);
            var replaceItem = dizi.AdvItems[slot];
            if (replaceItem != null) Faction.AddGameItem(replaceItem.Item, false);
            dizi.SetAdvItem(slot, item);
        }

        public void RemoveDiziAdvItem(string guid, int itemIndex, int slot)
        {
            var dizi = Faction.GetDizi(guid);
            var item = dizi.RemoveAdvItem(slot);
            Faction.AddGameItem(item, false);
        }
        //历练开始
        public void AdventureStart(string guid, int mapId)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.Activity != DiziActivities.Idle)
            {
                throw new InvalidOperationException($"当前弟子状态[{dizi.Activity}]不允许历练!");
            }
            IdleController.StopIdle(guid);
            var (map, isProduction) = AdventureHandler.GetMap(mapId);
            if (Faction.ActionLing < map.ActionLingCost)
            {
                XDebug.LogWarning($"行动令{Faction.ActionLing}不足以执行{map.Name}.体力消耗={map.ActionLingCost}");
                return;
            }

            State.RemoveStateless(guid);//确保弟子是从无状态中获取的

            var now = SysTime.UnixNow;
            var activity = new AdventureActivity(map, now, dizi);
            dizi.SetState(activity);
            Adventure.ActivityStart(activity);
        }

        //让弟子回程
        public void AdventureRecall(string guid)
        {
            var activity = Adventure.GetActivity(guid);
            if (activity.State is not AdventureActivity.States.Progress)
            {
                var dizi = Faction.GetDizi(guid);
                XDebug.LogError($"状态异常!{dizi}不可以召回 : {activity.State}");
            }
            AdventureHandler.ActivityRecall(activity);
            Adventure.ActivityRecall(guid);
        }

        public void AdventureReturn(string guid)
        {
            var activity = Adventure.GetActivity(guid);
            AdventureHandler.ActivityReturn(activity);
            Adventure.ActivityReturn(guid);
        }

        /// <summary>
        /// 让等待的弟子回到山门内, 仅是等待状态的弟子可以执行这个方法
        /// </summary>
        /// <param name="guid"></param>
        public void AdventureFinalize(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            var activity = Adventure.GetActivity(dizi.Guid);
            if (activity.State != AdventureActivity.States.Waiting)
                XDebug.Log($"弟子当前状态异常! : {activity.State}");
            RewardController.SetRewards(activity.Rewards.ToArray());
            Adventure.ActivityRemove(guid);
            State.AddStateless(dizi);
            IdleController.IdleStart(dizi.Guid);
        }

        private void AdventureUpdate(DiziActivityLog log) => Adventure.ActivityUpdate(log);

        /// <summary>
        /// 自动检查里数, 当里数故事执行完毕会在回调中返回故事距离和是否历练继续
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="onEventUpdate">回调返回历练总里数,和是否历练继续, true = 冒险结束</param>
        public async void UpdateActivity(AdventureActivity activity)
        {
            var now = SysTime.UnixNow;
            await AdventureHandler.UpdateActivity(now, activity, AdventureUpdate, SetLost);
        }
        //计算出间隔里数
        private int GetMiles(long now, long lastUpdate)
        {
            var millisecondInterval = now - lastUpdate;
            if (millisecondInterval < 0)
                throw new InvalidOperationException(
                    $"Interval ={millisecondInterval}! last = {lastUpdate}, now = {now},");
            //如果有马匹改变里数, 先计算马匹并叠加+里数和减去-时间. 剩余时间才与实际里数时间相乘
            //由于使用中物品目前只有马匹, 但使用中状态剩余多少百分比还是有必要记录, 并且是一并记录使用那个一物品
            var timeSpan = SysTime.MillisecondsToTimeSpan(millisecondInterval);
            var miles = (int)(MinuteInMile * timeSpan.TotalMinutes);
            return miles;
        }

        /// <summary>
        /// 获取所有的触发主要故事点的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMajorMiles(int mapId) => AdventureHandler.GetMap(mapId).map.ListMajorMiles();

        /// <summary>
        /// 获取所有触发小故事的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMinorMiles(int mapId)
        {
            var miles = AdventureHandler.GetMap(mapId).map.ListMinorMiles();
            return miles;
        }

        //历练状态失踪方法
        private void SetLost(DiziActivityLog log)
        {
            var dizi = Faction.GetDizi(log.DiziGuid);
            StopActivity(dizi);
            LostController.LostStart(dizi.Guid, log);
        }

        private void StopActivity(Dizi dizi)
        {
            Adventure.ActivityRemove(dizi.Guid);
            State.AddStateless(dizi);
        }

        public void Terminate(string guid)
        {
            var dizi = Adventure.GetActivity(guid).Dizi;
            StopActivity(dizi);
        }
    }
}