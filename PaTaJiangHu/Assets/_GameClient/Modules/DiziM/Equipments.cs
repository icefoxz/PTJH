using AOT._AOT.Core;
using GameClient.Args;
using GameClient.Modules.BattleM;
using UnityEngine;

namespace GameClient.Modules.DiziM
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

    public interface IDiziEquipment
    {
        IWeapon Weapon { get; }
        IArmor Armor { get; }
        IShoes Shoes { get; }
        IDecoration Decoration { get; }
        float GetPropAddon(DiziProps prop);
        /// <summary>
        /// 战斗高级属性
        /// </summary>
        /// <returns></returns>
        ICombatSet GetCombatSet();
    }


    public interface IAdvPackage
    {
        int Grade { get; }
        IStacking<IGameItem>[] AllItems { get; }
        IAdvResPackage Package { get; }
    }

    public interface IAdvResPackage
    {
        int Food { get; }
        int Wine { get; }
        int Herb { get; }
        int Pill { get; }
        int Silver { get; }
        int YuanBao { get; }
    }
    /// <summary>
    /// 主要的游戏奖励, 可以用于宝箱接口
    /// </summary>
    public interface IGameReward
    {
        IAdvPackage[] Packages { get; }
        IStacking<IGameItem>[] AllItems { get; }
    }

    public interface IRewardHandler
    {
        void SetReward(IGameReward reward);
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

    public interface ISingleCombatNpc : ICombatNpc
    {
        string Faction { get; }
        IGameReward Reward { get; }
    }

    /// <summary>
    /// Npc基本信息类
    /// </summary>
    public interface INpc
    {
        string Name { get; }
        int Level { get; }
        Sprite Icon { get; }
    }
    /// <summary>
    /// 关卡npc类
    /// </summary>
    public interface ICombatNpc : INpc
    {
        bool IsBoss { get; }
        DiziCombatUnit GetDiziCombat();
        IDiziReward DiziReward { get; }
        int Hp { get; }
        int Mp { get; }
        int Strength { get; }
        int Agility { get; }
        IDiziEquipment Equipment { get; }
        ICombatSet GetCombatSet();
        ISkillMap<ISkillInfo> ForceSkillInfo { get; }
        ISkillMap<ICombatSkillInfo> CombatSkillInfo { get; }
        ISkillMap<ISkillInfo> DodgeSkillInfo { get; }
    }
    //对单个弟子的奖励
    public interface IDiziReward
    {
        int Exp { get; }
    }
}
