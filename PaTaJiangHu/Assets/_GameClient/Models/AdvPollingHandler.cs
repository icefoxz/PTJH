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
        protected abstract string DiziName { get; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime { get; }
        /// <summary>
        /// 上次更新时间
        /// </summary>
        public long LastUpdate { get; private set; }

        protected AdvPollingHandler(long startTime)
        {
            StartTime = startTime;
            LastUpdate = startTime;
            CoService = new CoPollingInstance(1, UpdateEverySecs);
            CoService.StartService(DiziName);
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

        protected abstract void PollingUpdate();
        protected abstract void StoryUpdate();

        //更新冒险位置
        protected void UpdateTime(long updatedTicks) => LastUpdate = updatedTicks;

        /// <summary>
        /// 只会执行一次, 第二次开始无效
        /// </summary>
        protected virtual void StopService() => CoService.StopService();
    }
}