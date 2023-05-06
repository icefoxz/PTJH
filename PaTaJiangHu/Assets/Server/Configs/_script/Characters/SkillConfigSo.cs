using System;
using System.Linq;
using Models;
using Server.Configs.Battles;
using Server.Configs.Skills;
using UnityEngine;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "技能配置", menuName = "配置/弟子/技能配置")]
    internal class SkillConfigSo : ScriptableObject
    {
        [SerializeField] private ForceFieldSo 默认内功;
        [SerializeField] private DodgeFieldSo 默认轻功;
        [SerializeField] private CombatFieldSo 默认剑法;
        [SerializeField] private CombatFieldSo 默认刀法;
        [SerializeField] private CombatFieldSo 默认空手;
        [SerializeField] private CombatFieldSo 默认棍法;

        private ForceFieldSo BasicForceSkill => 默认内功;
        private DodgeFieldSo BasicDodgeSkill => 默认轻功;
        private CombatFieldSo BasicSwordSkill => 默认剑法;
        private CombatFieldSo BasicBladeSkill => 默认刀法;
        private CombatFieldSo BasicUnarmedSkill => 默认空手;
        private CombatFieldSo BasicStaffSkill => 默认棍法;

        private ICombatSet GetBasicForceSkill() => BasicForceSkill.GetCombatSet(1);
        private ICombatSet GetBasicDodgeSkill() => BasicDodgeSkill.GetCombatSet(1);
        private ICombatSet GetBasicSwordSkill() => BasicSwordSkill.GetCombatSet(1);
        private ICombatSet GetBasicBladeSkill() => BasicBladeSkill.GetCombatSet(1);
        private ICombatSet GetBasicUnarmedSkill() => BasicUnarmedSkill.GetCombatSet(1);
        private ICombatSet GetBasicStaffSkill() => BasicStaffSkill.GetCombatSet(1);

        public ICombatSet GetCombatSet(Dizi dizi)
        {
            var combat = GetCombatSkillSet(dizi);
            var force = GetForceSkillSet(dizi);
            var dodge = GetDodgeSkillSet(dizi);
            return new[] { combat, force, dodge }.Combine();
        }

        private ICombatSet GetDodgeSkillSet(Dizi dizi)
        {
            var diziSkill = dizi.Skill;
            if (diziSkill.Dodge != null) //如果弟子有轻功
                return diziSkill.Dodge.GetCombatSet(diziSkill.Dodge.Level); //返回当前使用的轻功
            //否则返回最高等的轻功
            var dodgeSkill = diziSkill.DodgeSkills.OrderByDescending(d => d.Level).FirstOrDefault();
            return dodgeSkill != null
                ? dodgeSkill.GetCombatSet(dodgeSkill.Level)
                : GetBasicDodgeSkill(); //如果没有轻功则返回基础轻功
        }

        private ICombatSet GetForceSkillSet(Dizi dizi)
        {
            var diziSkill = dizi.Skill;
            if (diziSkill.Force != null) //如果弟子有内功
                return diziSkill.Force.GetCombatSet(diziSkill.Force.Level); //返回当前使用的内功
            //否则返回最高等的内功
            var forceSkill = diziSkill.ForceSkills.OrderByDescending(f => f.Level).FirstOrDefault();
            return forceSkill != null 
                ? forceSkill.GetCombatSet(forceSkill.Level) 
                : GetBasicForceSkill(); //如果没有内功则返回基础内功
        }

        private ICombatSet GetCombatSkillSet(Dizi dizi)
        {
            var diziSkill = dizi.Skill;
            var diziArmed = dizi.Weapon?.Armed ?? WeaponArmed.Unarmed; //如果弟子没有装备默认为空手
            if (diziSkill.Combat != null && diziSkill.Combat.Skill.Armed == diziArmed) //如果弟子有武功并且武学类型与装备类型相符
                return diziSkill.Combat.GetCombatSet(diziSkill.Combat.Level); //返回当前使用的武功
            var combatSkill = diziSkill.CombatSkills //否则返回与装备相符最高等的武功
                .Where(c => c.Skill.Armed == diziArmed) 
                .OrderByDescending(c => c.Level).FirstOrDefault(); 
            return combatSkill != null 
                ? combatSkill.GetCombatSet(combatSkill.Level) 
                : GetBasicSkill(diziArmed); //如果没有武功则返回基础武功
        }

        private ICombatSet GetBasicSkill(WeaponArmed armed)
        {
            var basic = armed switch
            {
                WeaponArmed.Unarmed => GetBasicUnarmedSkill(),
                WeaponArmed.Sword => GetBasicSwordSkill(),
                WeaponArmed.Blade => GetBasicBladeSkill(),
                WeaponArmed.Staff => GetBasicStaffSkill(),
                _ => throw new ArgumentOutOfRangeException()
            };
            return basic ?? throw new NullReferenceException($"找不到基本武功, 类型={armed}, 请确保武功已配置!");
        }
    }
}