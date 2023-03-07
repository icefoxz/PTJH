using Data;
using MyBox;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "combatSo", menuName = "战斗测试/武功")]
    internal class CombatFieldSo : ScriptableObject, IDataElement//,ILeveling<ICombatSkill>
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
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private WeaponArmed 类型;

        public int Id => id;
        public string Name => _name;
        public WeaponArmed Armed => 类型;

        private SkillGrades Grade => _grade;
    }
}