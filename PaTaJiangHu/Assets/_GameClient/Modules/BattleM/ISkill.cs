using AOT.Core;
using AOT.Core.Dizi;
using GameClient.Modules.DiziM;
using UnityEngine;

namespace GameClient.Modules.BattleM
{
    public interface ISkillLevelConfig
    {
        int Level { get; }
        float Rate { get; }
        int MinCost { get; }
        int BookCost { get; }
    }
    public interface IBook : IGameItem
    {
        ColorGrade Grade { get; }
        ISkillLevelConfig GetLevelMap(int nextLevel);
        ISkill GetSkill();
    }
// 技能等级映射
    public interface ISkillMap<out T> where T : ISkillInfo
    {
        int Level { get; }
        T Skill { get; }
    }

    public interface ICombatSkill : ISkill, ICombatSkillInfo
    {
    }

    public interface ICombatSkillInfo : ISkillInfo
    {
        WeaponArmed Armed { get; }
    }

    public interface ISkillInfo
    {
        int Id { get; }
        string Name { get; }
        SkillType SkillType { get; }
        ColorGrade Grade { get; }
        Sprite Icon { get; }
        string About { get; }
        int MaxLevel { get; }
    }

    public interface ISkill : ISkillInfo
    {
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
        [InspectorName("武功")] Combat,
        [InspectorName("内功")] Force,
        [InspectorName("轻功")] Dodge
    }
}