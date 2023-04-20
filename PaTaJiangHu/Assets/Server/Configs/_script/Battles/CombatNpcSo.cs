using System;
using System.Linq;
using Models;
using MyBox;
using Server.Configs.BattleSimulation;
using Server.Configs.Items;
using Server.Configs.Skills;
using UnityEngine;
using UnityEngine.Analytics;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "id_战斗Npc", menuName = "历练/战斗Npc")]
    internal class CombatNpcSo : AutoDashNamingObject
    {
        [SerializeField] private Gender 性别;
        [SerializeField] private int 力;
        [SerializeField] private int 敏;
        [SerializeField] private int _hp;
        [SerializeField] private int _mp;
        [SerializeField] private WeaponFieldSo 武器;
        [SerializeField] private ArmorFieldSo 防具;
        [SerializeField] private SkillField[] 技能;

        internal Gender Gender => 性别;
        internal int Strength => 力;
        internal int Agility => 敏;
        internal int Hp => _hp;
        internal int Mp => _mp;

        private SkillField[] Skills => 技能;
        internal WeaponFieldSo Weapon => 武器;
        internal ArmorFieldSo Armor => 防具;
        internal WeaponArmed Armed => Weapon?.Armed ?? WeaponArmed.Unarmed;

        public ISimCombat GetSimCombat(BattleSimulatorConfigSo cfg)
        {
            return cfg.GetSimulation(1, Name, Strength, Agility, Hp, Mp,
                Weapon != null ? Weapon.Damage : 0,
                Armor != null ? Armor.AddHp : 0);
        }

        public ICombatSet GetCombatSet()
        {
            var f = Skills.First(s => s.SkillType == SkillType.Force);
            var d = Skills.First(s => s.SkillType == SkillType.Dodge);
            var c = Skills.First(s => s.SkillType == SkillType.Combat && s.Combat.Armed == Armed);
            return DiziSkill.Instance((c.Combat, c.Level), (f.Force, f.Level), (d.Dodge, d.Level)).GetcombatSet();
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