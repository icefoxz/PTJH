using System;
using BattleM;
using Data;
using Server.Configs._script.Skills;
using UnityEngine;

namespace Server.Configs._script.Items
{
    [CreateAssetMenu(fileName = "armorSo",menuName = "战斗测试/防具")]
    [Serializable] public class ArmorFieldSo : ScriptableObject, IArmor,IDataElement
    {

        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 护甲;
        [SerializeField] private SkillGrades _grade;

        public int Id => id;
        public string Name => _name;
        public int Def => 护甲;
        public SkillGrades Grade => _grade;
    }
}