using System;
using UnityEngine;
using Utls;

namespace Server.Configs._script.Characters
{
    [CreateAssetMenu(fileName = "StaminaConvert", menuName = "配置/体力产出")]
    public class StaminaGenerateSo : ScriptableObject
    {
        [Header("1体力的产出时间")] [SerializeField] private int 分钟;
        [SerializeField] private int 小时;

        private int Minutes => 分钟;
        private int Hours => 小时;

        public int GetStamina(long lastTick)
        {
            var intervalTick = SysTime.UnixNow - lastTick;
            var intervalTimeSpan = SysTime.TickFromMilliseconds(intervalTick);
            var perStamina = PerStamina;
            var stamina = (int)(intervalTimeSpan / perStamina);
            return stamina;
        }
        public long PerStaminaTicks => (long)PerStamina.TotalMilliseconds;
        public TimeSpan PerStamina
            => TimeSpan.FromHours(Hours) + TimeSpan.FromMinutes(Minutes);
        //{
            //get
            //{
            //    if( _perStamina == default)
            //        _perStamina = TimeSpan.FromHours(Hours) + TimeSpan.FromMinutes(Minutes);
            //    return _perStamina;
            //}
        //}

        public long NextGen(long lastTicks) => lastTicks + (long)PerStamina.TotalMilliseconds;
        public TimeSpan CountdownTotalSecs(long lastTicks) =>
            TimeSpan.FromMilliseconds(NextGen(lastTicks) - SysTime.UnixNow);
    }
}