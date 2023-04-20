//弟子模型,处理技能和战斗相关代码

using System.Collections.Generic;
using System.Linq;
using Server.Configs.Battles;
using Server.Configs.Skills;

namespace Models
{
    //弟子装备和技能处理类,主要处理战斗相关的功能
    public partial class Dizi
    {
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }
        internal void SetWeapon(IWeapon weapon)
        {
            Log(weapon == null ? $"卸下{Weapon.Name}" : $"装备{weapon.Name}!");
            Weapon = weapon;
        }
        internal void SetArmor(IArmor armor)
        {
            Log(armor == null ? $"卸下{Armor.Name}" : $"装备{armor.Name}!");
            Armor = armor;
        }

        public DiziSkill Skill { get; private set; }

        internal ICombatSet GetBattle() => Skill.GetcombatSet();
        internal void SetSkill(DiziSkill skill) => Skill = skill;
    }

    //弟子技能栏
    public class DiziSkill
    {
        private List<CombatSkill> _combatSkills;
        private List<ForceSkill> _forceSkills;
        private List<DodgeSkill> _dodgeSkills;

        public IReadOnlyList<CombatSkill> CombatSkills => _combatSkills;
        public IReadOnlyList<ForceSkill> ForceSkills => _forceSkills;
        public IReadOnlyList<DodgeSkill> DodgeSkills => _dodgeSkills;

        public CombatSkill Combat { get; private set; }
        public ForceSkill Force { get; private set; }
        public DodgeSkill Dodge { get; private set; }

        public DiziSkill(CombatSkill combat, ForceSkill force, DodgeSkill dodge) : this(
            new List<CombatSkill> { combat },
            new List<ForceSkill> { force },
            new List<DodgeSkill> { dodge })
        {
            Dodge = dodge;
            Combat = combat;
            Force = force;
        }

        public DiziSkill(List<CombatSkill> combatSkills, List<ForceSkill> forceSkills, List<DodgeSkill> dodgeSkills)
        {
            _dodgeSkills = dodgeSkills;
            _combatSkills = combatSkills;
            _forceSkills = forceSkills;
            Dodge = dodgeSkills.First();
            Combat = combatSkills.First();
            Force = forceSkills.First();
        }

        public void SetDodge(DodgeSkill skill) => Dodge = skill;
        public void SetCombat(CombatSkill skill) => Combat = skill;
        public void SetForce(ForceSkill skill) => Force = skill;
        public ICombatSet GetcombatSet() => new[] { Combat.GetCombatSet(), Force.GetCombatSet(), Dodge.GetCombatSet() }.Combine();

        public abstract class SkillBase<TSkill> where TSkill : ISkill
        {
            public TSkill Skill { get; private set; }
            public int Level { get; private set;  }
            public int MaxLevel => Skill.MaxLevel();

            protected SkillBase(TSkill skill, int level = 1)
            {
                Skill = skill;
                Level = level;
            }
            public void SetLevel(int level) => Level = level;
            public void UpLevel() => Level++;
            public ICombatSet GetCombatSet()=> Skill.GetCombatSet(Level);
        }

        public class DodgeSkill : SkillBase<ISkill>
        {
            public DodgeSkill(ISkill skill, int level = 1) : base(skill, level) { }
        }

        public class CombatSkill : SkillBase<ICombatSkill>
        {
            public CombatSkill(ICombatSkill skill, int level = 1) : base(skill, level) { }
        }

        public class ForceSkill : SkillBase<ISkill>
        {
            public ForceSkill(ISkill skill, int level = 1) : base(skill, level) { }
        }

        public static DodgeSkill InstanceDodge(ISkill skill, int level = 1) => new(skill, level);
        public static CombatSkill InstanceCombat(ICombatSkill skill, int level = 1) => new(skill, level);
        public static ForceSkill InstanceForce(ISkill skill, int level = 1) => new(skill, level);

        public static DiziSkill Instance(
            (ICombatSkill combat,int level) combatSkill, 
            (ISkill force,int level) forceSkill,
            (ISkill dodge, int level) dodgeSkill)
        {
            return new DiziSkill(
                InstanceCombat(combatSkill.combat, combatSkill.level),
                InstanceForce(forceSkill.force, forceSkill.level),
                InstanceDodge(dodgeSkill.dodge, dodgeSkill.level));
        }
    }

}