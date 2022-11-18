using BattleM;
using Data;
using MyBox;
using System.Linq;
using UnityEngine;

namespace So
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗测试/轻功")]
    internal class DodgeFieldSo : ScriptableObject, IDodgeSkill, IDataElement
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
        [SerializeField] private int 内消耗;
        [SerializeField] private int 使用息;
        [SerializeField] private int 身法值;
        [SerializeField] private CombatBuffSoBase[] _buffs;

        public int Id => id;
        public string Name => _name;
        public int DodgeMp => 内消耗;

        public int Breath => 使用息;
        public int Dodge => 身法值;
        public CombatBuffSoBase[] GetAllBuffs() => _buffs;

        public IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append) =>
            _buffs.GetSortedInstance(append, unit).ToArray();
    }
}