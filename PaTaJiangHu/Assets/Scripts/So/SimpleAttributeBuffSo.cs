using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "buffSo", menuName = "战斗测试/Buff")]
    [Serializable]
    public class SimpleAttributeBuffSo : CombatBuffSoBase
    {
        public enum Attributes
        {
            [InspectorName("力量")]Strength,
            [InspectorName("敏捷")]Agility,
            [InspectorName("招架")]Parry,
            [InspectorName("身法")]Dodge,
            [InspectorName("武器")]Weapon,
            [InspectorName("内劲")]ExtraMp,
        }
        [SerializeField] private AttributeField[] _fields;
        private Func<CombatUnit, float> _addStrength;
        private Func<CombatUnit, float> _addAgility;
        private Func<CombatUnit, float> _addParry;
        private Func<CombatUnit, float> _addDodge;
        private Func<CombatUnit, float> _addWeaponDamage;
        private Func<CombatUnit, float> _extraMpValue;
        private AttributeField[] Fields => _fields;
        public override Func<CombatUnit, float> AddStrength => _addStrength;
        public override Func<CombatUnit, float> AddAgility => _addAgility;
        public override Func<CombatUnit, float> AddParry => _addParry;
        public override Func<CombatUnit, float> AddDodge => _addDodge;
        public override Func<CombatUnit, float> AddWeaponDamage => _addWeaponDamage;
        public override Func<CombatUnit, float> ExtraMpValue => _extraMpValue;

        private static float GetAttribute(IEnumerable<AttributeField> fields, Attributes attribute) =>
            fields.Single(f => f.Attrib == attribute).Value;

        public override IBuffInstance InstanceBuff(CombatUnit unit)
        {
            _addStrength = Fields.Any(f => f.Attrib == Attributes.Strength)
                ? _ => GetAttribute(Fields, Attributes.Strength)
                : default;
            _addAgility = Fields.Any(f => f.Attrib == Attributes.Agility)
                ? _ => GetAttribute(Fields, Attributes.Agility)
                : default;
            _addParry = Fields.Any(f => f.Attrib == Attributes.Parry)
                ? _ => GetAttribute(Fields, Attributes.Parry)
                : default;
            _addDodge = Fields.Any(f => f.Attrib == Attributes.Dodge)
                ? _ => GetAttribute(Fields, Attributes.Dodge)
                : default;
            _addWeaponDamage = Fields.Any(f => f.Attrib == Attributes.Weapon)
                ? _ => GetAttribute(Fields, Attributes.Weapon)
                : default;
            _extraMpValue = Fields.Any(f => f.Attrib == Attributes.ExtraMp)
                ? _ => GetAttribute(Fields, Attributes.ExtraMp)
                : default;
            return new CombatBuff(combatId: unit.CombatId,
                lasting: Lasting, spriteId: SpriteId,
                stacks: Stacks, consumption: Consumption,
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
            [SerializeField] private Attributes 属性;
            [SerializeField] private int 值;
            public Attributes Attrib => 属性;
            public int Value => 值;
        }
    }
}