using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Models;
using MyBox;
using Server.Configs.BattleSimulation;
using Server.Configs.ChallengeStages;
using Server.Configs.Characters;
using Server.Configs.Items;
using Server.Configs.Skills;
using UnityEngine;
using UnityEngine.Analytics;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "id_战斗Npc", menuName = "状态玩法/历练/战斗Npc")]
    internal class CombatNpcSo : AutoDashNamingObject,IDiziEquipment
    {
        [SerializeField] private Gender 性别;
        [SerializeField] private int 力;
        [SerializeField] private int 敏;
        [SerializeField] private int _hp;
        [SerializeField] private int _mp;
        [SerializeField] private WeaponFieldSo 武器;
        [SerializeField] private ArmorFieldSo 防具;
        [SerializeField] private ShoesFieldSo 鞋子;
        [SerializeField] private DecorationFieldSo 挂件;
        [SerializeField] private SkillField[] 技能;
        [SerializeField] private Sprite 头像;
        [SerializeField] private NpcGifted 战斗天赋;

        internal Gender Gender => 性别;
        public int Strength => 力;
        public int Agility => 敏;
        public int Hp => _hp;
        public int Mp => _mp;
        public Sprite Icon => 头像;

        private SkillField[] Skills => 技能;
        private WeaponFieldSo Weapon => 武器;
        private ArmorFieldSo Armor => 防具;
        
        IArmor IDiziEquipment.Armor => Armor;
        IWeapon IDiziEquipment.Weapon => Weapon;
        private WeaponArmed Armed => Weapon?.Armed ?? WeaponArmed.Unarmed;
        public IDiziEquipment Equipment => this;
        public IShoes Shoes => 鞋子;
        public IDecoration Decoration => 挂件;
        IEnumerable<IEquipment> AllEquipments => new IEquipment[]
        {
            Weapon,
            Armor,
            Shoes,
            Decoration
        }.Where(e => e != null);
        public ICombatGifted Gifted => 战斗天赋;

        public float GetPropAddon(DiziProps prop)=> AllEquipments.Sum(e => e.GetAddOn(prop));

        public ISimCombat GetSimCombat(BattleSimulatorConfigSo cfg)
        {
            var strAddon = GetPropAddon(DiziProps.Strength);
            var agiAddon = GetPropAddon(DiziProps.Agility);
            var hpAddon = GetPropAddon(DiziProps.Hp);
            var mpAddon = GetPropAddon(DiziProps.Mp);
            return cfg.GetSimulation(1, Name, Strength + strAddon, Agility + agiAddon, Hp + hpAddon, Mp + mpAddon);
        }

        public ISkillMap<ISkillInfo> GetDodgeSkillInfo()
        {
            var d = GetSkill(SkillType.Dodge);
            return new SkillMap<ISkill>(d.Level, d.Dodge);
        }
        public ISkillMap<ISkillInfo> GetForceSkillinfo()
        {
            var f = GetSkill(SkillType.Force);
            return new SkillMap<ISkill>(f.Level, f.Force);
        }
        public ISkillMap<ICombatSkillInfo> GetCombatSkillInfo()
        {
            var c = GetSkill(SkillType.Combat);
            return new SkillMap<ICombatSkillInfo>(c.Level, c.Combat);
        }

        public ICombatSet GetCombatSet()
        {
            var f = GetSkill(SkillType.Force);
            var d = GetSkill(SkillType.Dodge);
            var c = GetSkill(SkillType.Combat);
            return new[]
            {
                c.Combat.GetCombatSet(c.Level),
                f.Force.GetCombatSet(f.Level),
                d.Dodge.GetCombatSet(d.Level)
            }.Combine();
        }

        // 目前获取技能的方式是第一个符合条件的技能, 所以如果技能有多个, 只会获取第一个是主手武器的技能
        private SkillField GetSkill(SkillType skillType)
        {
            return skillType == SkillType.Combat
                ? Skills.First(s => s.SkillType == skillType && s.Combat.Armed == Armed)
                : Skills.First(s => s.SkillType == skillType);
        }

        [Serializable]private class SkillField
        {
            private bool SetName()
            {
                switch (SkillType)
                {
                    case SkillType.Combat:
                        _name = Combat != null ? $"{Combat.Name}【{Level}】" : "未设置";
                        break;
                    case SkillType.Force:
                        _name = Force != null ? $"{Force.Name}【{Level}】" : "未设置";
                        break;
                    case SkillType.Dodge:
                        _name = Dodge != null ? $"{Dodge.Name}【{Level}】" : "未设置";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return true;
            }
            [ConditionalField(true, nameof(SetName))][ReadOnly][SerializeField] private string _name;
            [Min(1)][SerializeField] private int 等级 = 1;
            [SerializeField] private SkillType 类型;
            [ConditionalField(nameof(类型),false, SkillType.Combat)][SerializeField] private CombatFieldSo 武功;
            [ConditionalField(nameof(类型),false, SkillType.Force)][SerializeField] private ForceFieldSo 内功;
            [ConditionalField(nameof(类型),false, SkillType.Dodge)][SerializeField] private DodgeFieldSo 轻功;

            public SkillType SkillType => 类型;
            public int Level => 等级;
            public CombatFieldSo Combat => 武功;
            public ForceFieldSo Force => 内功;
            public DodgeFieldSo Dodge => 轻功;
        }
        [Serializable]private class NpcGifted : ICombatGifted
        {
            [SerializeField] private float 闪避率上限加成;
            [SerializeField] private float 会心率上限加成;
            [SerializeField] private float 重击率上限加成;
            [SerializeField] private float 会心伤害加成;
            [SerializeField] private float 重击伤害加成;
            [SerializeField] private float 内力伤害转化加成;
            [SerializeField] private float 内力护甲转化加成;

            public float DodgeRateMax => 闪避率上限加成;
            public float CritRateMax => 会心率上限加成;
            public float HardRateMax => 重击率上限加成;
            public float CritDamageRate => 会心伤害加成;
            public float HardDamageRate => 重击伤害加成;
            public float MpDamageRate => 内力伤害转化加成;
            public float MpArmorRate => 内力护甲转化加成;
        }
        private record SkillMap<T> : ISkillMap<T> where T : ISkillInfo
        {
            public int Level { get; }
            public T Skill { get; }

            public SkillMap(int level, T skill)
            {
                Level = level;
                Skill = skill;
            }
        }
    }
}