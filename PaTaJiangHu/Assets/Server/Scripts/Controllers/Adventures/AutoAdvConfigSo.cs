using UnityEngine;

namespace Server.Controllers.Adventures
{
    [CreateAssetMenu(fileName = "AutoMapCfg", menuName = "历练/配置")]
    internal class AutoAdvConfigSo : ScriptableObject
    {
        [Header("一里多少分钟?")]
        [SerializeField] private int 分钟转化里 = 1;
        [SerializeField] private readonly int 事件触发秒数 = 1;

        public int EventSecs => 事件触发秒数;
        public int MinuteInMile => 分钟转化里;
    }
}