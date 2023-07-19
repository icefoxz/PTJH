﻿using System;

namespace AOT.Utls
{
    /// <summary>
    /// 基础毫秒的Unix时间戳系统
    /// </summary>
    public static class SysTime
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const long ChinaTimeZoneTick = 28800000;
        public const string ChinaTimeZone = "ChinaStandardTime";
        public static readonly TimeZoneInfo ChinaTimeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(ChinaTimeZone, TimeSpan.FromHours(8), "(GMT+08:00)China Time",
            "China Time");
        /// <summary>
        /// 给出时间戳，返回是不是今天
        /// </summary>
        /// <param name="unixMillisecondsTicks"></param>
        /// <returns></returns>
        public static bool IsToday(long unixMillisecondsTicks)
        {
            var localTick = unixMillisecondsTicks + ChinaTimeZoneTick;
            var localDate = Epoch.AddMilliseconds(localTick);
            var cnDate = Now.Date;
            return cnDate.Day == localDate.Day &&
                   cnDate.Month == localDate.Month &&
                   cnDate.Year == localDate.Year;
        }
        /// <summary>
        /// 中国时段的现在时间
        /// </summary>
        public static DateTime Now => ConvertChinaTimeZone(DateTime.UtcNow);

        private static DateTime ConvertChinaTimeZone(DateTime date) => TimeZoneInfo.ConvertTimeFromUtc(date, ChinaTimeZoneInfo);

        /// <summary>
        /// 当前的unix时间戳，(UTC)
        /// </summary>
        public static long UnixNow => (long)DateTime.UtcNow.Subtract(Epoch).TotalMilliseconds;
        /// <summary>
        /// 计算是否在范围内
        /// </summary>
        /// <param name="fromTicks">从</param>
        /// <param name="range">范围</param>
        /// <param name="verifyTime">检查的时间</param>
        /// <returns></returns>
        public static bool IsInRange(long fromTicks, TimeSpan range, long verifyTime)
        {
            var timeSpan = MillisecondsToTimeSpan(verifyTime - fromTicks);
            return timeSpan <= range;
        }
        /// <summary>
        /// 从Tick获取事件跨度
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static TimeSpan MillisecondsToTimeSpan(long milliseconds) => 
            TimeSpan.FromMilliseconds(milliseconds);
        /// <summary>
        /// 从时间戳转换UTC时间
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime UtcFromUnixTicks(long milliseconds) => Epoch.AddMilliseconds(milliseconds);
        /// <summary>
        /// 从时间戳转换本地时间
        /// </summary>
        /// <param name="milliseconds"></param>
        /// <returns></returns>
        public static DateTime LocalFromUnixTicks(long milliseconds) => ConvertChinaTimeZone(Epoch.AddMilliseconds(milliseconds));
        /// <summary>
        /// 把本地或者本质UTC时段转成unix时间戳
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long ToUtcUnixTicks(DateTime dateTime) => (long)dateTime.ToUniversalTime().Subtract(Epoch).TotalMilliseconds;
        /// <summary>
        /// 从现在开始与传入的时间计算出时间戳
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static long UnixTicksFromNow(TimeSpan timeSpan) => (long)DateTime.UtcNow.Add(timeSpan).Subtract(Epoch).TotalMilliseconds;
        /// <summary>
        /// 跟现在时间比较差值
        /// </summary>
        /// <param name="unixTime"></param>
        /// <returns></returns>
        public static TimeSpan CompareUnixNow(long unixTime)
        {
            var now = UnixNow;
            return unixTime > now
                ? TimeSpan.FromMilliseconds(unixTime - now)
                : TimeSpan.FromMilliseconds(now - unixTime);
        }
    }
}