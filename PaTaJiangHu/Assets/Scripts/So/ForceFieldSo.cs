using System;
using System.Collections.Generic;
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
        [SerializeField] private int 内转化率;
        [SerializeField] private int 内护甲;
        [SerializeField] private int 使用息;

        public int Id => id;
        public string Name => _name;
        public int MpRate => 内转化率;
        public int MpArmor => 内护甲;
        public int Breath => 使用息;
    }
}