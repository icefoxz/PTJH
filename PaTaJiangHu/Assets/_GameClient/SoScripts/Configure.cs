using System;
using System.Runtime.CompilerServices;
using AOT._AOT.Utls;
using GameClient.GameScene.Background;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;
using GameClient.SoScripts.ChallengeStages;
using GameClient.SoScripts.Characters;
using GameClient.SoScripts.Factions;
using GameClient.SoScripts.GameVisualize;
using GameClient.SoScripts.Skills;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameClient.SoScripts
{
    /// <summary>
    /// 游戏配置
    /// </summary>
    internal class Configure : MonoBehaviour
    {
        [SerializeField] private ConfigSo 游戏配置;

        public Config Config => 游戏配置.Config;
    }

    [Serializable] public class Config
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
        [Serializable] public class Recruit
        {
            [SerializeField] private GradeConfigSo 资质配置;
            [SerializeField] private RecruitConfigSo 招募配置;
            internal GradeConfigSo GradeCfg => GetSo(资质配置);
            internal RecruitConfigSo RecruitCfg => GetSo(招募配置);
        }

        [SerializeField] private BattleConfig 战斗配置;
        public BattleConfig BattleCfg => 战斗配置;
        [Serializable]public class BattleConfig
        {
            [SerializeField] private RestraintCfgSo 武功系数克制配置;
            public RestraintCfgSo Restraint => 武功系数克制配置;
        }

        //弟子配置
        [SerializeField] private DiziConfig 弟子配置;
        public DiziConfig DiziCfg => 弟子配置;
        [Serializable] public class DiziConfig
        {
            [SerializeField] private StaminaConfigSo 体力配置;
            [SerializeField] private LevelConfigSo 升级配置;
            [SerializeField] private PropStateConfigSo 属性状态配置;
            [SerializeField] private SkillConfigSo 技能配置;
            [SerializeField] private DiziBattleConfigSo 战斗配置;
            internal StaminaConfigSo StaminaCfg => GetSo(体力配置);
            internal LevelConfigSo LevelConfigSo => GetSo(升级配置);
            internal PropStateConfigSo PropState => GetSo(属性状态配置);
            internal SkillConfigSo SkillCfg => GetSo(技能配置);
            internal DiziBattleConfigSo DiziBattleCfg => GetSo(战斗配置);
        }

        //弟子闲置配置
        [SerializeField] private DiziIdle 弟子闲置配置;
        public DiziIdle Idle => 弟子闲置配置;
        [Serializable]public class DiziIdle
        {
            [SerializeField]private IdleMapSo 闲置映像;
            [SerializeField]private int 信息更新秒数 = 5;
            internal IdleMapSo IdleMapSo => 闲置映像;
            internal int MessageUpdateSecs => 信息更新秒数;
        }

        //历练配置
        [SerializeField] private Adventure 历练配置;
        public Adventure AdvCfg => 历练配置;
        [Serializable] public class Adventure
        {
            [SerializeField] private AdventureConfigSo 历练配置;
            [SerializeField] private BattleSimulatorConfigSo 战斗模拟器;
            internal BattleSimulatorConfigSo BattleSimulation => GetSo(战斗模拟器);
            internal AdventureConfigSo AdventureCfg => GetSo(历练配置);
        }

        [FormerlySerializedAs("挑战关卡配置")][SerializeField] private GameStageCfg 关卡配置;
        internal GameStageCfg StageCfg => 关卡配置;

        [Serializable]internal class GameStageCfg
        {
            [FormerlySerializedAs("挑战")][SerializeField] private GameStageSo[] 关卡;
            internal GameStageSo[] Stages => 关卡;
        }

        [SerializeField] private ChallengeCfgSo 挑战关卡配置;
        internal ChallengeCfgSo ChallengeCfg => 挑战关卡配置;

        //[SerializeField] private FactionConfig 门派配置;
        //public FactionConfig FactionCfg => 门派配置;
        //[Serializable] internal class FactionConfig
        //{
        //}

        //数据配置
        [SerializeField] private DataConfigSo 游戏数据;
        public DataConfigSo Data => 游戏数据;
        //[Serializable] public class DataCfg 
        //{
        //    [SerializeField] private WeaponFieldSo[] 武器;
        //    [SerializeField] private ArmorFieldSo[] 防具;
        //    [SerializeField] private ShoesFieldSo[] 鞋子;
        //    [SerializeField] private DecorationFieldSo[] 挂件;
        //    [SerializeField] private MedicineFieldSo[] 药品;
        //    [SerializeField] private BookFieldSo[] 书籍;
        //    [SerializeField] private AdvItemFieldSo[] 历练道具;
        //    [SerializeField] private ComprehendItemSo[] 功能道具;

        //    public WeaponFieldSo[] Weapons => 武器;
        //    public ArmorFieldSo[] Armors => 防具;
        //    public ShoesFieldSo[] Shoes => 鞋子;
        //    public DecorationFieldSo[] Decorations => 挂件;
        //    public MedicineFieldSo[] Medicines => 药品;
        //    public BookFieldSo[] Books => 书籍;
        //    public AdvItemFieldSo[] AdvItems => 历练道具;

        //    public ComprehendItemSo[] FunctionItems => 功能道具;
        //}

        [SerializeField] private GameAnimConfig 游戏演示配置;
        internal GameAnimConfig GameAnimCfg => 游戏演示配置;
        //游戏演示配置
        [Serializable]
        public class GameAnimConfig
        {
            [SerializeField] private CharacterOperator 角色预设;
            internal CharacterOperator CharacterOpPrefab => 角色预设;
            [SerializeField] private DiziCombatVisualConfigSo 弟子战斗配置;
            internal DiziCombatVisualConfigSo DiziCombatVisualCfg => 弟子战斗配置;

            public CharacterOperator InstanceCharacterOp(Transform parent) => UnityEngine.Object.Instantiate(CharacterOpPrefab, parent);
        }

        private static T GetSo<T>(T so, [CallerMemberName]string method = null) where T : ScriptableObject
        {
            if(so == null)
                XDebug.LogError($"{nameof(Configure)}.{method}.{nameof(T)} 未配置! 请先完成配置文件!");
            return so;
        }
    }
}