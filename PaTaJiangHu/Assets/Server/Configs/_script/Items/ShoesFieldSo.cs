using System;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "鞋子",menuName = "物件/鞋子")]
    [Serializable] internal class ShoesFieldSo : EquipmentSoBase,IShoes
    {
        public override EquipKinds EquipKind => EquipKinds.Shoes;

        public IShoes Instance() =>
            new ShoesField(Id, Name, Icon, About, Grade, Quality, GetAddOn, GetCombatSet);


        private class ShoesField : EquipmentBaseField,IShoes
        {
            public override EquipKinds EquipKind => EquipKinds.Armor;

            public ShoesField(int id, string name, Sprite icon, string about, ColorGrade grade, int quality,
                Func<DiziProps, float> getAddOnFunc, Func<ICombatSet> getCombatSetFunc) : base(id, name, icon,
                about, grade, quality, getAddOnFunc, getCombatSetFunc)
            {
            }
        }
    }
}