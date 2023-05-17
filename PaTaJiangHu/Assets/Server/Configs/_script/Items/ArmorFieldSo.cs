using System;
using Core;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "防具",menuName = "物件/防具")]
    [Serializable] internal class ArmorFieldSo : EquipmentSoBase,IArmor
    {
        public override EquipKinds EquipKind => EquipKinds.Armor;

        public IArmor Instance() =>
            new ArmorField(Id, Name, Icon, About, Grade, Quality, GetAddOn, GetCombatSet);

        private class ArmorField : EquipmentBaseField, IArmor
        {
            public override EquipKinds EquipKind => EquipKinds.Armor;

            public ArmorField(int id, string name, Sprite icon, string about, ColorGrade grade, int quality,
                Func<DiziProps, float> getAddOnFunc, Func<ICombatSet> getCombatSetFunc) : base(id, name, icon,
                about, grade, quality, getAddOnFunc, getCombatSetFunc)
            {
            }
        }
    }
}