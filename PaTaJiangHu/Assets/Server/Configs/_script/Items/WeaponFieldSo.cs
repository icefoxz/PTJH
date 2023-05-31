using System;
using System.Linq;
using Core;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "武器", menuName = "物件/武器")]
    [Serializable] internal class WeaponFieldSo : EquipmentSoBase,IWeapon
    {
        [SerializeField] private WeaponArmed 类型;

        public WeaponArmed Armed => 类型;
        public override EquipKinds EquipKind => EquipKinds.Weapon;
        public IWeapon Instance() =>
            new WeaponField(Id, Name, Armed, Icon, About, Grade, Quality, GetAddOn, GetCombatSet);
        private class WeaponField : EquipmentBaseField, IWeapon
        {
            public WeaponArmed Armed { get; }
            public override EquipKinds EquipKind => EquipKinds.Weapon;

            public WeaponField(int id, string name, WeaponArmed armed, Sprite icon, string about, ColorGrade grade,
                int quality, Func<DiziProps, float> getAddOnFunc, Func<ICombatSet> getCombatSetFunc) : base(id, name, icon,
                about, grade, quality, getAddOnFunc, getCombatSetFunc)
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
        [SerializeField] private int 韧性;
        [SerializeField] private int 等级;
        [FormerlySerializedAs("加成")][SerializeField] private DiziPropAddOn[] 属性;
        [SerializeField] private CombatAdvancePropField[] 高级属性;
        public Sprite Icon => 图标;
        public string About => 说明;
        public ItemType Type => ItemType.Equipment;
        public abstract EquipKinds EquipKind { get; }
        public ColorGrade Grade => 品级;
        public int Quality => 韧性;
        public int Level => 等级;

        private DiziPropAddOn[] AddOns => 属性;
        private CombatAdvancePropField[] AdvanceProps => 高级属性;

        [Serializable]
        internal class DiziPropAddOn
        {
            private bool SetName()
            {
                _name = $"【{PropText()}】{AddOnText()}";
                return true;
            }

            private string AddOnText()=> AddOn switch
            {
                > 0 => $"+{AddOn}",
                < 0 => $"-{AddOn}",
                _ => AddOn.ToString()
            };

            private string PropText()
            {
                return Prop switch
                {
                    DiziProps.Strength => "力量",
                    DiziProps.Agility => "敏捷",
                    DiziProps.Hp => "血量",
                    DiziProps.Mp => "内力",
                    _ => "未设"
                };
            }

            [ConditionalField(true, nameof(SetName))][SerializeField][ReadOnly] private string _name;
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

        public ICombatSet GetCombatSet() => AdvanceProps.Select(a=>a.GetCombatSet()).Combine();

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
            private event Func<ICombatSet> GetCombatSetFunc;
            public float GetAddOn(DiziProps prop) => GetAddOnFunc?.Invoke(prop) ?? 0;
            public ICombatSet GetCombatSet() => GetCombatSetFunc?.Invoke() ?? CombatSet.Empty;

            protected EquipmentBaseField(int id, string name, Sprite icon, string about, ColorGrade grade, int quality,
                Func<DiziProps, float> getAddOnFunc, Func<ICombatSet> getCombatSetFunc)
            {
                Id = id;
                Icon = icon;
                Name = name;
                About = about;
                Grade = grade;
                Quality = quality;
                GetAddOnFunc = getAddOnFunc;
                GetCombatSetFunc = getCombatSetFunc;
            }
        }
    }
}