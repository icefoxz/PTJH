using System;
using BattleM;
using Data;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗测试/内功")]
    [Serializable]
    public class ForceFieldSo : ScriptableObject, IForce, ISkillForm, IDataElement
    {
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 息;
        [SerializeField] private int 气血恢复量;
        [SerializeField] private int 内功转化;
        [SerializeField] private int 蓄转内;
        [SerializeField] private int 护甲值;
        [SerializeField] private int 护甲消耗;

        public int Id => id;
        public string Name => _name;
        public int ForceRate => 内功转化;
        public int MpConvert => 蓄转内;
        public int Recover => 气血恢复量;
        public int ArmorDepletion => 护甲消耗;
        public int Armor => 护甲值;
        public int Breath => 息;
    }
}