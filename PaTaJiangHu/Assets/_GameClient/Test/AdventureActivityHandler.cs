using System;
using System.Linq;
using System.Threading.Tasks;
using AOT.Utls;
using GameClient.Controllers;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.SoScripts;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;

namespace GameClient.Test
{
    public class AdventureActivityHandler
    {
        private AdventureConfigSo AdventureCfg { get; }
        private Config.BattleConfig BattleCfg { get; }
        private BattleSimulatorConfigSo BattleSimulation { get; }
        private AdventureMapSo[] AdvMaps => AdventureCfg.AdvMaps; //回城秒数

        internal AdventureActivityHandler(AdventureConfigSo adventureCfg, Config.BattleConfig battleCfg,
            BattleSimulatorConfigSo battleSimulation)
        {
            AdventureCfg = adventureCfg;
            BattleCfg = battleCfg;
            BattleSimulation = battleSimulation;
        }

        /// <summary>
        /// 自动检查里数, 当里数故事执行完毕会在回调中返回故事距离和是否历练继续
        /// </summary>
        public async Task UpdateActivity(long checkTime,AdventureActivity activity,
            Action<DiziActivityLog> onActivity,
            Action<DiziActivityLog> onLost)
        {
            var lastMile = activity.LastMiles;
            var lastUpdate = activity.LastUpdate;
            var now = checkTime;
            var miles = GetMiles(now, lastUpdate);
            if (miles == 0) return;

            var totalMiles = lastMile + miles;
            await OnAdventureProgress(now, totalMiles, activity, onActivity, onLost);
        }

        /// <summary>
        /// 根据当前走过的路段触发响应的故事点
        /// </summary>
        /// <param name="now"></param>
        /// <param name="toMiles"></param>
        /// <param name="activity"></param>
        /// <param name="onActivityAction"></param>
        /// <param name="onLostAction"></param>
        private async Task OnAdventureProgress(
            long now,
            int toMiles,
            AdventureActivity activity,
            Action<DiziActivityLog> onActivityAction,
            Action<DiziActivityLog> onLostAction)
        {
            var mapId = activity.Map.Id;
            var fromMiles = activity.LastMiles;
            var dizi = activity.Dizi;
            if (activity.State is not AdventureActivity.States.Progress)
                throw new InvalidOperationException($"活动状态异常! activity.state = {activity.State}"!);

            var (map, isProduction) = GetMap(mapId);
            var places = map.PickAllTriggerPlaces(fromMiles, toMiles); //根据当前路段找出故事地点
            if (places.Length <= 0)
            {
                XDebug.Log($"{dizi}当前({fromMiles}~{toMiles}里)已在[{map.Name}]找不到任何可执行的地点~自动返回!");
                ActivityRecall(activity); //结束故事
                return; 
            }

            for (var i = 0; i < places.Length; i++) //为每一个故事地点获取一个故事
            {
                var place = places[i];
                var story = InstanceStoryHandler(place, map);
                await story.Invoke(dizi, now, toMiles, string.Join('-', place.Name, story.StoryName));
                //当获取到地点, 执行故事
                var log = story.ActivityLog;
                activity.AddLog(log);
                onActivityAction(log);
                if (story.IsLost) //强制失踪事件
                {
                    SetLost(activity, story, place);
                    onLostAction(story.ActivityLog);
                    return;
                }

                if (map.LostStrategy?.IsInTerm(dizi) ?? false) //如果弟子状态触发失踪
                {
                    var luck = Sys.Luck;
                    if (map.LostStrategy.IsTriggerConditionLost(luck))
                    {
                        SetLost(activity, story, place);
                        onLostAction(story.ActivityLog);
                        return;
                    }
                }

                if (story.IsAdvFailed) //当历练失败
                {
                    //执行历练失败的处罚
                    activity.Set(log.OccurredTime, log.LastMiles, log.Occasion, GetReachingTime(activity, log.OccurredTime));
                    return;
                }

                if (story.IsForceQuit) //历练强制结束
                {
                    activity.Set(log.OccurredTime, log.LastMiles, log.Occasion, GetReachingTime(activity, log.OccurredTime));
                    return;
                }
            }

            if (map.MaxMiles < toMiles) //如果超过最大里数
            {
                XDebug.Log($"{dizi}当前({fromMiles}~{toMiles}里)已超过[{map.Name}]最大里数:[{map.MaxMiles}],历练结束!");
                ActivityRecall(activity);
                return;
            }

            return; //返回故事继续

            void SetLost(AdventureActivity adv, StoryHandler story, IAdvPlace place)
            {
                var log = story.ActivityLog;
                adv.Set(log.OccurredTime, log.LastMiles, string.Join('-', place.Name, story.StoryName), log.OccurredTime);
            }
        }

        //获取地图信息
        internal (AdventureMapSo map, bool isProduction) GetMap(int mapId)
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

        //获取故事信息
        private StoryHandler InstanceStoryHandler(IAdvPlace place, AdventureMapSo map)
        {
            var story = place.WeighPickStory(); //根据权重随机故事
            var eventHandler = new AdvEventMiddleware(BattleSimulation, BattleCfg); //生成事件处理器
            var storyHandler = new StoryHandler(story, eventHandler, map.LostStrategy); //生成故事处理器
            return storyHandler;
        }

        //强制弟子历练回程
        public void ActivityRecall(AdventureActivity activity)
        {
            var now = SysTime.UnixNow;
            var dizi = activity.Dizi;
            if (activity.State is not AdventureActivity.States.Progress)
                throw new InvalidOperationException($"弟子[{dizi.Name}]当前状态[{activity.State}]不能执行历练回程!");
            var reachingTime = GetReachingTime(activity, now);
            var recallMsg = $"{dizi.Name}回程中...";
            var totalMile = activity.LastMiles;
            var log = new DiziActivityLog(dizi.Guid, now, totalMile, activity.CurrentOccasion); //生成回程活动
            log.SetMessages(new[] { recallMsg }); //回程描述
            activity.AddLog(log);
            activity.Set(log.OccurredTime, log.LastMiles, log.Occasion, reachingTime); //执行回程
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
            var miles = (int)(AdventureCfg.MinuteInMile * timeSpan.TotalMinutes);
            return miles;
        }

        /// <summary>
        /// 获取回程时间
        /// </summary>
        /// <returns></returns>
        private long GetReachingTime(AdventureActivity activity,long lastUpdateTime)
        {
            var now = lastUpdateTime;
            var reachingTime = now + activity.Map.FixReturnSec * 1000; //转化成milliseconds
            if (activity.AdvType == AdventureActivity.AdvTypes.Adventure && !activity.Map.IsFixReturnTime)
            {
                var mileMilliseconds = (long)TimeSpan.FromSeconds(AdventureCfg.GetSeconds(activity.LastMiles) / 2d).TotalMilliseconds;
                reachingTime = now + mileMilliseconds;
            } //如果是历练, 时间将根据当前里数计算回程秒数
            return reachingTime;
        }

        public void ActivityReturn(AdventureActivity activity)
        {
            var dizi = activity.Dizi;
            var recallMsg = $"{dizi.Name}已回到山门等待...";
            var totalMile = activity.LastMiles;
            var log = new DiziActivityLog(dizi.Guid, activity.EndTime, totalMile, "宗门"); //生成回程活动
            log.SetMessages(new[] { recallMsg }); //回程描述
            activity.AddLog(log);
            activity.Set(log.OccurredTime, log.LastMiles, log.Occasion, activity.EndTime); //执行回程
        }
    }
}