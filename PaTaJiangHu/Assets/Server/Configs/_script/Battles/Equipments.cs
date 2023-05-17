using Core;
using Server.Configs.Characters;
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
        Armor,
        Shoes,
        Decoration,
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
        ColorGrade Grade { get; }
        float GetAddOn(DiziProps prop);
        int Quality { get; }
        ICombatSet GetCombatSet();
        //ICombatProps GetCombatProps();
    }

    public interface IArmor : IEquipment
    {
    }

    public interface IShoes : IEquipment
    {

    }
    public interface IDecoration : IEquipment
    {
    }
    public interface IWeapon : IEquipment
    {
        WeaponArmed Armed { get; }
    }
}
