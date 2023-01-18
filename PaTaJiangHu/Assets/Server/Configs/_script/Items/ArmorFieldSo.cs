using System;
using BattleM;
using Core;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "armorSo",menuName = "物件/弟子/防具")]
    [Serializable] public class ArmorFieldSo : AutoUnderscoreNamingObject
    {
        [SerializeField] private int 护甲;
        [SerializeField] private SkillGrades 品级;
        [SerializeField] private int 价钱;
        [SerializeField][TextArea] private string 说明;

        public int Def => 护甲;
        public SkillGrades Grade => 品级;
        public int Price => 价钱;
        public string About => 说明;
        public IArmor Instance() => new ArmorField(Id, Name, Def, Grade, Price, About);

        private class ArmorField : IArmor
        {
            public int Id { get; }
            public string Name { get; }
            public string About { get; }
            public ItemType Type => ItemType.Equipment;
            public int Def { get; }
            public SkillGrades Grade { get; }
            public int Price { get; }

            public ArmorField(int id, string name, int def, SkillGrades grade, int price, string about)
            {
                Id = id;
                Name = name;
                Def = def;
                Grade = grade;
                Price = price;
                About = about;
            }
        }
    }
}