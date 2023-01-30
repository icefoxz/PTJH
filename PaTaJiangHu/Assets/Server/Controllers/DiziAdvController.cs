using _GameClient.Models;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utls;

namespace Server.Controllers
{
    /// <summary>
    /// 弟子历练控制器
    /// </summary>
    public class DiziAdvController : IGameController
    {
        /*注意,为了游戏方便, 这里的 '里' 这个单位用 'mile' 命名, 但与现实中的英里单位没关系*/
        private int EventLogSecs => AdventureCfg.EventLogSecs;//信息弹出秒数间隔
        private int JourneyReturnSec => AdventureCfg.AdvMap.JourneyReturnSec;//回城秒数
        private int MinuteInMile => AdventureCfg.MinuteInMile;//多少分钟 = 1里
        private AdventureConfigSo AdventureCfg => Game.Config.AdvCfg.AdventureCfg;
        private BattleSimulatorConfigSo BattleSimulation => Game.Config.AdvCfg.BattleSimulation;
        private ConditionPropertySo ConditionProperty => Game.Config.AdvCfg.ConditionProperty;

        private Faction Faction => Game.World.Faction;
        private RewardController RewardController => Game.Controllers.Get<RewardController>();
        public void AdventureStart(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            dizi.AdventureStart(SysTime.UnixNow, JourneyReturnSec, EventLogSecs);
        }

        public void AdventureRecall(string guid)
        {
            var now = SysTime.UnixNow;
            var dizi = Faction.GetDizi(guid);
            CheckMile(dizi.Guid, (totalMile, isAdvEnd) =>
            {
                if (dizi.Adventure.State == AutoAdventure.States.Progress)
                    dizi.AdventureRecall(now, totalMile);
            });
        }

        public void AdventureFinalize(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.Adventure is not { State: AutoAdventure.States.End })
            {
                XDebug.Log($"操作异常!弟子状态 = {dizi.Adventure?.State}");
            }
            RewardController.SetRewards(dizi.Adventure.Rewards.ToArray());
            dizi.AdventureFinalize();
        }

        /// <summary>
        /// 自动检查里数, 当里数故事执行完毕会在回调中返回故事距离和是否历练继续
        /// </summary>
        /// <param name="diziGuid"></param>
        /// <param name="onCallbackAction">回调返回历练总里数,和是否历练继续, true = 冒险结束</param>
        public async void CheckMile(string diziGuid, Action<int ,bool> onCallbackAction)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var lastMile = dizi.Adventure.LastMile;
            var lastUpdate = dizi.Adventure.LastUpdate;
            var now = SysTime.UnixNow;
            var miles = GetMiles(now, lastUpdate);
            //如果里数=0表示未去到任何地方,直接放回上个里数
            if (miles == 0)
            {
                onCallbackAction?.Invoke(lastMile, dizi.Stamina.Con.IsExhausted);
                return; //return lastMile;
            }
            var totalMiles = lastMile + miles;
            var isAdvEnd = await OnMileTrigger(now, lastMile, totalMiles, diziGuid);
            onCallbackAction?.Invoke(totalMiles, isAdvEnd);
        }

        /// <summary>
        /// 根据当前走过的路段触发响应的故事点, 返回故事是否继续
        /// </summary>
        /// <param name="now"></param>
        /// <param name="fromMiles"></param>
        /// <param name="toMiles"></param>
        /// <param name="diziGuid"></param>
        public async Task<bool> OnMileTrigger(long now, int fromMiles, int toMiles, string diziGuid)
        {
            var dizi = Faction.GetDizi(diziGuid);
            if (dizi.Adventure is not { State: AutoAdventure.States.Progress }) return true;//返回故事继续
            var places = AdventureCfg.AdvMap.PickAllTriggerPlaces(fromMiles, toMiles);//根据当前路段找出故事地点
            if (places.Length <= 0) return true; //返回故事继续
            for (int i = 0; i < places.Length; i++) //为每一个故事地点获取一个故事
            {
                var place = places[i];
                var (advLogs, forceExit) = await ProcessStory(place, now, toMiles, dizi);
                //当获取到地点, 执行故事
                foreach (var story in advLogs) 
                    dizi.AdventureStoryStart(story);

                if (forceExit || dizi.Stamina.Con.IsExhausted) //当弟子体力=0
                    return true; //返回故事结束
            }
            return false;//返回故事继续
        }
        /// <summary>
        /// 获取所有的触发主要故事点的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMajorMiles() => AdventureCfg.AdvMap.ListMajorMiles();

        /// <summary>
        /// 获取所有触发小故事的里数
        /// </summary>
        /// <returns></returns>
        public int[] GetMinorMiles(string diziGuid)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var miles = AdventureCfg.AdvMap.ListMinorMiles();
            return miles;
        }

        //获取故事信息
        private async Task<(DiziAdvLog[],bool)> ProcessStory(IAdvPlace place, long nowTicks, int updatedMiles, Dizi dizi)
        {
            var advLogs = new List<DiziAdvLog>();
            var story = place.WeighPickStory(); //根据权重随机故事
            var eventHandler = new AdvEventHandler(BattleSimulation, ConditionProperty, dizi.Adventure); //生成事件处理器
            var storyHandler = new StoryHandler(story, eventHandler); //生成故事处理器
            var (handledStory, forceExit) = await storyHandler.Invoke(dizi, nowTicks, updatedMiles);
            advLogs.Add(handledStory);
            return (advLogs.ToArray(), forceExit);
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

        #region Handler
        // 历练故事处理器
        private class StoryHandler
        {
            private const int RecursiveLimit = 9999;
            private int _recursiveIndex = 0;
            private List<string> Messages { get; } = new List<string>();

            private IAdvStory Story { get; }
            private IAdvEvent CurrentEvent { get; set; }
            private AdvEventHandler EventHandler { get; }
            private TaskCompletionSource<IAdvEvent> OnNextEventTask { get; set; }

            public StoryHandler(IAdvStory story, AdvEventHandler eventHandler)
            {
                Story = story;
                CurrentEvent = Story.StartAdvEvent;
                EventHandler = eventHandler;
            }

            public async Task<(DiziAdvLog, bool)> Invoke(Dizi dizi, long nowTicks, int updatedMiles)
            {
                var diziExhausted = false;
                while (CurrentEvent != null && CurrentEvent.AdvType != AdvTypes.Quit)
                {
                    if (_recursiveIndex >= RecursiveLimit)
                        throw new StackOverflowException($"故事{Story.Name} 死循环!检查其中事件{CurrentEvent.name}");
                    //如果强制退出事件
                    diziExhausted = Story.HaltOnExhausted && dizi.Stamina.Con.IsExhausted;
                    if (diziExhausted) break;
                    CurrentEvent.OnLogsTrigger += OnLogsTrigger;
                    CurrentEvent.OnNextEvent += OnNextEventTrigger;
                    OnNextEventTask = new TaskCompletionSource<IAdvEvent>();
                    EventHandler.Invoke(CurrentEvent, dizi);
                    var nextEvent = await OnNextEventTask.Task;
                    CurrentEvent = nextEvent;
                    _recursiveIndex++;
                }
                return (new DiziAdvLog(Messages.ToArray(), dizi.Guid, nowTicks, updatedMiles), diziExhausted);
            }

            private void OnNextEventTrigger(IAdvEvent nextEvent)
            {
                CurrentEvent.OnNextEvent -= OnNextEventTrigger;
                OnNextEventTask.TrySetResult(nextEvent);
            }

            private void OnLogsTrigger(string[] logs)
            {
                CurrentEvent.OnLogsTrigger -= OnLogsTrigger;
                Messages.AddRange(logs);
            }
        }
        // 历练事件处理器
        private class AdvEventHandler
        {
            private BattleSimulatorConfigSo Simulator { get; }
            private ConditionPropertySo Cfg { get; }
            private IRewardReceiver Receiver { get; }
            public AdvEventHandler(BattleSimulatorConfigSo simulator, ConditionPropertySo cfg, IRewardReceiver receiver)
            {
                Simulator = simulator;
                Cfg = cfg;
                Receiver = receiver;
            }

            public void Invoke(IAdvEvent advEvent, Dizi dizi)
            {
                var arg = new AdvArg(dizi,Receiver);
                switch (advEvent.AdvType)
                {
                    case AdvTypes.Option:
                    case AdvTypes.Battle:
                        throw new NotSupportedException($"历练不支持事件={advEvent.AdvType}!");
                    case AdvTypes.Quit:
                    case AdvTypes.Story:
                    case AdvTypes.Dialog:
                    case AdvTypes.Pool:
                    case AdvTypes.Term:
                    case AdvTypes.Adjust:
                    case AdvTypes.Reward: break;//其余的直接执行判断
                    case AdvTypes.Simulation://执行模拟战斗
                        if (advEvent is not BattleSimulationEventSo bs)
                            throw new NotImplementedException($"{advEvent.name} 事件类型错误!");
                        var diziSim = Cfg.GetSimulation(dizi.Name, dizi.Strength, dizi.Agility,
                            dizi.WeaponPower, dizi.ArmorPower,
                            dizi.CombatSkill.Grade, dizi.CombatSkill.Level,
                            dizi.ForceSkill.Grade, dizi.ForceSkill.Level,
                            dizi.DodgeSkill.Grade, dizi.DodgeSkill.Level);
                        var npc = bs.GetNpc(Cfg);
                        var outcome = Simulator.CountSimulationOutcome(diziSim, npc);
                        var staminaController = Game.Controllers.Get<StaminaController>();
                        if(outcome.IsPlayerWin && dizi.Stamina.Con.Value >= outcome.Result)
                        {
                            staminaController.ConsumeStamina(dizi.Guid, -outcome.Result);
                        }
                        else//当弟子战斗失败,或是血量不够扣除
                        {
                            staminaController.SetStaminaZero(dizi.Guid, true);
                        }
                        arg.SetSimulationOutcome(outcome);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                advEvent.EventInvoke(arg);
            }

            private class AdvArg : IAdvEventArg , IAdjustment
            {
                public string DiziName => Dizi.Name;
                private Dizi Dizi { get; }
                public ITerm Term { get; }
                public int InteractionResult => 0;//历练不会有交互结果
                public ISimulationOutcome SimOutcome { get; private set; }
                public IAdjustment Adjustment => this;
                public IRewardReceiver Receiver { get; }

                public AdvArg(Dizi dizi, IRewardReceiver receiver)
                {
                    Term = Dizi = dizi;
                    Receiver = receiver;
                }

                public void SetSimulationOutcome(ISimulationOutcome simulationOutcome)
                    => SimOutcome = simulationOutcome;

                public void Set(IAdjustment.Types type, int value, bool percentage)
                {
                    var controller = Game.Controllers.Get<DiziController>();
                    var adjValue = value;
                    if (percentage)
                    {
                        var conMax = type switch
                        {
                            IAdjustment.Types.Stamina => Dizi.Stamina.Con.Max,
                            IAdjustment.Types.Silver => Dizi.Silver.Max,
                            IAdjustment.Types.Food => Dizi.Food.Max,
                            IAdjustment.Types.Condition => Dizi.Emotion.Max,
                            IAdjustment.Types.Injury => Dizi.Injury.Max,
                            IAdjustment.Types.Inner => Dizi.Inner.Max,
                            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                        };
                        adjValue = value.PercentInt(conMax);
                    }
                    controller.AddDiziCon(Dizi.Guid, type, adjValue);
                }
            }
        }

        #endregion
    }

    public record DiziAdvLog
    {
        public string[] Messages { get; set; }
        public string DiziGuid { get; set; }
        public long NowTicks { get; set; }
        public int LastMiles { get; set; }

        public DiziAdvLog(string[] messages, string diziGuid, long nowTicks, int lastMiles)
        {
            Messages = messages;
            DiziGuid = diziGuid;
            NowTicks = nowTicks;
            LastMiles = lastMiles;
        }
    }
}