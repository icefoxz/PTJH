using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _GameClient.Models;
using Server.Configs.Adventures;

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
        private IAdvEvent CurrentEvent { get; set; }
        private IAdvEventMiddleware EventMiddleware { get; }
        private TaskCompletionSource<IAdvEvent> OnNextEventTask { get; set; }
        public bool IsAdvFailed { get; private set; }//是否历练强制失败
        public bool IsForceQuit { get; private set; }//强制历练结束
        public DiziAdvLog AdvLog { get; private set; }//故事信息

        public StoryHandler(IAdvStory story, AutoAdvEventMiddleware eventMiddleware)
        {
            Story = story;
            CurrentEvent = Story.StartAdvEvent;
            EventMiddleware = eventMiddleware;
        }

        public async Task Invoke(Dizi dizi, long nowTicks, int updatedMiles)
        {
            var eventQueue = new Queue<string>(QueueLimit);
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
                if (CurrentEvent is AdvQuitEventSo q)
                {
                    IsAdvFailed = q.IsAdvFailed;
                    IsForceQuit = q.IsForceQuit;
                    break;
                }
                CurrentEvent.OnLogsTrigger += OnLogsTrigger;
                CurrentEvent.OnNextEvent += OnNextEventTrigger;
                OnNextEventTask = new TaskCompletionSource<IAdvEvent>();
                var advArg = EventMiddleware.Invoke(advEvent: CurrentEvent, rewardHandler: dizi.Adventure, dizi: dizi);
                CurrentEvent.EventInvoke(advArg);
                var nextEvent = await OnNextEventTask.Task;
                CurrentEvent = nextEvent;
                _recursiveIndex++;
            }

            AdvLog = new DiziAdvLog(Messages.ToArray(), dizi.Guid, nowTicks, updatedMiles);
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
}