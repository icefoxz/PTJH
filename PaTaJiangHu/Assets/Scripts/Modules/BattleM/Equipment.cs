using Data;
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
    }

    public interface IEquipment : IEquip
    {
        void SetWeapon(IWeapon weapon);
        void SetArmor(IArmor armor);
        void FlingConsume();
        void RemoveWeapon(IWeapon weapon);
    }

    public class Equipment : IEquipment
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

    public interface IArmor: IDataElement
    {
        int Def { get; }
    }

    public class Armor : IArmor
    {
        public string Name { get; }
        public int Def { get; }
        public int Id { get; }

        public Armor(string name, int def, int id)
        {
            Name = name;
            Def = def;
            Id = id;
        }

        public Armor(IArmor a)
        {
            Id = a.Id;
            Name = a.Name;
            Def = a.Def;
        }
    }

    public interface IWeapon : IIdElement
    {
        string Name { get; }
        Way.Armed Armed { get; }
        int Damage { get; }
        Weapon.Injuries Injury { get; }
        int Grade { get; }
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
        public Way.Armed Armed { get; }
        public int Damage { get; }
        public Injuries Injury { get; }
        public int Grade { get; }
        public int FlingTimes { get; set; }
        public void AddFlingTime(int value) => FlingTimes += value;

        public Weapon(int id,string name, int damage, int grade, Injuries injury, Way.Armed armed, int flingTimes = 1)
        {
            Id = id;
            Name = name;
            Damage = damage;
            Grade = grade;
            Injury = injury;
            Armed = armed;
            FlingTimes = flingTimes;
        }

        public Weapon(IWeapon w)
        {
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