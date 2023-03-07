using System.Linq;
using Data;
using MyBox;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "dodgeSo", menuName = "战斗测试/轻功")]
    internal class DodgeFieldSo : ScriptableObject, IDataElement
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
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private DodgeLevelingSo 轻功等级配置;

        public int Id => id;
        public string Name => _name;
        public int DodgeMp => 内消耗;

        public int Breath => 使用息;
        public int Dodge => 身法值;
        public DodgeLevelingSo DodgeLevelingSo => 轻功等级配置;
        public SkillGrades Grade => _grade;
    }
}