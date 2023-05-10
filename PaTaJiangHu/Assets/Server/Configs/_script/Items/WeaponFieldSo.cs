using System;
using Core;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "武器", menuName = "物件/武器")]
    [Serializable] internal class WeaponFieldSo : EquipmentSoBase,IWeapon
    {
        [SerializeField] private WeaponArmed 类型;

        public WeaponArmed Armed => 类型;
        public override EquipKinds EquipKind => EquipKinds.Weapon;
        public IWeapon Instance() =>
            new WeaponField(Id, Name, Armed, Icon, About, Grade, Quality, GetAddOn, GetCombatProps);
        private class WeaponField : EquipmentBaseField, IWeapon
        {
            public WeaponArmed Armed { get; }
            public override EquipKinds EquipKind => EquipKinds.Weapon;

            public WeaponField(int id, string name, WeaponArmed armed, Sprite icon, string about, ColorGrade grade,
                int quality, Func<DiziProps, float> getAddOnFunc, Func<ICombatProps> getCombatPropsFunc) : base(id, name, icon,
                about, grade, quality, getAddOnFunc, getCombatPropsFunc)
            {
                Armed = armed;
            }
        }
    }

    internal abstract class EquipmentSoBase : AutoUnderscoreNamingObject, IEquipment
    {
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private Sprite 图标;
        [SerializeField] [TextArea] private string 说明;
        [SerializeField] private DiziPropAddOn[] 加成;
        [SerializeField] private int 韧性;
        public Sprite Icon => 图标;
        public string About => 说明;
        public ItemType Type => ItemType.Equipment;
        public abstract EquipKinds EquipKind { get; }
        public ColorGrade Grade => 品级;
        public int Quality => 韧性;
        private DiziPropAddOn[] AddOns => 加成;

        [Serializable]
        protected class DiziPropAddOn
        {
            [SerializeField] private DiziProps 属性;
            [SerializeField] private float 加成;
            public DiziProps Prop => 属性;
            public float AddOn => 加成;
        }

        public float GetAddOn(DiziProps prop)
        {
            var value = 0f;
            for (var i = 0; i < AddOns.Length; i++)
            {
                var addOn = AddOns[i];
                if (addOn.Prop == prop) value += addOn.AddOn;
            }

            return value;
        }
        public ICombatProps GetCombatProps()
        {
            (float str, float agi, float hp, float mp) tuple = (0, 0, 0, 0);
            for (var i = 0; i < AddOns.Length; i++)
            {
                var addOn = AddOns[i];
                switch (addOn.Prop)
                {
                    case DiziProps.Strength:
                        tuple.str += addOn.AddOn;
                        break;
                    case DiziProps.Agility:
                        tuple.agi += addOn.AddOn;
                        break;
                    case DiziProps.Hp:
                        tuple.hp += addOn.AddOn;
                        break;
                    case DiziProps.Mp:
                        tuple.mp += addOn.AddOn;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new CombatProps(tuple.str, tuple.agi, tuple.hp, tuple.mp);
        }

        protected abstract class EquipmentBaseField : IEquipment
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.Equipment;
            public abstract EquipKinds EquipKind { get; }
            public ColorGrade Grade { get; }
            public int Quality { get; }
            private event Func<DiziProps, float> GetAddOnFunc;
            private event Func<ICombatProps> GetCombatPropsFunc;
            public float GetAddOn(DiziProps prop) => GetAddOnFunc?.Invoke(prop) ?? 0;
            public ICombatProps GetCombatProps() => GetCombatPropsFunc?.Invoke();

            protected EquipmentBaseField(int id, string name, Sprite icon, string about, ColorGrade grade, int quality,
                Func<DiziProps, float> getAddOnFunc, Func<ICombatProps> getCombatPropsFunc)
            {
                Id = id;
                Icon = icon;
                Name = name;
                About = about;
                Grade = grade;
                Quality = quality;
                GetAddOnFunc = getAddOnFunc;
                GetCombatPropsFunc = getCombatPropsFunc;
            }
        }
    }
}