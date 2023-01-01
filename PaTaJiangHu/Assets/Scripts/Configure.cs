using System;
using Server.Configs._script.Characters;
using Server.Configs._script.Factions;
using UnityEngine;

/// <summary>
/// 游戏配置
/// </summary>
internal class Configure : MonoBehaviour
{
    //招募配置
    [SerializeField] private RecruitConfigure 招募配置;
    public RecruitConfigure Recruit => 招募配置;
    [Serializable] internal class RecruitConfigure
    {
        [SerializeField] private GradeConfigSo 资质配置;
        [SerializeField] private RecruitConfigSo 招募配置;
        internal GradeConfigSo GradeConfig => 资质配置;
        internal RecruitConfigSo RecruitConfig => 招募配置;
    }

    //弟子配置
    [SerializeField] private DiziConfigure _diziCfg;
    public DiziConfigure DiziCfg => _diziCfg;
    [Serializable] internal class DiziConfigure
    {
        [SerializeField] private StaminaGenerateSo 弟子体力配置;
        internal StaminaGenerateSo StaminaGenerator => 弟子体力配置;
    }
}