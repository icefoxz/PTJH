using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AOT.Utls;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Adventures;

namespace GameClient.Controllers
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
        private const int RecursiveLimit = 999;
        private const int QueueLimit = 30;
        private int _recursiveIndex = 0;
        private List<string> Messages { get; } = new List<string>();

        public string StoryName => Story.Name;
        private IAdvStory Story { get; }
        private LostStrategySo LostStrategy { get; }
        private IAdvEvent CurrentEvent { get; set; }
        private IAdvEventMiddleware EventMiddleware { get; }
        private TaskCompletionSource<IAdvEvent> OnNextEventTask { get; set; }
        public bool IsAdvFailed { get; private set; }//是否历练强制失败
        public string DiziExhaustedOnEvent { get; private set; }//是否弟子体力耗尽
        public bool IsContinueOnExhausted { get; private set; }//是否体力耗尽继续历练
        public bool IsForceQuit { get; private set; }//强制历练结束
        public bool IsLost { get; private set; }//强制失踪
        public DiziActivityLog ActivityLog { get; private set; }//故事信息

        public StoryHandler(IAdvStory story, AdvEventMiddleware eventMiddleware, LostStrategySo lostStrategy)
        {
            Story = story;
            CurrentEvent = Story.StartAdvEvent;
            IsContinueOnExhausted = Story.ContinueOnExhausted;
            EventMiddleware = eventMiddleware;
            LostStrategy = lostStrategy;
            DiziExhaustedOnEvent = string.Empty;
        }

        public async Task Invoke(Dizi dizi, long nowTicks, int updatedMiles, string occasion)
        {
            var eventQueue = new Queue<string>(QueueLimit);
            ActivityLog = new DiziActivityLog(dizi.Guid, nowTicks, updatedMiles, occasion);
            while (CurrentEvent != null)
            {
                if (eventQueue.Count >= QueueLimit)
                    eventQueue.Dequeue();
                eventQueue.Enqueue(CurrentEvent.Name);
                if (_recursiveIndex >= RecursiveLimit)
                    throw new StackOverflowException(
                        $"故事{Story?.Name} 死循环!检查其中事件{CurrentEvent?.Name}\n事件经过:{string.Join(',', eventQueue)}");
                CurrentEvent.OnLogsTrigger += OnLogsTrigger;
                CurrentEvent.OnNextEvent += OnNextEventTrigger;
                CurrentEvent.OnAdjustmentEvent += OnAdjustEventTrigger;
                CurrentEvent.OnRewardEvent += OnRewardTrigger;
                var advArg = EventMiddleware.Invoke(advEvent: CurrentEvent, rewardHandler: ActivityLog, dizi: dizi);
                OnNextEventTask = new TaskCompletionSource<IAdvEvent>();
                var lastEvent = CurrentEvent;
                if (advArg == null)
                    throw new NullReferenceException("事件中间件 = null!");
                if (CurrentEvent == null)
                    throw new NullReferenceException(
                        $"弟子:{dizi}历练,当前事件为null!请检查故事:{Story.Name}. 上一个事件{lastEvent?.Name}!");
                if (advArg.RewardHandler == null)
                    throw new NullReferenceException(
                        $"当前奖励处理器为null,请检查状态中是否继承IRewardHandler, state = {dizi.Activity}!");

                //弟子体力信息必须在EventInvoke之前, 否则信息不会添加进去
                var diziExhausted = dizi.Stamina.Con.IsExhausted;
                if (diziExhausted)
                {
                    DiziExhaustedOnEvent = CurrentEvent.Name;
                    advArg.ExtraMessages.Add($"{dizi.Name}体力已消耗殆尽!");
                }

                try
                {
                    CurrentEvent.EventInvoke(advArg);
                }
                catch (Exception e)
                {
                    XDebug.LogError($"[{CurrentEvent?.Name}]事件异常!,上一个事件为[{lastEvent?.Name}]\n{e}");
                    throw;
                }

                //***********判断事件*************//

                if (CurrentEvent is AdvQuitEventSo q)
                {
#if UNITY_EDITOR
                    XDebug.Log($"{dizi.Name}, 故事结束: {GetQuitEventMessage(q)}");
#endif
                    IsAdvFailed = q.IsAdvFailed;
                    IsForceQuit = q.IsForceQuit;
                    IsLost = q.IsLost;
                    break;
                }

                if (LostStrategy != null && advArg.SimOutcome is { IsPlayerWin: false }) //如果有战斗并战败 + 失踪策略
                {
                    var luck = Sys.Luck;
                    IsLost = LostStrategy.IsTriggerBattleLost(luck); //如果触发概率
                    break;
                }

                //如果强制退出事件
                IsAdvFailed = diziExhausted && !IsContinueOnExhausted;
                if (IsAdvFailed) break;

                var nextEvent = await OnNextEventTask.Task;
                ResetCurrentEvent();
                CurrentEvent = nextEvent;
                _recursiveIndex++;
            }

            ActivityLog.SetMessages(Messages.ToArray());
            ResetCurrentEvent();
        }

        private string GetQuitEventMessage(AdvQuitEventSo q)
        {
            var text = string.Empty;
            if (q.IsAdvFailed)
                text += "历练失败! ";
            if (q.IsForceQuit)
                text += "强制回程! ";
            if (q.IsLost)
                text += "失踪! ";
            if(string.IsNullOrEmpty(text))
                text += "历练继续! ";
            return text;
        }

        private void ResetCurrentEvent()
        {
            CurrentEvent.OnLogsTrigger -= OnLogsTrigger;
            CurrentEvent.OnNextEvent -= OnNextEventTrigger;
            CurrentEvent.OnAdjustmentEvent -= OnAdjustEventTrigger;
            CurrentEvent.OnRewardEvent -= OnRewardTrigger;
        }

        private void OnRewardTrigger(IGameReward reward) => ActivityLog.SetReward(reward);

        private void OnAdjustEventTrigger(string[] adjustMessages) => ActivityLog.AddAdjustmentInfo(adjustMessages);

        private void OnNextEventTrigger(IAdvEvent nextEvent)
        {
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

        private void OnLogsTrigger(string[] logs) => Messages.AddRange(logs);
    }
}