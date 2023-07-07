using UnityEngine;

namespace GameClient.SoScripts
{
    [CreateAssetMenu(fileName = "ConfigureSo", menuName = "游戏配置/主配置")]
    public class ConfigSo : ScriptableObject
    {
        [SerializeField] private Config 游戏配置;

        public Config Config => 游戏配置;
    }
}