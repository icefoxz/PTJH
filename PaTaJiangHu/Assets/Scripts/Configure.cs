using System;
using System.Runtime.CompilerServices;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Factions;
using Server.Configs.Items;
using UnityEngine;
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
    //全局配置
    [SerializeField] private GlobalCfg 全局配置;
    public GlobalCfg Global => 全局配置;
    [Serializable] public class GlobalCfg
    {
        [SerializeField] private GradeColorSo 品阶颜色;
        public GradeColorSo GradeColor => 品阶颜色;
    }

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
    [SerializeField] private DiziConfig 弟子配置;
    public DiziConfig DiziCfg => 弟子配置;
    [Serializable] internal class DiziConfig
    {
        [SerializeField] private StaminaConfigSo 体力配置;
        [SerializeField] private LevelConfigSo 升级配置;
        [SerializeField] private PropStateConfigSo 属性状态配置;
        internal StaminaConfigSo StaminaCfg => GetSo(体力配置);
        internal LevelConfigSo LevelConfigSo => GetSo(升级配置);
        internal PropStateConfigSo PropState => GetSo(属性状态配置);
    }

    //弟子闲置配置
    [SerializeField] private DiziIdle 弟子闲置配置;
    public DiziIdle Idle => 弟子闲置配置;
    [Serializable]internal class DiziIdle
    {
        [SerializeField]private IdleMapSo 闲置映像;
        [SerializeField]private int 信息更新秒数 = 5;
        internal IdleMapSo IdleMapSo => 闲置映像;
        internal int MessageUpdateSecs => 信息更新秒数;
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


    //[SerializeField] private FactionConfig 门派配置;
    //public FactionConfig FactionCfg => 门派配置;
    //[Serializable] internal class FactionConfig
    //{
    //}

    //数据配置
    [SerializeField] private DataCfg 游戏数据;
    public DataCfg Data => 游戏数据;
    [Serializable] public class DataCfg
    {
        [SerializeField] private WeaponFieldSo[] 武器;
        [SerializeField] private ArmorFieldSo[] 防具;
        [SerializeField] private MedicineFieldSo[] 药品;
        [SerializeField] private BookFieldSo[] 书籍;
        [SerializeField] private AdvItemFieldSo[] 历练道具;

        public WeaponFieldSo[] Weapons => 武器;
        public ArmorFieldSo[] Armors => 防具;
        public MedicineFieldSo[] Medicines => 药品;
        public BookFieldSo[] Books => 书籍;
        public AdvItemFieldSo[] AdvItems => 历练道具;
    }

    private static T GetSo<T>(T so, [CallerMemberName]string method = null) where T : ScriptableObject
    {
        if(so == null)
            XDebug.LogError($"{nameof(Configure)}.{method}.{nameof(T)} 未配置! 请先完成配置文件!");
        return so;
    }
}