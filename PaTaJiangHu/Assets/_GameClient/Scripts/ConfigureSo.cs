using UnityEngine;

[CreateAssetMenu(fileName = "ConfigureSo", menuName = "游戏配置/主配置")]
internal class ConfigureSo : ScriptableObject
{
    [SerializeField] private Config 游戏配置;

    public Config Config => 游戏配置;
}