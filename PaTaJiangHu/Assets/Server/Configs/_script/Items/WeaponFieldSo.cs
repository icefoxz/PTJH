using System;
using Core;
using MyBox;
using Server.Configs.Battles;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "weaponSo", menuName = "物件/弟子/武器")]
    [Serializable] internal class WeaponFieldSo : AutoUnderscoreNamingObject,IWeapon
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }

        [ConditionalField(true,nameof(GetItem))][ReadOnly][SerializeField] private WeaponFieldSo So;
        [SerializeField] private WeaponArmed 类型;
        [SerializeField] private int 力量加成;
        [SerializeField] private DiziGrades 品级;
        [SerializeField] private int 价钱;
        [SerializeField][TextArea] private string 说明;

        public string About => 说明;

        public ItemType Type => ItemType.Equipment;
        public int Price => 价钱;
        public WeaponArmed Armed => 类型;
        public int Damage => 力量加成;
        public DiziGrades Grade => 品级;
        public EquipKinds EquipKind => EquipKinds.Weapon;
        public IWeapon Instance() => new WeaponField(Id, Name, Armed, Damage, Grade, Price, About);
        private class WeaponField : IWeapon
        {
            public int Id { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.Equipment;
            public int Price { get; }
            public WeaponArmed Armed { get; }
            public EquipKinds EquipKind => EquipKinds.Weapon;
            public int Damage { get; }
            public DiziGrades Grade { get; }

            public WeaponField(int id, string name, WeaponArmed armed,int damage, DiziGrades grade, int price, string about)
            {
                Id = id;
                Name = name;
                Armed = armed;
                Damage = damage;
                Grade = grade;
                About = about;
                Price = price;
            }
        }

    }
}