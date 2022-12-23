using System;
using MyBox;
using UnityEngine;

namespace _GameClient.Models
{
    [CreateAssetMenu(fileName = "状态属性配置", menuName = "配置/简易战斗/状态属性配置")]
    internal class ConditionPropertySo : ScriptableObject
    {
        private bool AutoCount()
        {
            力量最大值 = (float)Math.Round(MaxCoefficient * StrengthRatio * 0.01f);
            敏捷最大值 = (float)Math.Round(MaxCoefficient * AgilityRatio * 0.01f);
            装备最大值 = (float)Math.Round(MaxCoefficient * ArmorRatio * 0.01f);
            武器最大值 = (float)Math.Round(MaxCoefficient * WeaponRatio * 0.01f);
            内功最大值 = (float)Math.Round(MaxCoefficient * ForceRatio * 0.01f);
            武功最大值 = (float)Math.Round(MaxCoefficient * CombatRatio * 0.01f);
            轻功最大值 = (float)Math.Round(MaxCoefficient * DodgeRatio * 0.01f);
            return true;
        }

        private enum Coefficients
        {
            Strength,
            Agility,
            Weapon,
            Armor
        }

        private enum Skills
        {
            Combat,
            Force,
            Dodge
        }

        [ConditionalField(true,nameof(AutoCount))][SerializeField] private int 最大系数值 = 1400;
        [SerializeField] private SkillsCoefficientSo 武功系数配置;
        [SerializeField] private SkillsCoefficientSo 内功系数配置;
        [SerializeField] private SkillsCoefficientSo 轻功系数配置;
        [SerializeField] [ReadOnly] private float 力量最大值;
        [SerializeField] [ReadOnly] private float 敏捷最大值;
        [SerializeField] [ReadOnly] private float 装备最大值;
        [SerializeField] [ReadOnly] private float 武器最大值;
        [SerializeField] [ReadOnly] private float 内功最大值;
        [SerializeField] [ReadOnly] private float 武功最大值;
        [SerializeField] [ReadOnly] private float 轻功最大值;
        [SerializeField] [ReadOnly] private float 力量占比 = 14.286f;
        [SerializeField] [ReadOnly] private float 敏捷占比 = 14.286f;
        [SerializeField] [ReadOnly] private float 防具占比 = 7.143f;
        [SerializeField] [ReadOnly] private float 武器占比 = 7.143f;
        [SerializeField] [ReadOnly] private float 内功占比 = 10.714f;
        [SerializeField] [ReadOnly] private float 武功占比 = 35.714f;
        [SerializeField] [ReadOnly] private float 轻功占比 = 10.714f;

        private SkillsCoefficientSo CombatSkill => 武功系数配置;
        private SkillsCoefficientSo ForceSkill => 内功系数配置;
        private SkillsCoefficientSo DodgeSkill => 轻功系数配置;

        private int MaxCoefficient => 最大系数值;
        private float MaxStrength => (float)Math.Round(力量最大值);
        private float MaxAgility => (float)Math.Round(敏捷最大值);
        private float MaxArmor => (float)Math.Round(装备最大值);
        private float MaxWeapon => (float)Math.Round(武器最大值);
        private float MaxForce => (float)Math.Round(内功最大值);
        private float MaxCombat => (float)Math.Round(武功最大值);
        private float MaxDodge => (float)Math.Round(轻功最大值);

        private float StrengthRatio => 力量占比;
        private float AgilityRatio => 敏捷占比;
        private float ArmorRatio => 防具占比;
        private float WeaponRatio => 武器占比;
        private float ForceRatio => 内功占比;
        private float CombatRatio => 武功占比;
        private float DodgeRatio => 轻功占比;

        public ISimulation GetSimulation(int strength, int agility,
            int weapon, int armor,
            SkillsCoefficientSo.Grades combatGrade, int combatLevel,
            SkillsCoefficientSo.Grades forceGrade, int forceLevel,
            SkillsCoefficientSo.Grades dodgeGrade, int dodgeLevel)
        {
            var str = GetCoefficient(strength, Coefficients.Strength);
            var arm = GetCoefficient(armor, Coefficients.Armor);
            var agi = GetCoefficient(agility, Coefficients.Agility);
            var wea = GetCoefficient(weapon, Coefficients.Weapon);
            var comOff = GetOffendCoefficient(Skills.Combat, combatLevel, combatGrade);
            var comDef = GetOffendCoefficient(Skills.Combat, combatLevel, combatGrade);
            var foc = GetCoefficient(Skills.Force, forceLevel, forceGrade);
            var dod = GetCoefficient(Skills.Dodge, dodgeLevel, dodgeGrade);
            var off = (str * 0.5f) + wea + comOff;
            var def = (str * 0.5f) + arm + agi + foc + dod + comDef;
            return new Simulation((int)Math.Round(off), (int)Math.Round(def),
                str, agi, wea, arm, comOff + comDef, foc, dod);
        }

        private float GetCoefficient(int value, Coefficients coe)
        {
            return coe switch
            {
                Coefficients.Strength => CountRate(value, MaxStrength, StrengthRatio),
                Coefficients.Agility => CountRate(value, MaxAgility, AgilityRatio),
                Coefficients.Weapon => CountRate(value, MaxWeapon, WeaponRatio),
                Coefficients.Armor => CountRate(value, MaxArmor, ArmorRatio),
                _ => throw new ArgumentOutOfRangeException(nameof(coe), coe, null)
            };
        }

        private float GetCoefficient(Skills skill, int level, SkillsCoefficientSo.Grades grade)
        {
            return skill switch
            {
                Skills.Combat => CountRate(CombatSkill.GetCoefficient(level, grade) , MaxCombat , CombatRatio),
                Skills.Force => CountRate(ForceSkill.GetCoefficient(level, grade) , MaxForce , ForceRatio),
                Skills.Dodge => CountRate(DodgeSkill.GetCoefficient(level, grade), MaxDodge, DodgeRatio),
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
        }

        private float CountRate(int value, float max, float ratio)
        {
            if (value <= 0) return 0;
            return value / max * ratio;
        }

        private float GetOffendCoefficient(Skills skill, int level, SkillsCoefficientSo.Grades grade)
        {
            return skill switch
            {
                Skills.Combat => CountRate(CombatSkill.GetOffendCoefficient(level, grade), MaxCombat, CombatRatio),
                Skills.Force => CountRate(ForceSkill.GetOffendCoefficient(level, grade), MaxForce, ForceRatio),
                Skills.Dodge => CountRate(DodgeSkill.GetOffendCoefficient(level, grade), MaxDodge, DodgeRatio),
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
        }
        private float GetDefendCoefficient(Skills skill, int level, SkillsCoefficientSo.Grades grade)
        {
            return skill switch
            {
                Skills.Combat => CountRate(CombatSkill.GetDefendCoefficient(level, grade), MaxCombat, CombatRatio),
                Skills.Force => CountRate(ForceSkill.GetDefendCoefficient(level, grade), MaxForce, ForceRatio),
                Skills.Dodge => CountRate(DodgeSkill.GetDefendCoefficient(level, grade), MaxDodge, DodgeRatio),
                _ => throw new ArgumentOutOfRangeException(nameof(skill), skill, null)
            };
        }

        private record Simulation : ISimulation
        {
            public float Offend { get; }
            public float Defend { get; }
            public float Strength { get; }
            public float Agility { get; }
            public float Weapon { get; }
            public float Armor { get; }
            public float Combat { get; }
            public float Force { get; }
            public float Dodge { get; }

            public Simulation(float offend, float defend, float strength, float agility, float weapon, float armor, float combat, float force, float dodge)
            {
                Offend = offend;
                Defend = defend;
                Strength = strength;
                Agility = agility;
                Weapon = weapon;
                Armor = armor;
                Combat = combat;
                Force = force;
                Dodge = dodge;
            }
        }
    }
}