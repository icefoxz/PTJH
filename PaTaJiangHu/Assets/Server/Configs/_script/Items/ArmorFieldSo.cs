using System;
using BattleM;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    [CreateAssetMenu(fileName = "armorSo",menuName = "战斗测试/防具")]
    [Serializable] public class ArmorFieldSo : AutoUnderscoreNamingObject
    {
        [SerializeField] private int 护甲;
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private int _price;

        public int Def => 护甲;
        public SkillGrades Grade => _grade;
        public int Price => _price;

        public IArmor Instance() => new ArmorField(Id, Name, Def, Grade, Price);

        private class ArmorField : IArmor
        {
            public int Id { get; }
            public string Name { get; }
            public int Def { get; }
            public SkillGrades Grade { get; }
            public int Price { get; }

            public ArmorField(int id, string name, int def, SkillGrades grade, int price)
            {
                Id = id;
                Name = name;
                Def = def;
                Grade = grade;
                Price = price;
            }
        }
    }
}