using System;
using Core;
using Server.Configs.Battles;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "armorSo",menuName = "物件/防具")]
    [Serializable] public class ArmorFieldSo : AutoUnderscoreNamingObject,IArmor
    {
        [FormerlySerializedAs("护甲")][SerializeField] private int 加血;
        [SerializeField] private DiziGrades 品级;
        [SerializeField][TextArea] private string 说明;
        [SerializeField] private Sprite 图标;

        public int AddHp => 加血;
        public EquipKinds EquipKind => EquipKinds.Armor;
        public DiziGrades Grade => 品级;
        public ItemType Type => ItemType.Equipment;
        public Sprite Icon => 图标;
        public string About => 说明;
        public IArmor Instance() => new ArmorField(Id, Name, AddHp, Grade, About, Icon);

        private class ArmorField : IArmor
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.Equipment;
            public int AddHp { get; }
            public EquipKinds EquipKind => EquipKinds.Armor;
            public DiziGrades Grade { get; }

            public ArmorField(int id, string name, int addHp, DiziGrades grade, string about, Sprite icon)
            {
                Id = id;
                Name = name;
                AddHp = addHp;
                Grade = grade;
                About = about;
                Icon = icon;
            }
        }
    }
}