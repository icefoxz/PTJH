using System;
using BattleM;
using Data;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "armorSo",menuName = "战斗测试/防具")]
    [Serializable] public class ArmorFieldSo : ScriptableObject, IArmor,IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 护甲;

        public int Id => id;
        public string Name => _name;
        public int Def => 护甲;
    }
}