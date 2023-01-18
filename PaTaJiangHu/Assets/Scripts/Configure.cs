using System;
using System.Runtime.CompilerServices;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Factions;
using UnityEngine;
using UnityEngine.Serialization;
using Utls;

/// <summary>
/// 游戏配置
/// </summary>
internal class Configure : MonoBehaviour
{
    [SerializeField] private ConfigureSo 游戏配置;

    public Config Config => 游戏配置.Config;
}

[Serializable] internal class Config
{
    //招募配置
    [SerializeField] private Recruit 招募配置;
    public Recruit RecruitCfg => 招募配置;
    [Serializable] internal class Recruit
    {
        [SerializeField] private GradeConfigSo 资质配置;
        [SerializeField] private RecruitConfigSo 招募配置;
        internal GradeConfigSo GradeCfg => GetSo(资质配置);
        internal RecruitConfigSo RecruitCfg => GetSo(招募配置);
    }

    //弟子配置
    [SerializeField] private Dizi 弟子配置;
    public Dizi DiziCfg => 弟子配置;
    [Serializable] internal class Dizi
    {
        [SerializeField] private StaminaConfigSo 体力配置;
        [SerializeField] private LevelConfigSo 升级配置;
        [SerializeField] private PropStateConfigSo 属性状态配置;
        internal StaminaConfigSo StaminaCfg => GetSo(体力配置);
        internal LevelConfigSo LevelConfigSo => GetSo(升级配置);
        internal PropStateConfigSo PropState => GetSo(属性状态配置);
    }

    //历练配置
    [SerializeField] private Adventure 历练配置;
    public Adventure AdvCfg => 历练配置;
    [Serializable] internal class Adventure
    {
        [SerializeField] private AdventureConfigSo 历练配置;
        [SerializeField] private BattleSimulatorConfigSo 战斗模拟器;
        [SerializeField] private ConditionPropertySo 状态属性系数配置;
        internal BattleSimulatorConfigSo BattleSimulation => GetSo(战斗模拟器);
        internal ConditionPropertySo ConditionProperty => GetSo(状态属性系数配置);
        internal AdventureConfigSo AdventureCfg => GetSo(历练配置);
    }

    private static T GetSo<T>(T so, [CallerMemberName]string method = null) where T : ScriptableObject
    {
        if(so == null)
            XDebug.LogError($"{nameof(Configure)}.{method}.{nameof(T)} 未配置! 请先完成配置文件!");
        return so;
    }
}