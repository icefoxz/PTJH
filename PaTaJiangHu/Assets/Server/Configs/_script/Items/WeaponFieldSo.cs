using System;
using Core;
using MyBox;
using Server.Configs.Battles;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "weaponSo", menuName = "物件/武器")]
    [Serializable] internal class WeaponFieldSo : AutoUnderscoreNamingObject,IWeapon
    {
        [SerializeField] private WeaponArmed 类型;
        [SerializeField] private int 力量加成;
        [SerializeField] private DiziGrades 品级;
        [SerializeField] private Sprite 图标;
        [SerializeField][TextArea] private string 说明;

        public Sprite Icon => 图标;

        public string About => 说明;

        public ItemType Type => ItemType.Equipment;
        public WeaponArmed Armed => 类型;
        public int Damage => 力量加成;
        public DiziGrades Grade => 品级;
        public EquipKinds EquipKind => EquipKinds.Weapon;
        public IWeapon Instance() => new WeaponField(Id, Name, Armed, Damage, Grade, About, Icon);
        private class WeaponField : IWeapon
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.Equipment;
            public WeaponArmed Armed { get; }
            public EquipKinds EquipKind => EquipKinds.Weapon;
            public int Damage { get; }
            public DiziGrades Grade { get; }

            public WeaponField(int id, string name, WeaponArmed armed,int damage, DiziGrades grade, string about, Sprite icon)
            {
                Id = id;
                Name = name;
                Armed = armed;
                Damage = damage;
                Grade = grade;
                About = about;
                Icon = icon;
            }
        }

    }
}