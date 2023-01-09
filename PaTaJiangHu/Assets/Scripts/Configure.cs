using System;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Factions;
using UnityEngine;

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
        internal GradeConfigSo GradeCfg => 资质配置;
        internal RecruitConfigSo RecruitCfg => 招募配置;
    }

    //弟子配置
    [SerializeField] private Dizi 弟子配置;
    public Dizi DiziCfg => 弟子配置;
    [Serializable] internal class Dizi
    {
        [SerializeField] private Stamina 弟子体力配置;
        internal Stamina StaminaCfg => 弟子体力配置;
    }

    //历练配置
    [SerializeField] private Adventure 历练配置;
    public Adventure AdvCfg => 历练配置;
    [Serializable] internal class Adventure
    {
        [SerializeField] private AdventureConfigSo 历练配置;
        [SerializeField] private BattleSimulatorConfigSo 战斗模拟器;
        [SerializeField] private ConditionPropertySo 状态属性系数配置;
        internal BattleSimulatorConfigSo BattleSimulation => 战斗模拟器;
        internal ConditionPropertySo ConditionProperty => 状态属性系数配置;
        internal AdventureConfigSo AdventureCfg => 历练配置;
    }
}