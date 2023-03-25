using Server.Configs.BattleSimulation;
using Server.Configs.Items;
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

        internal Gender Gender => 性别;
        internal int Strength => 力;
        internal int Agility => 敏;
        internal int Hp => _hp;
        internal int Mp => _mp;

        internal WeaponFieldSo Weapon => 武器;
        internal ArmorFieldSo Armor => 防具;

        public ISimCombat GetSimCombat(BattleSimulatorConfigSo cfg)
        {

            return cfg.GetSimulation(Name, Strength, Agility, Hp, Mp,
                Weapon != null ? Weapon.Damage : 0,
                Armor != null ? Armor.AddHp : 0);
        } }
}