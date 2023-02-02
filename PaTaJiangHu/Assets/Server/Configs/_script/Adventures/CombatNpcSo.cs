using System;
using BattleM;
using Data;
using Server.Configs.Battles;
using Server.Configs.BattleSimulation;
using Server.Configs.Items;
using UnityEngine;
using UnityEngine.Analytics;

namespace Server.Configs.Adventures
{
    [CreateAssetMenu(fileName = "id_战斗Npc", menuName = "历练/战斗Npc")]
    internal class CombatNpcSo : AutoDashNamingObject
    {
        [SerializeField] private Gender 性别;
        [SerializeField] private int 力;
        [SerializeField] private int 敏;
        [SerializeField] private int _hp;
        [SerializeField] private int _mp;
        [SerializeField] private CombatSkill 武功;
        [SerializeField] private ForceSkill 内功;
        [SerializeField] private DodgeSkill 轻功;
        [SerializeField] private WeaponFieldSo 武器;
        [SerializeField] private ArmorFieldSo 防具;

        internal Gender Gender => 性别;
        internal int Strength => 力;
        internal int Agility => 敏;
        internal int Hp => _hp;
        internal int Mp => _mp;

        internal WeaponFieldSo Weapon => 武器;
        internal ArmorFieldSo Armor => 防具;
        private CombatSkill Combat => 武功;
        private ForceSkill Force => 内功;
        private DodgeSkill Dodge => 轻功;
        internal ICombatSkill GetCombat() => Combat.GetCombat();
        internal IForceSkill GetForce() => Force.GetForce();
        internal IDodgeSkill GetDodge() => Dodge.GetDodge();

        public ISimCombat GetSimCombat(ConditionPropertySo cfg)
        {
            var combat = Combat.GetCombat();
            var force = Force.GetForce();
            var dodge = Dodge.GetDodge();
            return cfg.GetSimulation(Name, Strength, Agility,
                Weapon != null ? Weapon.Damage : 0, 
                Armor != null ? Armor.Def : 0,
                combat.Grade, combat.Level,
                force.Grade, force.Level,
                dodge.Grade, dodge.Level);
        }
        [Serializable] private class CombatSkill
        {
            [SerializeField] private CombatFieldSo _so;
            [SerializeField] private int _level = 1;

            private CombatFieldSo So => _so;
            private int Level => _level;
            public ICombatSkill GetCombat() => So.GetFromLevel(Level);
        }
        [Serializable] private class ForceSkill
        {
            [SerializeField] private ForceFieldSo _so;
            [SerializeField] private int _level = 1;

            private ForceFieldSo So => _so;
            private int Level => _level;
            public IForceSkill GetForce() => So.GetFromLevel(Level);
        }
        [Serializable] private class DodgeSkill
        {
            [SerializeField] private DodgeFieldSo _so;
            [SerializeField] private int _level = 1;

            private DodgeFieldSo So => _so;
            private int Level => _level;
            public IDodgeSkill GetDodge() => So.GetFromLevel(Level);
        }
    }
}