using System;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "挂件",menuName = "物件/挂件")]
    [Serializable] internal class DecorationFieldSo : EquipmentSoBase,IDecoration
    {
        public override EquipKinds EquipKind => EquipKinds.Decoration;

        public IDecoration Instance() =>
            new DecorationField(Id, Name, Icon, About, Grade, Quality, GetAddOn, GetCombatSet);


        private class DecorationField : EquipmentBaseField,IDecoration
        {
            public override EquipKinds EquipKind => EquipKinds.Decoration;

            public DecorationField(int id, string name, Sprite icon, string about, ColorGrade grade, int quality,
                Func<DiziProps, float> getAddOnFunc, Func<ICombatSet> getCombatSetFunc) : base(id, name, icon,
                about, grade, quality, getAddOnFunc, getCombatSetFunc)
            {
            }
        }
    }
}