using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using MyBox;
using UnityEngine;

namespace Server.Configs._script.Battles
{
    [CreateAssetMenu(fileName = "buffSo", menuName = "战斗测试/Buff")]
    [Serializable]
    public class SimplePropBuffSo : CombatBuffSoBase
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }
        [ConditionalField(true, nameof(ReferenceSo))][ReadOnly][SerializeField] private ScriptableObject _so;
        #endregion

        public enum Attributes
        {
            [InspectorName("力量")]Strength,
            [InspectorName("敏捷")]Agility,
            [InspectorName("招架")]Parry,
            [InspectorName("身法")]Dodge,
            [InspectorName("武器")]Weapon,
            [InspectorName("内劲")]ExtraMp,
        }
        [Header("属性类型设定每种只能有一个，否则会报错！")]
        [SerializeField] private AttributeField[] _fields;
        private Func<ICombatUnit, float> _addStrength;
        private Func<ICombatUnit, float> _addAgility;
        private Func<ICombatUnit, float> _addParry;
        private Func<ICombatUnit, float> _addDodge;
        private Func<ICombatUnit, float> _addWeaponDamage;
        private Func<ICombatUnit, float> _extraMpValue;
        private AttributeField[] Fields => _fields;
        public override Func<ICombatUnit, float> AddStrength => _addStrength;
        public override Func<ICombatUnit, float> AddAgility => _addAgility;
        public override Func<ICombatUnit, float> AddParry => _addParry;
        public override Func<ICombatUnit, float> AddDodge => _addDodge;
        public override Func<ICombatUnit, float> AddWeaponDamage => _addWeaponDamage;
        public override Func<ICombatUnit, float> ExtraMpValue => _extraMpValue;

        private static float GetAttribute(ICombatUnit unit, IEnumerable<AttributeField> fields,
            Attributes attribute)
        {
            //SimpleProp不允许同时设定多个相同类型。如果设了多个相同类型将会报错
            var value = fields.Single(f => f.Attrib == attribute).Value;
            var result = value * 1f;
            if (value < 0) result = value * -0.01f * GetUnitValue(attribute, unit);
            return result;

            float GetUnitValue(Attributes att, ICombatUnit cu)
            {
                return att switch
                {
                    Attributes.Strength => cu.Strength,
                    Attributes.Agility => cu.Agility,
                    Attributes.Parry => cu.BreathBar.Perform.CombatForm.Parry,
                    Attributes.Dodge => cu.BreathBar.Perform.DodgeSkill.Dodge,
                    Attributes.Weapon => cu.WeaponDamage,
                    Attributes.ExtraMp => cu.Status.Mp.Max,
                    _ => throw new ArgumentOutOfRangeException(nameof(att), att, null)
                };
            }
        }


        public override IBuffInstance InstanceBuff(ICombatUnit unit)
        {
            //实现所有基本值增强功能
            _addStrength = Fields.Any(f => f.Attrib == Attributes.Strength)
                ? _ => GetAttribute(unit, Fields, Attributes.Strength)
                : default;
            _addAgility = Fields.Any(f => f.Attrib == Attributes.Agility)
                ? _ => GetAttribute(unit, Fields, Attributes.Agility)
                : default;
            _addParry = Fields.Any(f => f.Attrib == Attributes.Parry)
                ? _ => GetAttribute(unit, Fields, Attributes.Parry)
                : default;
            _addDodge = Fields.Any(f => f.Attrib == Attributes.Dodge)
                ? _ => GetAttribute(unit, Fields, Attributes.Dodge)
                : default;
            _addWeaponDamage = Fields.Any(f => f.Attrib == Attributes.Weapon)
                ? _ => GetAttribute(unit, Fields, Attributes.Weapon)
                : default;
            _extraMpValue = Fields.Any(f => f.Attrib == Attributes.ExtraMp)
                ? _ => GetAttribute(unit, Fields, Attributes.ExtraMp)
                : default;
            return new CombatBuff(combatId: unit.CombatId,
                lasting: Lasting, spriteId: SpriteId,
                stacks: Stacks, consumption: Consumption,
                append: Append,
                addStrength: AddStrength,
                addAgility: AddAgility,
                addParry: AddParry,
                addDodge: AddDodge,
                addWeaponDamage: AddWeaponDamage,
                extraMpValue: ExtraMpValue,
                onRoundStart: RoundStart, onRoundEnd: RoundEnd);
        }

        [Serializable] private class AttributeField
        {
            [ConditionalField(true,nameof(RenameElement))][ReadOnly] [SerializeField] private string _name;
            [SerializeField] private Attributes 属性;
            [SerializeField] private int 值;
            public Attributes Attrib => 属性;
            public int Value => 值;

            private bool RenameElement()
            {
                var att = Attrib switch
                {
                    Attributes.Strength => "力量",
                    Attributes.Agility => "敏捷",
                    Attributes.Parry => "招架",
                    Attributes.Dodge => "身法",
                    Attributes.Weapon => "武器",
                    Attributes.ExtraMp => "内劲",
                    _ => throw new ArgumentOutOfRangeException()
                };
                var valueText = Value switch
                {
                    < 0 => $"{-Value}%",
                    > 0 => Value.ToString(),
                    _ => "无效"
                };
                _name = $"{att}:" + valueText;
                return true;
            }
        }
    }
}