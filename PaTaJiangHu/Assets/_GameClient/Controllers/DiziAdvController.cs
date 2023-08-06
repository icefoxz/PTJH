using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AOT.Core;
using AOT.Utls;
using GameClient.Models;
using GameClient.Models.States;
using GameClient.Modules.DiziM;
using GameClient.SoScripts;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;
using GameClient.System;

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
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private Config.BattleConfig BattleCfg => Game.Config.BattleCfg;
        private AdventureMapSo[] AdvMaps => AdventureCfg.AdvMaps;//回城秒数
        private Faction Faction => Game.World.Faction;
        private RewardController RewardController => Game.Controllers.Get<RewardController>();
        private DiziAdventure Adventure => Game.World.Adventure;
        private DiziProduction Production => Game.World.Production;
        private DiziLost Lost => Game.World.Lost;

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

        private (AdventureMapSo map,bool isProduction) GetMap(int mapId)
        {
            var map = AdvMaps.FirstOrDefault(m => m.Id == mapId);
            var isProduction = false;
            if (map == null)
            {
                map = AdventureCfg.ProductionMaps.FirstOrDefault(m => m.Id == mapId);
                isProduction = true;
            }
            if (map == null)
                XDebug.LogError($"找不到地图id={mapId},请确保地图已配置!");
            return (map, isProduction);
        }

        /// <summary>
        /// 设定弟子历练装备
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="itemIndex"></param>
        /// <param name="slot"></param>
        public void SetDiziAdvItem(string guid,int itemIndex,int slot)
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
            var (map, isProduction) = GetMap(mapId);
            if (Faction.ActionLing < map.ActionLingCost)
            {
                XDebug.LogWarning($"行动令{Faction.ActionLing}不足以执行{map.Name}.体力消耗={map.ActionLingCost}");
                return;
            }

            var dizi = Faction.GetDizi(guid);
            if (dizi.State.Current != DiziStateHandler.States.Idle)
                XDebug.Log($"状态切换异常! : {dizi.State.Current}");
            dizi.StopIdle();
            dizi.AdventureStart(map, SysTime.UnixNow, EventLogSecs, isProduction);

            DiziStateModel stateModel = isProduction ? Production : Adventure;
            stateModel.AddDizi(dizi);
        }

        //让弟子回程
        public void AdventureRecall(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.State.Current is not (
                DiziStateHandler.States.AdvProgress or 
                DiziStateHandler.States.AdvProduction))
                XDebug.LogError($"状态异常!{dizi}不可以召回 : {dizi.State.Current}");
            CheckMile(dizi.State.Adventure.Map.Id, dizi.Guid, null, () =>
            {
                if (dizi.State.Adventure.State == AutoAdventure.States.Progress) DirectRecallAdv(guid);
            });
        }

        //强制弟子历练回程, 不检查间隔里数生成故事
        public void DirectRecallAdv(string diziGuid)
        {
            var now = SysTime.UnixNow;
            var dizi = Faction.GetDizi(diziGuid);
            if (dizi.State.Adventure.State != AutoAdventure.States.Progress)
                throw new InvalidOperationException($"弟子[{dizi.Name}]当前状态[{dizi.State.Current}]不能执行历练回程!");
            var reachingTime = GetReachingTime(dizi);

            var recallMsg = $"{dizi.Name}回程中...";
            var totalMile = dizi.State.Adventure.LastMile;
            var recallLog = new DiziActivityLog(dizi.Guid, now, totalMile);//生成回程活动
            recallLog.SetMessages(new[] { recallMsg });//回程描述
            dizi.AdventureStoryLogging(recallLog);//注册活动
            dizi.AdventureRecall(now, totalMile, reachingTime);//执行回程
        }

        /// <summary>
        /// 获取回程时间
        /// </summary>
        /// <param name="dizi"></param>
        /// <returns></returns>
        public long GetReachingTime(Dizi dizi)
        {
            var now = SysTime.UnixNow;
            var adv = dizi.State.Adventure;
            var reachingTime = now + adv.Map.ProductionReturnSec * 1000;//转化成milliseconds
            if (adv.AdvType == AutoAdventure.AdvTypes.Adventure && !adv.Map.IsFixReturnTime)
                reachingTime = now + TimeSpan.FromSeconds(AdventureCfg
                        .GetSeconds(dizi.State.Adventure.LastMile) / 2d).Milliseconds;//如果是历练, 时间将根据当前里数计算回程秒数
            return reachingTime;
        }

        /// <summary>
        /// 让等待的弟子回到山门内, 仅是等待状态的弟子可以执行这个方法
        /// </summary>
        /// <param name="guid"></param>
        public void AdventureFinalize(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.State.Current != DiziStateHandler.States.AdvProgress)
                XDebug.Log($"弟子当前状态异常! : {dizi.State.Current}");
            if (dizi.State.Adventure.State != AutoAdventure.States.End)
                XDebug.Log($"操作异常!弟子状态 = {dizi.State.Adventure?.State}");
            RewardController.SetRewards(dizi.State.Adventure.Rewards.ToArray());
            dizi.AdventureFinalize();
            dizi.StartIdle(SysTime.UnixNow);
            if(Adventure.Contains(dizi))
                Adventure.RemoveDizi(dizi);
            else Production.RemoveDizi(dizi);
        }

        /// <summary>
        /// 自动检查里数, 当里数故事执行完毕会在回调中返回故事距离和是否历练继续
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="diziGuid"></param>
        /// <param name="onEventUpdate">回调返回历练总里数,和是否历练继续, true = 冒险结束</param>
        /// <param name="onFinishAction">当执行完毕的回调, 这个回调是一定执行.并且确保异步事件结束后会调用</param>
        public async void CheckMile(int mapId, string diziGuid,
            Action<(int mile, string placeName, bool adventureEnd)> onEventUpdate,
            Action onFinishAction = null)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var lastMile = dizi.State.Adventure.LastMile;
            var lastUpdate = dizi.State.Adventure.LastUpdate;
            var now = SysTime.UnixNow;
            var miles = GetMiles(now, lastUpdate);

            if (miles == 0)
            {
                onFinishAction?.Invoke();
                return; 
            }
            var totalMiles = lastMile + miles;
            var (stopAdv, placeName) = await OnAdventureProgress(mapId, now, lastMile, totalMiles, diziGuid);
            onEventUpdate?.Invoke((totalMiles, placeName, stopAdv));
            onFinishAction?.Invoke();
        }

        /// <summary>
        /// 根据当前走过的路段触发响应的故事点, 返回故事是否结束
        /// </summary>
        /// <param name="mapId"></param>
        /// <param name="now"></param>
        /// <param name="fromMiles"></param>
        /// <param name="toMiles"></param>
        /// <param name="diziGuid"></param>
        public async Task<(bool stopAdv,string placeName)> OnAdventureProgress(int mapId,long now, int fromMiles, int toMiles, string diziGuid)
        {
            var dizi = Faction.GetDizi(diziGuid);
            if (dizi.State.Adventure is not { State: AutoAdventure.States.Progress }) return (true, string.Empty);//返回故事继续
            var (map, isProduction) = GetMap(mapId);
            var places = map.PickAllTriggerPlaces(fromMiles, toMiles);//根据当前路段找出故事地点
            if (places.Length <= 0)
            {
                XDebug.Log($"{dizi}当前({fromMiles}~{toMiles}里)已在[{map.Name}]找不到任何可执行的地点~自动返回!");
                return (true, string.Empty); //结束故事
            }
            for (var i = 0; i < places.Length; i++) //为每一个故事地点获取一个故事
            {
                var place = places[i];
                var story = await ProcessStory(place, now, toMiles, dizi, map);
                //当获取到地点, 执行故事
                dizi.AdventureStoryLogging(story.ActivityLog);
                if (story.IsLost)//强制失踪事件
                {
                    SetLost(dizi.Guid, story.ActivityLog);
                    return (true, place.Name);
                }

                if (map.LostStrategy?.IsInTerm(dizi) ?? false) //如果弟子状态触发失踪
                {
                    var luck = Sys.Luck;
                    if (map.LostStrategy.IsTriggerConditionLost(luck))
                    {
                        SetLost(dizi.Guid, story.ActivityLog);
                        return (true, place.Name);
                    }
                }
                if (story.IsAdvFailed) //当历练失败
                {
                    //执行历练失败的处罚
                    return (true, place.Name);
                }

                if (story.IsForceQuit) return (true, place.Name); //结束故事
            }

            if (map.MaxMiles < toMiles) //如果超过最大里数
            {
                XDebug.Log($"{dizi}当前({fromMiles}~{toMiles}里)已超过[{map.Name}]最大里数:[{map.MaxMiles}],历练结束!");
                return (true, string.Empty);
            }
            return (false, places.Last().Name); //返回故事继续
        }
        /// <summary>
        /// 获取所有的触发主要故事点的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMajorMiles(int mapId) => GetMap(mapId).map.ListMajorMiles();

        /// <summary>
        /// 获取所有触发小故事的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMinorMiles(int mapId)
        {
            var miles = GetMap(mapId).map.ListMinorMiles();
            return miles;
        }

        //获取故事信息
        private async Task<StoryHandler> ProcessStory(IAdvPlace place, long nowTicks, int updatedMiles, Dizi dizi,
            AdventureMapSo map)
        {
            var story = place.WeighPickStory(); //根据权重随机故事
            var eventHandler = new AdvEventMiddleware(BattleSimulation, BattleCfg); //生成事件处理器
            var storyHandler = new StoryHandler(story, eventHandler, map.LostStrategy); //生成故事处理器
            await storyHandler.Invoke(dizi, nowTicks, updatedMiles);
            return storyHandler;
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

        //历练状态失踪方法
        private void SetLost(string guid, DiziActivityLog lastLog)
        {
            var dizi = Faction.GetDizi(guid);
            var now = SysTime.UnixNow;
            dizi.AdventureTerminate(now, lastLog.LastMiles);
            dizi.StartLostState(now, lastLog);
            Lost.AddDizi(dizi);
        }
    }

    public record DiziActivityLog
    {
        public string[] Messages { get; set; }
        public string DiziGuid { get; set; }
        public long NowTicks { get; set; }
        public int LastMiles { get; set; }
        public List<string> AdjustEvents { get; set; }
        public IGameReward Reward { get; set; }

        public DiziActivityLog(string diziGuid, long nowTicks, int lastMiles)
        {
            DiziGuid = diziGuid;
            NowTicks = nowTicks;
            LastMiles = lastMiles;
            AdjustEvents = new List<string>();
        }

        public void SetMessages(string[] messages)
        {
            Messages = messages;
        }

        public void SetReward(IGameReward reward)
        {
            Reward = reward;
        }

        public void AddAdjustmentInfo(string[] adjustMessages)
        {
            AdjustEvents.AddRange(adjustMessages);
        }
    }
}