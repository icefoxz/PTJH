using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        int Id { get; }
        string Name { get; }
        SkillType SkillType { get; }
        ColorGrade Grade { get; }
        Sprite Icon { get; }
        string About { get; }
        int MaxLevel();
        ICombatSet GetCombatSet(int level);
        ISkillAttribute[] GetAttributes(int level);
        ISkillProp[] GetProps(int level);
        IBook Book { get; }
    }

    public interface ISkillProp
    {
        string Name { get; }
        string Value { get; }
    }
    /// <summary>
    /// 技能属性
    /// </summary>
    public interface ISkillAttribute
    {
        string Name { get; }
        string Intro { get; }
    }

    public enum SkillType
    {
        [InspectorName("武功")]Combat,
        [InspectorName("内功")]Force,
        [InspectorName("轻功")]Dodge
    }
     
    [CreateAssetMenu(fileName = "combatSo", menuName = "战斗/武学/武功")]
    public class CombatFieldSo : SkillFieldSo ,ICombatSkill
    {
        [SerializeField] private WeaponArmed 类型;
        public WeaponArmed Armed => 类型;
        public override SkillType SkillType => SkillType.Combat;
    }

    public abstract class SkillFieldSo : AutoDashNamingObject, ISkill
    {
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private Sprite 图标;
        [SerializeField] private BookSoBase 秘籍;
        [SerializeField] private SkillLevelStrategySo 等级策略;
        [SerializeField] [TextArea] private string 描述;
        public abstract SkillType SkillType { get; }
        public ColorGrade Grade => 品级;
        public Sprite Icon => 图标;
        public string About => 描述;
        public IBook Book => 秘籍;

        private SkillLevelStrategySo LevelStrategy => 等级策略;

        protected virtual IList<ICombatSet> CustomCombatSets()
        {
            CheckThis();
            return Array.Empty<ICombatSet>();
        }

        public ICombatSet GetCombatSet(int level)
        {
            CheckThis();
            var list = CustomCombatSets().ToList();
            list.Add(LevelStrategy.GetCombatSet(level));
            return list.Combine();
        }

        public ISkillAttribute[] GetAttributes(int level)
        {
            CheckThis();
            return LevelStrategy.GetAttributes(level);
        }

        public ISkillProp[] GetProps(int level)
        {
            CheckThis();
            return LevelStrategy.GetProps(level);
        }

        public int MaxLevel()
        {
            CheckThis();
            return LevelStrategy.MaxLevel();
        }

        private void CheckThis([CallerMemberName] string methodName = null)
        {
            if (this == null) throw new NullReferenceException($"技能异常!{methodName}方法调用了null的技能!");
        }
    }
}