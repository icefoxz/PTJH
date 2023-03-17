using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _GameClient.Models;
using Server.Configs.Adventures;
using Utls;

namespace Server.Controllers
{
    internal interface IAdvEventMiddleware
    {
        IAdvArg Invoke(IAdvEvent advEvent,IRewardHandler rewardHandler ,Dizi dizi);
    }
    /// <summary>
    /// 故事处理器
    /// </summary>
    internal class StoryHandler
    {
        private const int RecursiveLimit = 9999;
        private const int QueueLimit = 100;
        private int _recursiveIndex = 0;
        private List<string> Messages { get; } = new List<string>();

        private IAdvStory Story { get; }
        private LostStrategySo LostStrategy { get; }
        private IAdvEvent CurrentEvent { get; set; }
        private IAdvEventMiddleware EventMiddleware { get; }
        private TaskCompletionSource<IAdvEvent> OnNextEventTask { get; set; }
        public bool IsAdvFailed { get; private set; }//是否历练强制失败
        public bool IsForceQuit { get; private set; }//强制历练结束
        public bool IsLost { get; private set; }//强制失踪
        public DiziActivityLog ActivityLog { get; private set; }//故事信息

        public StoryHandler(IAdvStory story, AdvEventMiddleware eventMiddleware, LostStrategySo lostStrategy)
        {
            Story = story;
            CurrentEvent = Story.StartAdvEvent;
            EventMiddleware = eventMiddleware;
            LostStrategy = lostStrategy;
        }

        public async Task Invoke(Dizi dizi, long nowTicks, int updatedMiles)
        {
            var eventQueue = new Queue<string>(QueueLimit);
            ActivityLog = new DiziActivityLog(dizi.Guid, nowTicks, updatedMiles);
            while (CurrentEvent != null)
            {
                if (eventQueue.Count >= QueueLimit)
                    eventQueue.Dequeue();
                eventQueue.Enqueue(CurrentEvent.name);
                if (_recursiveIndex >= RecursiveLimit)
                    throw new StackOverflowException(
                        $"故事{Story.Name} 死循环!检查其中事件{CurrentEvent.name}\n事件经过:{string.Join(',', eventQueue)}");
                //如果强制退出事件
                IsAdvFailed = dizi.Stamina.Con.IsExhausted && !Story.ContinueOnExhausted;
                if (IsAdvFailed) break;
                CurrentEvent.OnLogsTrigger += OnLogsTrigger;
                CurrentEvent.OnNextEvent += OnNextEventTrigger;
                CurrentEvent.OnAdjustmentEvent += OnAdjustEventTrigger;
                CurrentEvent.OnRewardEvent += OnRewardTrigger;
                var advArg = EventMiddleware.Invoke(advEvent: CurrentEvent, rewardHandler: dizi.Adventure, dizi: dizi);
                OnNextEventTask = new TaskCompletionSource<IAdvEvent>();
                CurrentEvent.EventInvoke(advArg);

                if (CurrentEvent is AdvQuitEventSo q)
                {
                    XDebug.Log($"{dizi.Name},历练结束事件: 历练失败:{q.IsAdvFailed}, 强制回程:{q.IsForceQuit}, 是否失踪:{q.IsForceQuit}");
                    IsAdvFailed = q.IsAdvFailed;
                    IsForceQuit = q.IsForceQuit;
                    IsLost = q.IsLost;
                    break;
                }
                if (LostStrategy != null && (advArg.SimOutcome?.IsPlayerWin ?? false)) //如果有战斗并战败 + 失踪策略
                {
                    var luck = Sys.Luck;
                    IsLost = LostStrategy.IsTriggerBattleLost(luck);//如果触发概率
                    break;
                }

                var nextEvent = await OnNextEventTask.Task;
                CurrentEvent = nextEvent;
                _recursiveIndex++;
            }
            ActivityLog.SetMessages(Messages.ToArray());
        }

        private void OnRewardTrigger(IGameReward reward)
        {
            CurrentEvent.OnRewardEvent -= OnRewardTrigger;
            ActivityLog.SetReward(reward);
        }

        private void OnAdjustEventTrigger(string[] adjustMessages)
        {
            CurrentEvent.OnAdjustmentEvent -= OnAdjustEventTrigger;
            ActivityLog.AddAdjustmentInfo(adjustMessages);
        }

        private void OnNextEventTrigger(IAdvEvent nextEvent)
        {
            CurrentEvent.OnNextEvent -= OnNextEventTrigger;
            try
            {
                OnNextEventTask.TrySetResult(nextEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void OnLogsTrigger(string[] logs)
        {
            CurrentEvent.OnLogsTrigger -= OnLogsTrigger;
            Messages.AddRange(logs);
        }
    }
}