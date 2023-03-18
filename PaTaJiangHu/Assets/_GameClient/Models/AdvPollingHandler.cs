using Server.Controllers;
using System;

namespace _GameClient.Models
{
    /// <summary>
    /// 事件框架下的轮询处理器
    /// </summary>
    public abstract class AdvPollingHandler
    {
        protected enum Modes
        {
            Polling,
            Story
        }

        protected Modes Mode { get; set; }//内部使用的状态模式
        protected CoPollingInstance CoService { get; }
        protected virtual string CoName => string.Empty;
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime { get; }
        /// <summary>
        /// 上次更新时间
        /// </summary>
        public long LastUpdate { get; private set; }
        private DiziActivityPlayer ActivityPlayer { get; }
        protected AdvPollingHandler(long startTime,string diziName, DiziActivityPlayer activityPlayer)
        {
            StartTime = startTime;
            LastUpdate = startTime;
            CoService = new CoPollingInstance(1, UpdateEverySecs);
            CoService.StartService(diziName, CoName);
            ActivityPlayer = activityPlayer;
        }

        private void UpdateEverySecs()
        {
            switch (Mode)
            {
                case Modes.Polling:
                    PollingUpdate();
                    break;
                case Modes.Story:
                    StoryUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void UpdateServiceName(string text = null)
        {
#if UNITY_EDITOR
            CoService.UpdateCoName(CoName + Mode + $".{text}");
#endif
        }

        protected void SetMode(Modes mode) => Mode = mode;

        /// <summary>
        /// 轮询模式, 每隔{自定义秒数}更新
        /// </summary>
        protected abstract void PollingUpdate();
        /// <summary>
        /// 故事模式, 每隔{自定义秒数}更新
        /// </summary>
        protected abstract void StoryUpdate();

        /// <summary>
        /// 当有故事注册的时候{一般上都是控制器调用}
        /// </summary>
        /// <param name="story"></param>
        internal void RegStory(DiziActivityLog story)
        {
            ActivityPlayer.Reg(story);
            OnRegStory(story);
        }

        protected abstract void OnRegStory(DiziActivityLog story);

        //更新冒险位置
        protected void UpdateTime(long updatedTicks) => LastUpdate = updatedTicks;

        /// <summary>
        /// 只会执行一次, 第二次开始无效
        /// </summary>
        protected virtual void StopService() => CoService.StopService();
    }
}