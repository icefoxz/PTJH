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
        void SetFling(IWeapon weapon,int flings);
        void SetArmor(IArmor armor);
        void FlingConsume();
        void RemoveWeapon(IWeapon weapon);
    }

    public class Equipment : IEquipment
    {
        public IWeapon Weapon { get; private set; }
        public IWeapon Fling { get; private set; }
        public IArmor Armor { get; private set; }
        public int FlingTimes { get; private set; }

        public Equipment()
        {
            
        }
        public Equipment(IEquip e)
        {
            Weapon = e?.Weapon;
            Fling = e?.Fling;
            Armor = e?.Armor;
        }

        public void SetWeapon(IWeapon weapon) => Weapon = weapon;
        public void SetFling(IWeapon weapon,int flings)
        {
            Fling = weapon;
            FlingTimes = flings;
        }

        public void SetArmor(IArmor armor) => Armor = armor;

        public void FlingConsume()
        {
            FlingTimes--;
            if (FlingTimes <= 0) RemoveWeapon(Fling);
        }

        public void RemoveWeapon(IWeapon weapon)
        {
            if (Weapon == weapon)
                SetWeapon(null);
            if (Fling == weapon)
                SetFling(null, 0);
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

        public Armor(int id, IArmor a)
        {
            Id = id;
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

        public Weapon(string name, int damage, int grade, Injuries injury, Way.Armed armed, int flingTimes = 1)
        {
            Name = name;
            Damage = damage;
            Grade = grade;
            Injury = injury;
            Armed = armed;
            FlingTimes = flingTimes;
        }

        public Weapon(IWeapon w)
        {
            Name = w.Name;
            Damage = w.Damage;
            Grade = w.Grade;
            Injury = w.Injury;
            Armed = w.Armed;
            FlingTimes = w.FlingTimes;
        }
    }
}