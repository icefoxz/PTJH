using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AOT.Core.Dizi;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Items;
using UnityEngine;

namespace GameClient.SoScripts.Skills
{
    
    [CreateAssetMenu(fileName = "combatSo", menuName = "战斗/武学/武功")]
    public class CombatFieldSo : SkillFieldSo ,ICombatSkill
    {
        [SerializeField] private WeaponArmed 类型;
        public WeaponArmed Armed => 类型;
        public override SkillType SkillType => SkillType.Combat;
    }

    public abstract class SkillFieldSo : AutoUnderscoreNamingObject, ISkill
    {
        [SerializeField] private bool 打开测试;
        public bool TestMode => 打开测试;
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private Sprite 图标;
        [SerializeField] private BookSoBase 秘籍;
        [SerializeField] private SkillLevelStrategySo 等级策略;
        [SerializeField] [TextArea] private string 描述;
        public abstract SkillType SkillType { get; }
        public ColorGrade Grade => 品级;
        public Sprite Icon => 图标;
        public string About => 描述;
        public IBook Book => 秘籍;

        private SkillLevelStrategySo LevelStrategy => 等级策略;

        protected virtual IList<ICombatSet> AdditionCombatSets()
        {
            CheckThis();
            return Array.Empty<ICombatSet>();
        }

        public ICombatSet GetCombatSet(int level)
        {
            CheckThis();
            var list = AdditionCombatSets().ToList();
            list.Add(LevelStrategy.GetCombatSet(level));
            return list.Combine();
        }

        public ISkillAttribute[] GetAttributes(int level)
        {
            CheckThis();
            return LevelStrategy.GetAttributes(level);
        }

        public ISkillProp[] GetProps(int level)
        {
            CheckThis();
            return LevelStrategy.GetProps(level);
        }

        public int MaxLevel
        {
            get
            {
                CheckThis();
                return LevelStrategy.MaxLevel();
            }
        }

        private void CheckThis([CallerMemberName] string methodName = null)
        {
            if (this == null) throw new NullReferenceException($"技能异常!{methodName}方法调用了null的技能!");
        }
    }
}