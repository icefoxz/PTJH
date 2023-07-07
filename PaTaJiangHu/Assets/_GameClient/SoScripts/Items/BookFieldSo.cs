using System;
using AOT._AOT.Core;
using GameClient.Modules.BattleM;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Skills;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    [CreateAssetMenu(fileName = "id_秘籍名字",menuName = "物件/秘籍")]
    public class BookFieldSo : BookSoBase
    {
        [SerializeField] private SkillFieldSo 武学;
        [SerializeField] private SkillLevelingSo 等级策略;
        [SerializeField] private ColorGrade 品级;
        [SerializeField] private Sprite 图片;
        [SerializeField][TextArea] private string 说明;
        public override int Id => id;
        public override string Name => _name;
        public override string About => 说明;
        public override ColorGrade Grade => 品级;
        public override Sprite Icon => 图片;

        internal SkillFieldSo SkillFieldSo => 武学;
        private SkillLevelingSo SkillLeveling => 等级策略;

        /// <summary>
        /// 获取升级设定
        /// </summary>
        /// <param name="nextLevel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override ISkillLevelConfig GetLevelMap(int nextLevel)
        {
            if (nextLevel < 1)
                throw new NotImplementedException("等级不可为<=1");
            if (nextLevel > SkillFieldSo.MaxLevel)
                throw new NotImplementedException("等级不可超过最大等级");
            return SkillLeveling.GetLevelMap(nextLevel);
        }

        public override ISkill GetSkill()=> SkillFieldSo;
    }

    public abstract class BookSoBase : AutoUnderscoreNamingObject, IBook
    {
        public abstract string About { get; }
        public ItemType Type => ItemType.Book;
        public abstract ColorGrade Grade { get; }
        public abstract Sprite Icon { get; }
        public abstract ISkillLevelConfig GetLevelMap(int nextLevel);
        public abstract ISkill GetSkill();
    }
}