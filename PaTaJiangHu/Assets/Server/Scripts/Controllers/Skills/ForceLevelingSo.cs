using System;
using System.Linq;
using System.Text;
using BattleM;
using MyBox;
using So;
using UnityEngine;

namespace Server.Controllers.Skills
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
        [SerializeField] private Level[] 等级列;
        private Level[] Levels => 等级列;
        private ForceFieldSo ForceSo => 内功;
        public IForceSkill GetFromLevel(int level)
        {
            if (level == 1) return ForceSo;
            var index = IndexAlign(level);
            if (index < 0 || index >= Levels.Length)
                throw new IndexOutOfRangeException($"等级[{level}]不支持！等级总长度{Levels.Length + 2}");
            return Levels[index].GetForce();
        }

        private static int IndexAlign(int level) => level - 2;
        public int MaxLevel => Levels.Length + 1;//最大等级 = 最后一个索引+2 = 索引长度+1

        public IForceSkill GetMaxLevel() => Levels[^1].GetForce();

        public CombatBuffSoBase[] GetBuffs(int level) =>
            level == 1 ? ForceSo.GetAllBuffs() : Levels[IndexAlign(level)].GetBuffs();

        [Serializable] private class Level
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

            public IForceSkill GetForce() => new LevelForce(Force, 
                GetBreath(), 
                GetForceRate(), 
                GetArmor(),
                GetArmorDepletion(), 
                GetRecover(), 
                GetMpCharge(),
                Buffs);

            private int GetBreath() => BreathCfg.GetValue(Force.Breath);
            private int GetForceRate() => ForceRateCfg.GetValue(Force.ForceRate);
            private int GetArmor() => ArmorCfg.GetValue(Force.Armor);
            private int GetArmorDepletion() => ArmorCostCfg.GetValue(Force.ArmorCost);
            private int GetRecover() => RecoverCfg.GetValue(Force.Recover);
            private int GetMpCharge() => MpChargeCfg.GetValue(Force.MpCharge);

            private record LevelForce: IForceSkill
            {
                private IForceSkill Force { get; }
                private CombatBuffSoBase[] AddOnBuffs { get; }
                public LevelForce(IForceSkill force, int breath, int forceRate, int armor, int armorDepletion,
                    int recover, int mpConvert, CombatBuffSoBase[] addOnBuffs)
                {
                    Force = force;
                    Breath = breath;
                    ForceRate = forceRate;
                    Armor = armor;
                    ArmorCost = armorDepletion;
                    Recover = recover;
                    MpCharge = mpConvert;
                    AddOnBuffs = addOnBuffs;
                }

                public int Breath { get; }
                public int ForceRate { get; }
                public int Armor { get; }
                public int ArmorCost { get; }
                public int Recover { get; }
                public int MpCharge { get; }

                public string Name => Force.Name;

                public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => Force
                    .GetBuffs(unit, append).Concat(AddOnBuffs.GetSortedInstance(append, unit)).ToArray();
            }
        }
    }
}