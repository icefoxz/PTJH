using System.Linq;
using Data;
using MyBox;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Battles
{
    [CreateAssetMenu(fileName = "forceSo", menuName = "战斗测试/内功")]
    internal class ForceFieldSo : ScriptableObject, IDataElement
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
        [SerializeField] private SkillGrades _grade;
        [SerializeField] private ForceLevelingSo 等级配置;

        public int Id => id;
        public string Name => _name;
        public int ForceRate => 内功转化;
        public int MpCharge => 蓄转内;
        public int Recover => 气血恢复量;
        public int ArmorCost => 护甲消耗;
        public int Armor => 护甲值;
        public int Breath => 息;
        public SkillGrades Grade => _grade;

        public ForceLevelingSo LevelingSo => 等级配置;
    }

}