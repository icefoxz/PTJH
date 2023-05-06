using Core;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{

    /// <summary>
    /// 装备类型
    /// </summary>
    public enum EquipKinds
    {
        Weapon,
        Armor
    }

    public enum WeaponArmed
    {
        [InspectorName("空手")]Unarmed,
        [InspectorName("剑")]Sword,
        [InspectorName("刀")]Blade,
        [InspectorName("棍")]Staff
    }

    public interface IEquipment : IGameItem
    {
        EquipKinds EquipKind { get; }
        DiziGrades Grade { get; }
    }

    public interface IArmor : IEquipment
    {
        int AddHp { get; }
    }
    public interface IWeapon : IEquipment
    {
        WeaponArmed Armed { get; }
        int Damage { get; }
    }
}
