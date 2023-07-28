using System;
using AOT.Core.Dizi;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    [CreateAssetMenu(fileName = "挂件",menuName = "物件/挂件")]
    [Serializable] public class DecorationFieldSo : EquipmentSoBase,IDecoration
    {
        public override EquipKinds EquipKind => EquipKinds.Decoration;

        public IDecoration Instance() =>
            new DecorationField(Id, Name, Icon, About, Grade, Quality, GetAddOn, GetCombatSet);


        private record DecorationField : EquipmentBaseField,IDecoration
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