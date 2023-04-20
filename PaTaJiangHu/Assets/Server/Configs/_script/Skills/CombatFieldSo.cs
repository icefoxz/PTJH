using MyBox;
using Server.Configs.Battles;
using Server.Configs.Items;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Skills
{
    public interface ICombatSkill : ISkill
    {
        WeaponArmed Armed { get; }
    }
    public interface ISkill
    {
        SkillType SkillType { get; }
        ColorGrade Grade { get; }
        Sprite Icon { get; }
        string About { get; }
        ICombatSet GetCombatSet(int level);
        int MaxLevel();
    }

    public enum SkillType
    {
        [InspectorName("武功")]Combat,
        [InspectorName("内功")]Force,
        [InspectorName("轻功")]Dodge
    }
     
    [CreateAssetMenu(fileName = "combatSo", menuName = "战斗/武学/武功")]
    internal class CombatFieldSo : SkillFieldSo ,ICombatSkill
    {
        [SerializeField] private WeaponArmed 类型;
        public WeaponArmed Armed => 类型;
        public override SkillType SkillType => SkillType.Combat;
    }

    internal abstract class SkillFieldSo : AutoDashNamingObject,ISkill
    {
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private Sprite 图标;
        [SerializeField] private SkillLevelStrategySo 等级策略;
        [SerializeField][TextArea] private string 描述;
        public abstract SkillType SkillType { get; }
        public ColorGrade Grade => 品级;
        public Sprite Icon => 图标;
        public string About => 描述;
        private SkillLevelStrategySo LevelStrategy => 等级策略;
        public ICombatSet GetCombatSet(int level) => LevelStrategy.GetCombatSet(level - 1);
        public int MaxLevel() => LevelStrategy.MaxLevel();
    }

}