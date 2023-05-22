using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using MyBox;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Items;
using Server.Configs.Skills;
using UnityEngine;
using UnityEngine.Analytics;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "id_战斗Npc", menuName = "历练/战斗Npc")]
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

        internal Gender Gender => 性别;
        internal int Strength => 力;
        internal int Agility => 敏;
        internal int Hp => _hp;
        internal int Mp => _mp;

        private SkillField[] Skills => 技能;
        internal WeaponFieldSo Weapon => 武器;
        internal ArmorFieldSo Armor => 防具;
        IArmor IDiziEquipment.Armor => Armor;
        IWeapon IDiziEquipment.Weapon => Weapon;
        internal WeaponArmed Armed => Weapon?.Armed ?? WeaponArmed.Unarmed;
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

        public int GetPropAddon(DiziProps prop)=> (int)AllEquipments.Sum(e => e.GetAddOn(prop));

        public ISimCombat GetSimCombat(BattleSimulatorConfigSo cfg)
        {
            var strAddon = GetPropAddon(DiziProps.Strength);
            var agiAddon = GetPropAddon(DiziProps.Agility);
            var hpAddon = GetPropAddon(DiziProps.Hp);
            var mpAddon = GetPropAddon(DiziProps.Mp);
            return cfg.GetSimulation(1, Name, Strength + strAddon, Agility + agiAddon, Hp + hpAddon, Mp + mpAddon);
        }

        public ICombatSet GetCombatSet()
        {
            var f = Skills.First(s => s.SkillType == SkillType.Force);
            var d = Skills.First(s => s.SkillType == SkillType.Dodge);
            var c = Skills.First(s => s.SkillType == SkillType.Combat && s.Combat.Armed == Armed);
            return new[]
            {
                c.Combat.GetCombatSet(c.Level),
                f.Force.GetCombatSet(f.Level),
                d.Dodge.GetCombatSet(d.Level)
            }.Combine();
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
    }
}