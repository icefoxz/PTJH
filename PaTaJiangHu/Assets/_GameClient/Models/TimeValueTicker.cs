using System;

namespace _GameClient.Models
{
    /// <summary>
    /// 时间值戳, 主要为一些时间的状态类型的计算, 以<see cref="ZeroTicks"/>为状态值为0的基础导算
    /// </summary>
    public abstract class TimeValueTicker
    {
        private long _zeroTicks;
        public long ZeroTicks => _zeroTicks;

        protected TimeValueTicker(long zeroTicks)
        {
            _zeroTicks = zeroTicks;
        }
        /// <summary>
        /// 当<see cref="ZeroTicks"/>更新
        /// </summary>
        protected abstract void OnUpdate();
        public void Update(long zeroTicks)
        {
            _zeroTicks = zeroTicks;
            OnUpdate();
        }

        public void Update(long ticks, int value, TimeSpan timePerValue)
        {
            _zeroTicks = CountZeroTicks(ticks, value, timePerValue);
            OnUpdate();
        }

        public static long CountZeroTicks(long ticks, int value, TimeSpan timePerValue) =>
            (long)(ticks - timePerValue.TotalMilliseconds * value);

        public static int CountCurrentValue(long fromTicks, long toTicks, TimeSpan timePerValue)
        {
            var interval = fromTicks - toTicks;
            if (interval < 0) return 0;
            return (int)(interval / timePerValue.TotalMilliseconds);
        }
    }
}