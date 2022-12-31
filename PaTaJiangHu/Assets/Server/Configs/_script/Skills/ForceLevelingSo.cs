using System;
using System.Linq;
using System.Text;
using BattleM;
using MyBox;
using Server.Configs._script.Battles;
using UnityEngine;

namespace Server.Configs._script.Skills
{
    /// <summary>
    /// 内功等级配置
    /// </summary>
    [CreateAssetMenu(fileName = "forceLevelSo", menuName = "战斗测试/内功等级配置")]
    internal class ForceLevelingSo : ScriptableObject,ILeveling<IForceSkill>
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }
        [ConditionalField(true, nameof(ReferenceSo))][ReadOnly][SerializeField] private ScriptableObject _so;
        #endregion
        private bool LevelSupport()
        {
            等级支持 = Levels?.Length == 0 ? 1.ToString() : $"{Levels?.Length + 1}";
            return true;
        }
        [ConditionalField(true,nameof(LevelSupport))][ReadOnly][SerializeField] private string 等级支持;
        [SerializeField] private ForceFieldSo 内功;
        [SerializeField] private LevelField[] 等级列;

        private LevelField[] Levels => 等级列;
        private ForceFieldSo ForceSo => 内功;

        public IForceSkill GetFromLevel(int level)
        {
            if (level == 1) return ForceSo.GetMinLevel();//TODO: 注意这里会照成死循环!, 不可以调用 So.GetFromLevel
            var index = LevelToIndex(level);
            if (index < 0 || index >= Levels.Length)
                throw new IndexOutOfRangeException($"等级[{level}]不支持！等级总长度{Levels.Length + 2}");
            return Levels[index].GetForce(level);
        }

        private static int LevelToIndex(int level) => level - 2;
        private static int IndexToLevel(int index) => index + 2;

        public int MaxLevel => Levels.Length + 1;//最大等级 = 最后一个索引+2 = 索引长度+1

        public IForceSkill GetMaxLevel() => Levels[^1].GetForce(IndexToLevel(Levels.Length - 1));

        public CombatBuffSoBase[] GetBuffs(int level) =>
            level == 1 ? ForceSo.GetAllBuffs() : Levels[LevelToIndex(level)].GetBuffs();

        [Serializable] private class LevelField
        {
            private bool SetSummaryText()
            {
                if (Force!=null)
                {
                    var sb = new StringBuilder();
                    sb.Append($"息[{GetBreath()}]");
                    sb.Append($"内转[{GetForceRate()}]");
                    sb.Append($"甲[{GetArmor()}]");
                    sb.Append($"甲耗[{GetArmorDepletion()}]");
                    sb.Append($"复[{GetRecover()}]");
                    sb.Append($"蓄转[{GetMpCharge()}]");
                    _summary = sb.ToString();
                }else _summary = string.Empty;
                return true;
            }
            private bool SetLevelName()
            {
                _name = Force == null ? string.Empty : $"{Force.name}【{Force.Name}】";
                return true;
            }

            [ConditionalField(true,nameof(SetLevelName))][ReadOnly][SerializeField] private string _name;
            [ConditionalField(true,nameof(SetSummaryText))][ReadOnly][SerializeField] private string _summary;
            [SerializeField] private ForceFieldSo 内功;
            [SerializeField] private SoValueConfig 息;
            [SerializeField] private SoValueConfig 内功转化;
            [SerializeField] private SoValueConfig 护甲;
            [SerializeField] private SoValueConfig 护甲内耗;
            [SerializeField] private SoValueConfig 恢复值;
            [SerializeField] private SoValueConfig 蓄转内;
            [SerializeField] private CombatBuffSoBase[] _buffs;

            private ForceFieldSo Force => 内功;
            private SoValueConfig BreathCfg => 息;
            private SoValueConfig ForceRateCfg => 内功转化;
            private SoValueConfig ArmorCfg => 护甲;
            private SoValueConfig ArmorCostCfg => 护甲内耗;
            private SoValueConfig RecoverCfg => 恢复值;
            private SoValueConfig MpChargeCfg => 蓄转内;

            private CombatBuffSoBase[] Buffs => _buffs;
            public CombatBuffSoBase[] GetBuffs() => Force.GetAllBuffs().Concat(Buffs).ToArray();

            public IForceSkill GetForce(int level) => new ForceFieldSo.LevelForce(Force, 
                GetBreath(), 
                GetForceRate(), 
                GetArmor(),
                GetArmorDepletion(), 
                GetRecover(), 
                GetMpCharge(),
                level,
                Buffs);

            private int GetBreath() => BreathCfg.GetValue(Force.Breath);
            private int GetForceRate() => ForceRateCfg.GetValue(Force.ForceRate);
            private int GetArmor() => ArmorCfg.GetValue(Force.Armor);
            private int GetArmorDepletion() => ArmorCostCfg.GetValue(Force.ArmorCost);
            private int GetRecover() => RecoverCfg.GetValue(Force.Recover);
            private int GetMpCharge() => MpChargeCfg.GetValue(Force.MpCharge);
        }
    }
}