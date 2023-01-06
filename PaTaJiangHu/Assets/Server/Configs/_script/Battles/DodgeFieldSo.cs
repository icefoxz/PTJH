using System.Linq;
using BattleM;
using Data;
using MyBox;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗测试/轻功")]
    internal class DodgeFieldSo : ScriptableObject, IDataElement
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }
        [ConditionalField(true, nameof(ReferenceSo))][ReadOnly][SerializeField] private ScriptableObject _so;
        #endregion
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 内消耗;
        [SerializeField] private int 使用息;
        [SerializeField] private int 身法值;
        [SerializeField] private CombatBuffSoBase[] _buffs;
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private DodgeLevelingSo 轻功等级配置;

        public int Id => id;
        public string Name => _name;
        public int DodgeMp => 内消耗;

        public int Breath => 使用息;
        public int Dodge => 身法值;
        public CombatBuffSoBase[] GetAllBuffs() => _buffs;

        public DodgeLevelingSo DodgeLevelingSo => 轻功等级配置;
        public SkillGrades Grade => _grade;

        public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) =>
            _buffs.GetSortedInstance(append, unit).ToArray();

        public IDodgeSkill GetFromLevel(int level)=> DodgeLevelingSo.GetFromLevel(level);
        public IDodgeSkill GetMaxLevel()=> DodgeLevelingSo.GetMaxLevel();
        public IDodgeSkill GetMinLevel() => new LevelDodge(this, Breath, Dodge, DodgeMp, 1, GetAllBuffs());

        public record LevelDodge : IDodgeSkill
        {
            private DodgeFieldSo DodgeSo { get; }

            private CombatBuffSoBase[] AddOnBuffs { get; }

            public LevelDodge(DodgeFieldSo dodgeSo, int breath, int dodgeValue, int dodgeMp,
                int level, CombatBuffSoBase[] addOnBuffs)
            {
                DodgeSo = dodgeSo;
                Level = level;
                Breath = breath;
                Dodge = dodgeValue;
                DodgeMp = dodgeMp;
                AddOnBuffs = addOnBuffs;
            }

            public string Name => DodgeSo.Name;
            public int Breath { get; }
            public int Dodge { get; }
            public int DodgeMp { get; }
            public SkillGrades Grade => DodgeSo.Grade;
            public int Level { get; }

            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) =>
                DodgeSo.GetBuffs(unit, append).Concat(AddOnBuffs.GetSortedInstance(append, unit)).ToArray();

        }
    }
}