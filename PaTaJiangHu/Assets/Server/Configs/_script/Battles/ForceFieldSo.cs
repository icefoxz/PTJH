using System.Linq;
using BattleM;
using Data;
using MyBox;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗测试/内功")]
    internal class ForceFieldSo : ScriptableObject, IDataElement
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
        [SerializeField] private int 息;
        [SerializeField] private int 气血恢复量;
        [SerializeField] private int 内功转化;
        [SerializeField] private int 蓄转内;
        [SerializeField] private int 护甲值;
        [SerializeField] private int 护甲消耗;
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private CombatBuffSoBase[] _buffs;
        [SerializeField] private ForceLevelingSo 等级配置;

        public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) =>
            _buffs.GetSortedInstance(append, unit).ToArray();

        public int Id => id;
        public string Name => _name;
        public int ForceRate => 内功转化;
        public int MpCharge => 蓄转内;
        public int Recover => 气血恢复量;
        public int ArmorCost => 护甲消耗;
        public int Armor => 护甲值;
        public int Breath => 息;
        public SkillGrades Grade => _grade;

        public ForceLevelingSo LevelingSo => 等级配置;
        public CombatBuffSoBase[] GetAllBuffs() => _buffs;

        public IForceSkill GetFromLevel(int level) => LevelingSo.GetFromLevel(level);
        public IForceSkill GetMaxLevel() => LevelingSo.GetMaxLevel();
        public IForceSkill GetMinLevel() => new LevelForce(this, Breath, ForceRate, Armor, ArmorCost, Recover, MpCharge,
            1, GetAllBuffs());

        public record LevelForce : IForceSkill
        {
            private ForceFieldSo ForceSo { get; }
            private CombatBuffSoBase[] AddOnBuffs { get; }
            public LevelForce(ForceFieldSo force, int breath, int forceRate, int armor, int armorDepletion,
                int recover, int mpConvert, int level, CombatBuffSoBase[] addOnBuffs)
            {
                ForceSo = force;
                Breath = breath;
                ForceRate = forceRate;
                Armor = armor;
                ArmorCost = armorDepletion;
                Recover = recover;
                MpCharge = mpConvert;
                AddOnBuffs = addOnBuffs;
                Level = level;
            }

            public int Breath { get; }
            public int ForceRate { get; }
            public int Armor { get; }
            public int ArmorCost { get; }
            public int Recover { get; }
            public int MpCharge { get; }
            public SkillGrades Grade => ForceSo.Grade;
            public int Level { get; }
            public string Name => ForceSo.Name;

            public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) => ForceSo
                .GetBuffs(unit, append).Concat(AddOnBuffs.GetSortedInstance(append, unit)).ToArray();

        }
    }

}