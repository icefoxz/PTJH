using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    [CreateAssetMenu(fileName = "AdventureCfg", menuName = "状态玩法/历练/配置")]
    internal class AdventureConfigSo : ScriptableObject
    {
        [Header("一里多少分钟?")]
        [SerializeField] private int 分钟转化里 = 1;
        [SerializeField] private int 事件触发秒数 = 1;
        [SerializeField] private int 回程百分比;
        [SerializeField] private AdventureMapSo[] 历练地图;
        [SerializeField] private AdventureMapSo[] 生产地图;

        internal AdventureMapSo[] AdvMaps => 历练地图;
        internal AdventureMapSo[] ProductionMaps => 生产地图;
        public int EventLogSecs => 事件触发秒数;
        public int MinuteInMile => 分钟转化里;
        //根据总里数的回城百分比决定回程时间
        public int ReturnPercentage => 回程百分比;

        public double GetSeconds(int mile)=> MinuteInMile * 60 * mile;
    }
}