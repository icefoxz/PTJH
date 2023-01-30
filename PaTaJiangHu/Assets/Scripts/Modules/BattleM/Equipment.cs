using Core;
using Data;
using Server.Controllers;
using Systems;
using UnityEngine;

namespace BattleM
{
    /// <summary>
    /// 最基础的角色装备栏属性
    /// </summary>
    public interface IEquip
    {
        /// <summary>
        /// 武器
        /// </summary>
        IWeapon Weapon { get; }
        /// <summary>
        /// 暗器
        /// </summary>
        IWeapon Fling { get; }
        /// <summary>
        /// 护甲
        /// </summary>
        IArmor Armor { get; }

        Way.Armed Armed => Weapon?.Armed ?? Way.Armed.Unarmed;
        void FlingConsume();
    }

    public class Equipment : IEquip
    {
        public IWeapon Weapon { get; private set; }
        public IWeapon Fling { get; private set; }
        public IArmor Armor { get; private set; }
        public int FlingTimes => Fling?.FlingTimes ?? 0;

        public Equipment()
        {
            
        }
        public Equipment(IEquip e)
        {
            Weapon = e != null ? new Weapon(e.Weapon) : null;
            Fling = e is { Fling: { } } ? new Weapon(e.Fling) : null;
            Armor = e != null ? new Armor(e.Armor) : null;
        }

        public void SetWeapon(IWeapon weapon) => Weapon = weapon;

        public void SetArmor(IArmor armor) => Armor = armor;

        public void FlingConsume()
        {
            Fling.AddFlingTime(-1);
            if (FlingTimes <= 0) RemoveWeapon(Fling);
        }

        public void RemoveWeapon(IWeapon weapon)
        {
            if (Weapon == weapon)
                Weapon = null;
            if (Fling == weapon)
                Fling = null;
        }
    }

    public interface IArmor: IEquipment
    {
        int Def { get; }
        SkillGrades Grade { get; }
    }

    public class Armor : IArmor
    {
        public string Name { get; }
        public string About { get; }
        public ItemType Type => ItemType.Equipment;
        public int Def { get; }
        public EquipKinds EquipKind => EquipKinds.Armor;
        public SkillGrades Grade { get; }
        public int Price { get; }
        public int Id { get; }

        public Armor(string name, int def, int id, int price, SkillGrades grade, string about)
        {
            Name = name;
            Def = def;
            Id = id;
            Price = price;
            Grade = grade;
            About = about;
        }

        public Armor(IArmor a)
        {
            About = a.About;
            Grade = a.Grade;
            Price = a.Price;
            Id = a.Id;
            Name = a.Name;
            Def = a.Def;
        }
    }

    public interface IWeapon : IEquipment
    {
        Way.Armed Armed { get; }
        int Damage { get; }
        Weapon.Injuries Injury { get; }
        SkillGrades Grade { get; }
        int FlingTimes { get; }
        void AddFlingTime(int value);
    }

    public class Weapon : IWeapon
    {
        public enum Injuries
        {
            [InspectorName("利器")] Sharp,
            [InspectorName("钝器")] Blunt
        }
        public int Id { get; }
        public string Name { get; }
        public string About { get; }
        public ItemType Type => ItemType.Equipment;
        public int Price { get; }
        public Way.Armed Armed { get; }
        public EquipKinds EquipKind => EquipKinds.Weapon;
        public int Damage { get; }
        public Injuries Injury { get; }
        public SkillGrades Grade { get; }
        public int FlingTimes { get; set; }
        public void AddFlingTime(int value) => FlingTimes += value;

        public Weapon(int id,string name, int damage, SkillGrades grade, Injuries injury, Way.Armed armed, string about, int price, int flingTimes = 1)
        {
            Id = id;
            Name = name;
            Damage = damage;
            Grade = grade;
            Injury = injury;
            Armed = armed;
            About = about;
            Price = price;
            FlingTimes = flingTimes;
        }

        public Weapon(IWeapon w)
        {
            About = w.About;
            Price = w.Price;
            Id = w.Id;
            Name = w.Name;
            Damage = w.Damage;
            Grade = w.Grade;
            Injury = w.Injury;
            Armed = w.Armed;
            FlingTimes = w.FlingTimes;
        }
    }
}