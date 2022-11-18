using System;
using System.Linq;
using BattleM;
using Data;
using MyBox;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗测试/内功")]
    internal class ForceFieldSo : ScriptableObject, IForceSkill, ISkillForm, IDataElement
    {
        #region ReferenceSo
        private bool ReferenceSo()
        {
            _so = this;
            return true;
        }
        [ConditionalField(true, nameof(ReferenceSo))][ReadOnly][SerializeField] private ScriptableObject _so;
        #endregion
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 息;
        [SerializeField] private int 气血恢复量;
        [SerializeField] private int 内功转化;
        [SerializeField] private int 蓄转内;
        [SerializeField] private int 护甲值;
        [SerializeField] private int 护甲消耗;
        [SerializeField] private CombatBuffSoBase[] _buffs;

        public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) =>
            _buffs.GetSortedInstance(append, unit).ToArray();

        public int Id => id;
        public string Name => _name;
        public int ForceRate => 内功转化;
        public int MpCharge => 蓄转内;
        public int Recover => 气血恢复量;
        public int ArmorCost => 护甲消耗;
        public int Armor => 护甲值;
        public int Breath => 息;

        public CombatBuffSoBase[] GetAllBuffs() => _buffs;
    }

}