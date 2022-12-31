using System;
using System.Linq;
using System.Text;
using BattleM;
using MyBox;
using Server.Configs._script.Battles;
using UnityEngine;

namespace Server.Configs._script.Skills
{
    [CreateAssetMenu(fileName = "dodgeLevelSo", menuName = "战斗测试/轻功等级配置")]
    internal class DodgeLevelingSo : ScriptableObject,ILeveling<IDodgeSkill>
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

        [ConditionalField(true, nameof(LevelSupport))] [ReadOnly] [SerializeField]
        private string 等级支持;

        [SerializeField] private DodgeFieldSo 轻功;
        [SerializeField] private LevelField[] 等级列;
        [SerializeField] private SkillGrades _grade;

        private LevelField[] Levels => 等级列;
        private DodgeFieldSo DodgeSo => 轻功;

        private SkillGrades Grade => _grade;

        public IDodgeSkill GetFromLevel(int level)
        {
            if (level == 1) return DodgeSo.GetMinLevel();//TODO: 注意这里会照成死循环!, 不可以调用 So.GetFromLevel
            var index = LevelToIndex(level);
            if (index < 0 || index >= Levels.Length)
                throw new IndexOutOfRangeException($"等级[{level}]不支持！等级总长度{Levels.Length + 2}");
            return Levels[index].GetDodge(level);
        }
        private static int LevelToIndex(int level) => level - 2;
        private static int IndexToLevel(int index) => index + 2;
        public int MaxLevel => Levels.Length + 1;//最大等级 = 最后一个索引+2 = 索引长度+1

        public IDodgeSkill GetMaxLevel() => Levels[^1].GetDodge(IndexToLevel(Levels.Length - 1));

        public CombatBuffSoBase[] GetBuffs(int level) =>
            level == 1 ? DodgeSo.GetAllBuffs() : Levels[LevelToIndex(level)].GetBuffs();

        [Serializable] private class LevelField
        {
            private bool SetLevelName()
            {
                _name = Dodge == null ? string.Empty : $"{Dodge.name}【{Dodge.Name}】";
                return true;
            }
            private bool SetSummaryText()
            {
                if (Dodge != null)
                {
                    var sb = new StringBuilder();
                    sb.Append($"息[{GetBreath()}]");
                    sb.Append($"身法[{GetDodgeValue()}]");
                    sb.Append($"内耗[{GetDodgeMp()}]");
                    _summary = sb.ToString();
                }
                else _summary = string.Empty;
                return true;
            }

            [ConditionalField(true, nameof(SetLevelName))] [ReadOnly] [SerializeField] private string _name;
            [ConditionalField(true, nameof(SetSummaryText))][ReadOnly][SerializeField] private string _summary;
            [SerializeField] private DodgeFieldSo _dodge;

            [SerializeField] private SoValueConfig 息;
            [SerializeField] private SoValueConfig 身法值;
            [SerializeField] private SoValueConfig 身法内耗;
            [SerializeField] private CombatBuffSoBase[] _buffs;

            private DodgeFieldSo Dodge => _dodge;

            private SoValueConfig BreathCfg => 息;
            private SoValueConfig DodgeCfg => 身法值;
            private SoValueConfig DodgeMpCfg => 身法内耗;

            private CombatBuffSoBase[] Buffs => _buffs;
            public CombatBuffSoBase[] GetBuffs() => Buffs;

            public IDodgeSkill GetDodge(int level) => new DodgeFieldSo.LevelDodge(Dodge,
                GetBreath(),
                GetDodgeValue(),
                GetDodgeMp(),
                level, GetBuffs());

            private int GetBreath() => BreathCfg.GetValue(Dodge.Breath);
            private int GetDodgeValue() => DodgeCfg.GetValue(Dodge.Dodge);
            private int GetDodgeMp() => DodgeMpCfg.GetValue(Dodge.DodgeMp);
        }
    }
}
