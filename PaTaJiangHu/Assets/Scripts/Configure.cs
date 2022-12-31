using System;
using Server.Configs._script.Characters;
using Server.Configs._script.Factions;
using UnityEngine;

/// <summary>
/// 游戏配置
/// </summary>
internal class Configure : MonoBehaviour
{
    [SerializeField] private RecruitConfigure 招募配置;
    public RecruitConfigure Recruit => 招募配置;

    //招募配置
    [Serializable]
    internal class RecruitConfigure
    {
        [SerializeField] private GradeConfigSo 资质配置;
        [SerializeField] private RecruitConfigSo 招募配置;
        internal GradeConfigSo GradeConfig => 资质配置;
        internal RecruitConfigSo RecruitConfig => 招募配置;
    }
}