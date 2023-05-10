//弟子模型,处理技能和战斗相关代码
using System;
using System.Collections.Generic;
using System.Linq;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Configs.Skills;
using Utls;

namespace Models
{
    //弟子装备和技能处理类,主要处理战斗相关的功能
    public partial class Dizi
    {
        private SkillConfigSo SkillConfig => Game.Config.DiziCfg.SkillCfg;
        public IDiziEquipment Equipment => _equipment;
        internal DiziEquipment _equipment;
        internal void SetWeapon(IWeapon weapon)
        {
            Log(weapon == null ? $"卸下{_equipment.Weapon.Name}" : $"装备{weapon.Name}!");
            _equipment.SetWeapon(weapon);
        }
        internal void SetArmor(IArmor armor)
        {
            Log(armor == null ? $"卸下{_equipment.Armor.Name}" : $"装备{armor.Name}!");
            _equipment.SetArmor(armor);
        }
        internal void SetShoes(IShoes shoes)
        {
            Log(shoes == null ? $"卸下{_equipment.Shoes.Name}" : $"装备{shoes.Name}!");
            _equipment.SetShoes(shoes);
        }
        internal void SetDecoration(IDecoration decoration)
        {
            Log(decoration == null ? $"卸下{_equipment.Decoration.Name}" : $"装备{decoration.Name}!");
            _equipment.SetDecoration(decoration);
        }

        public DiziSkill Skill { get; private set; } = DiziSkill.Empty();

        public ICombatSet GetCombatSet() => new[] { SkillConfig.GetCombatSet(this),Equipment.GetCombatSet() }.Combine();
        internal void SetSkill(DiziSkill skill) => Skill = skill;

        public void SkillLevelUp(ISkill skill)
        {
            var level = Skill.LevelUp(skill);
            SendEvent(EventString.Dizi_Skill_LevelUp);
            Log($"技能{skill.Name}升级到{level}!");
        }

        public void UseSkill(SkillType type, int index)
        {
            var skill = Skill.UseSkill(type, index);
            SendEvent(EventString.Dizi_Skill_Update);
            Log($"使用{skill}");
        }

        public void ForgetSkill(SkillType type, int index)
        {
            var skill = Skill.GetSkill(type, index);
            Skill.RemoveSkill(skill);
            SendEvent(EventString.Dizi_Skill_Update);
            Log($"遗忘{skill.Name}");
        }
    }

    public interface IDiziEquipment
    {
        IWeapon Weapon { get; }
        IArmor Armor { get; }
        IShoes Shoes { get; }
        IDecoration Decoration { get; }
        IEnumerable<IEquipment> AllEquipments { get; }
        int GetPropAddon(DiziProps prop);
        ICombatProps GetCombatProps();
        /// <summary>
        /// 战斗时被打掉装备的主要逻辑
        /// </summary>
        /// <param name="teamId"></param>
        /// <param name="equipment"></param>
        /// <returns></returns>
        IDiziCombatUnit CombatDisarm(int teamId,IEquipment equipment);
        ICombatSet GetCombatSet();
    }

    public class DiziEquipment : IDiziEquipment
    {
        private Dizi _dizi;
        public DiziEquipment(Dizi dizi)
        {
            _dizi = dizi;
        }
        public IWeapon Weapon { get; private set; }
        public IArmor Armor { get; private set; }
        public IShoes Shoes { get; private set; }
        public IDecoration Decoration { get; private set; }
        public IEnumerable<IEquipment> AllEquipments => new IEquipment[] { Weapon, Armor, Shoes, Decoration }.Where(e => e != null);
        internal void SetWeapon(IWeapon weapon) => Weapon = weapon;
        internal void SetArmor(IArmor armor) => Armor = armor;
        internal void SetShoes(IShoes shoes) => Shoes = shoes;
        internal void SetDecoration(IDecoration decoration) => Decoration = decoration;

        public int GetPropAddon(DiziProps prop) => (int)AllEquipments.Sum(e => e.GetAddOn(prop));
        public ICombatProps GetCombatProps() => AllEquipments.Select(e => e.GetCombatProps()).Combine();
        public IDiziCombatUnit CombatDisarm(int teamId,IEquipment equipment)
        {
            var type = equipment.EquipKind;
            switch (type)
            {
                case EquipKinds.Weapon:
                    Weapon = null;
                    break;
                case EquipKinds.Armor:
                    Armor = null;
                    break;
                case EquipKinds.Shoes:
                    Shoes = null;
                    break;
                case EquipKinds.Decoration:
                    Decoration = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return new DiziCombatUnit(teamId, _dizi);
        }

        public ICombatSet GetCombatSet() => AllEquipments.Select(e=>e.GetCombatSet()).Combine();
    }

    //弟子技能栏
    public class DiziSkill
    {
        private List<CombatSkillMap> _combatSkills = new List<CombatSkillMap>();
        private List<ForceSkillMap> _forceSkills = new List<ForceSkillMap>();
        private List<DodgeSkillMap> _dodgeSkills = new List<DodgeSkillMap>();

        public IReadOnlyList<CombatSkillMap> CombatSkills => _combatSkills;
        public IReadOnlyList<ForceSkillMap> ForceSkills => _forceSkills;
        public IReadOnlyList<DodgeSkillMap> DodgeSkills => _dodgeSkills;
        private IEnumerable<SkillMap> CombatMaps => CombatSkills;
        private IEnumerable<SkillMap> ForceMaps => ForceSkills;
        private IEnumerable<SkillMap> DodgeMaps => DodgeSkills;
        public IEnumerable<SkillMap> Maps => CombatMaps.Concat(ForceMaps).Concat(DodgeMaps);

        public CombatSkillMap Combat { get; private set; }
        public ForceSkillMap Force { get; private set; }
        public DodgeSkillMap Dodge { get; private set; }

        public DiziSkill(CombatSkillMap combat, ForceSkillMap force, DodgeSkillMap dodge) : this(
            new List<CombatSkillMap> { combat },
            new List<ForceSkillMap> { force },
            new List<DodgeSkillMap> { dodge })
        {
            Dodge = dodge;
            Combat = combat;
            Force = force;
        }

        public DiziSkill(List<CombatSkillMap> combatSkills, List<ForceSkillMap> forceSkills, List<DodgeSkillMap> dodgeSkills)
        {
            _dodgeSkills = dodgeSkills;
            _combatSkills = combatSkills;
            _forceSkills = forceSkills;
            Dodge = dodgeSkills.First();
            Combat = combatSkills.First();
            Force = forceSkills.First();
        }

        public SkillMap UseSkill(SkillType type, int index)
        {
            SkillMap map = null;
            switch (type)
            {
                case SkillType.Combat:
                    map = Combat = CombatSkills[index];
                    break;
                case SkillType.Force:
                    map = Force = ForceSkills[index];
                    break;
                case SkillType.Dodge:
                    map = Dodge = DodgeSkills[index];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return map;
        }

        public int LevelUp(ISkill skill)
        {
            var set = Maps.SingleOrDefault(c => c.IsThis(skill));
            if (set == null)
                set = AddSkill(skill);
            else set.LevelUp();
            return set.Level;
        }

        public SkillMap AddSkill(ISkill skill)
        {
            SkillMap map = null;
            switch (skill.SkillType)
            {
                case SkillType.Combat:
                    var com = InstanceCombat((ICombatSkill)skill);
                    _combatSkills.Add(com);
                    map = com;
                    break;
                case SkillType.Force:
                    var force = InstanceForce(skill);
                    _forceSkills.Add(force);
                    map = force;
                    break;
                case SkillType.Dodge:
                    var dodge = InstanceDodge(skill);
                    _dodgeSkills.Add(dodge);
                    map = dodge;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return map;
        }

        public void RemoveSkill(ISkill skill) 
        {
            switch (skill.SkillType)
            {
                case SkillType.Combat:
                    var combat = CombatSkills.Single(m => m.IsThis(skill));
                    _combatSkills.Remove(combat);
                    if (Combat.IsThis(skill))
                        Combat = null;
                    break;
                case SkillType.Force:
                    var force = ForceSkills.Single(m => m.IsThis(skill));
                    _forceSkills.Remove(force);
                    if (Force.IsThis(skill))
                        Force = null;
                    break;
                case SkillType.Dodge:
                    var dodge = DodgeSkills.Single(m => m.IsThis(skill));
                    _dodgeSkills.Remove(dodge);
                    if (Dodge.IsThis(skill))
                        Dodge = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        }

        public int GetLevel(ISkill skill) => Maps.SingleOrDefault(m => m.IsThis(skill))?.Level ?? 0;

        public ISkill GetSkill(SkillType type, int index)
        {
            var skill = type switch
            {
                SkillType.Combat => CombatSkills[index].Skill,
                SkillType.Force => ForceSkills[index].Skill,
                SkillType.Dodge => DodgeSkills[index].Skill,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            XDebug.Log($"获取技能{skill.Name}");
            return skill;
        }

        public abstract class SkillMap
        {
            public int Level { get; protected set;  }
            public abstract int MaxLevel { get; }

            public void SetLevel(int level) => Level = level;
            public void LevelUp() => Level++;
            public abstract ISkillProp[] GetProps(int level);
            public abstract ISkillProp[] GetProps();
            public abstract ISkillAttribute[] GetAttributes(int level);
            public abstract ISkillAttribute[] GetAttributes();
            public abstract ICombatSet GetCombatSet();
            public abstract ICombatSet GetCombatSet(int level);
            public abstract bool IsThis(ISkill skill);
        }
        public abstract class SkillMap<TSkill> : SkillMap where TSkill : ISkill
        {
            public TSkill Skill { get; private set; }
            public override int MaxLevel => Skill.MaxLevel();

            protected SkillMap(TSkill skill, int level = 1)
            {
                Skill = skill;
                Level = level;
            }
            public override ISkillProp[] GetProps(int level) => Skill.GetProps(level);
            public override ISkillProp[] GetProps() => GetProps(Level);
            public override ISkillAttribute[] GetAttributes(int level) => Skill.GetAttributes(level);
            public override ISkillAttribute[] GetAttributes() => GetAttributes(Level);
            public override ICombatSet GetCombatSet()=> GetCombatSet(Level);
            public override ICombatSet GetCombatSet(int level) => Skill.GetCombatSet(level);
            public override bool IsThis(ISkill skill) => Skill.Id == skill.Id && Skill.SkillType == skill.SkillType;

            public override string ToString() => $"{Skill.Name}({Level})";
        }

        public class DodgeSkillMap : SkillMap<ISkill>
        {
            public DodgeSkillMap(ISkill skill, int level = 1) : base(skill, level) { }
        }

        public class CombatSkillMap : SkillMap<ICombatSkill>
        {
            public CombatSkillMap(ICombatSkill skill, int level = 1) : base(skill, level) { }
        }

        public class ForceSkillMap : SkillMap<ISkill>
        {
            public ForceSkillMap(ISkill skill, int level = 1) : base(skill, level) { }
        }

        public static DodgeSkillMap InstanceDodge(ISkill skill, int level = 1) => new(skill, level);
        public static CombatSkillMap InstanceCombat(ICombatSkill skill, int level = 1) => new(skill, level);
        public static ForceSkillMap InstanceForce(ISkill skill, int level = 1) => new(skill, level);

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

        public static DiziSkill Empty() => new(
            InstanceCombat(null),
            InstanceForce(null),
            InstanceDodge(null));

    }

}