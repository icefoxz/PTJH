using System;
using AOT.Core.Dizi;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    [CreateAssetMenu(fileName = "防具",menuName = "物件/防具")]
    [Serializable] public class ArmorFieldSo : EquipmentSoBase,IArmor
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