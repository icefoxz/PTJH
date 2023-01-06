using UnityEngine;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "AdventureCfg", menuName = "历练/配置")]
    internal class AdventureConfigSo : ScriptableObject
    {
        [Header("一里多少分钟?")]
        [SerializeField] private int 分钟转化里 = 1;
        [SerializeField] private int 事件触发秒数 = 1;
        [SerializeField] private AdventureMapSo 历练地图;
        internal AdventureMapSo AdvMap => 历练地图;
        public int EventLogSecs => 事件触发秒数;
        public int MinuteInMile => 分钟转化里;
    }
}