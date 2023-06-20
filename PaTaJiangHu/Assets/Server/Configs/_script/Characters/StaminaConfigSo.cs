using System;
using Models;
using UnityEngine;
using Utls;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "StaminaConvert", menuName = "弟子/体力产出")]
    public class StaminaConfigSo : ScriptableObject
    {
        [Header("1体力的产出时间")] [SerializeField] private int 分钟;
        [SerializeField] private int 小时;

        private int Minutes => 分钟;
        private int Hours => 小时;
        
        /// <summary>
        /// 获取总体力
        /// </summary>
        /// <param name="lastTick"></param>
        /// <returns></returns>
        public int GetStamina(long lastTick)
        {
            var intervalTick = SysTime.UnixNow - lastTick;
            var intervalTimeSpan = SysTime.MillisecondsToTimeSpan(intervalTick);
            var perStamina = PerStamina;
            var stamina = (int)(intervalTimeSpan / perStamina);
            return stamina;
        }
        public long PerStaminaTicks => (long)PerStamina.TotalMilliseconds;
        public TimeSpan PerStamina
            => TimeSpan.FromHours(Hours) + TimeSpan.FromMinutes(Minutes);
        /// <summary>
        /// 算出下一个体力的时间戳
        /// </summary>
        /// <param name="lastTicks"></param>
        /// <returns></returns>
        public long NextGenTicks(long lastTicks) => lastTicks + (long)PerStamina.TotalMilliseconds;
        /// <summary>
        /// 算出现在和下一个体力产出的时间间隔
        /// </summary>
        /// <param name="lastTicks"></param>
        /// <returns></returns>
        public TimeSpan GetNextStaminaTimeInterval(long lastTicks) => TimeSpan.FromMilliseconds(NextGenTicks(lastTicks) - SysTime.UnixNow);

        /// <summary>
        /// 算出下个倒数时差
        /// </summary>
        /// <param name="zeroStaminaTicks"></param>
        /// <returns></returns>
        public TimeSpan CountdownTimeSpan(long zeroStaminaTicks)
        {
            var timeInterval = TimeIntervalFromNow(zeroStaminaTicks);
            var remainSecs = TimeSpan.FromMilliseconds(timeInterval % PerStamina.TotalMilliseconds);
            return PerStamina.Subtract(remainSecs);
        }
        /// <summary>
        /// 算出0体力的时间戳
        /// </summary>
        /// <param name="stamina"></param>
        /// <returns></returns>
        public long GetZeroTicksFromStamina(int stamina)
        {
            var totalTimespan = PerStamina.Multiply(stamina);
            return SysTime.UnixNow - (long)totalTimespan.TotalMilliseconds;
        }

        /// <summary>
        /// 计算体力产出
        /// </summary>
        /// <param name="zeroStaminaTicks"></param>
        /// <param name="maxStamina"></param>
        /// <returns></returns>
        public int CountStamina(long zeroStaminaTicks, int maxStamina = -1)
        {
            var timeInterval = TimeIntervalFromNow(zeroStaminaTicks);
            var totalStamina = (int)TimeSpan.FromMilliseconds(timeInterval).Divide(PerStamina);
            if (maxStamina > 0 && maxStamina < totalStamina) return maxStamina;
            return totalStamina;
        }

        private static long TimeIntervalFromNow(long ticks)
        {
            var now = SysTime.UnixNow;
            var interval = now - ticks;
            if (interval < 0) throw new InvalidOperationException($"时差为负数! {interval}, ticks = {ticks}, now = {now}");
            return interval;
        }
    }
}