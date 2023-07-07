using System;
using AOT._AOT.Utls;
using GameClient.Modules.DiziM;

namespace GameClient.Models
{
    /// <summary>
    /// 状态时间戳
    /// </summary>
    public class ConditionTicker : TimeValueTicker
    {
        private ConValue _con;
        public IGameCondition Con => _con;
        public TimeSpan TimePerValue { get; }

        public ConditionTicker(TimeSpan timePerValue, long zeroTicks, int max) : base(zeroTicks)
        {
            TimePerValue = timePerValue;
            var value = CountCurrentValue(zeroTicks, SysTime.UnixNow, timePerValue);
            value = Math.Max(value, max);
            _con = new ConValue(max, max, value);
        }

        protected override void OnUpdate()
        {
            var value = CountCurrentValue(ZeroTicks, SysTime.UnixNow, TimePerValue);
            _con.Set(value);
        }
    }
}