using System;
using BattleM;
using Core;
using MyBox;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "weaponSo", menuName = "物件/弟子/武器")]
    [Serializable] internal class WeaponFieldSo : AutoUnderscoreNamingObject, IGameItem
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }

        [ConditionalField(true,nameof(GetItem))][ReadOnly][SerializeField] private WeaponFieldSo So;
        [SerializeField] private Way.Armed 类型;
        [SerializeField] private int 力量加成;
        [SerializeField] private Weapon.Injuries 伤害类型;
        [SerializeField] private SkillGrades 品级;
        [SerializeField] private int 价钱;
        [SerializeField] private int 投掷次数 = 1;

        public ItemType Type => ItemType.Equipment;
        public int Price => 价钱;
        public Way.Armed Armed => 类型;
        public int Damage => 力量加成;
        public Weapon.Injuries Injury => 伤害类型;
        public SkillGrades Grade => 品级;
        public int FlingTimes => 投掷次数;
        public IWeapon Instance() => new WeaponField(Id, Name, Armed, Damage, Injury, Grade, FlingTimes);
        private class WeaponField : IWeapon
        {
            public int Id { get; }
            public string Name { get; }
            public Way.Armed Armed { get; }
            public int Damage { get; }
            public Weapon.Injuries Injury { get; }
            public SkillGrades Grade { get; }
            public int FlingTimes { get;  }

            public WeaponField(int id, string name, Way.Armed armed, int damage, Weapon.Injuries injury, SkillGrades grade, int flingTimes)
            {
                Id = id;
                Name = name;
                Armed = armed;
                Damage = damage;
                Injury = injury;
                Grade = grade;
                FlingTimes = flingTimes;
            }
            public void AddFlingTime(int value)
            {
                throw new NotImplementedException();
            }
        }
    }
}