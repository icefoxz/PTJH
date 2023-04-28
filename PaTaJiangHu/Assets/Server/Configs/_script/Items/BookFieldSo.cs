using System;
using Core;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;

namespace Server.Configs.Items
{
    public interface IBook : IGameItem
    {
        ColorGrade Grade { get; }
        ISkillLevelMap GetLevelMap(int nextLevel);
        ISkill GetSkill();
    }

    [CreateAssetMenu(fileName = "id_秘籍名字",menuName = "物件/秘籍")]
    internal class BookFieldSo : BookSoBase
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
        /// 获取升级概率, -1表示无法升级
        /// </summary>
        /// <param name="nextLevel"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override ISkillLevelMap GetLevelMap(int nextLevel)
        {
            if (nextLevel <= 1)
                throw new NotImplementedException("等级不可为<=1");
            if (nextLevel > SkillFieldSo.MaxLevel())
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
        public abstract ISkillLevelMap GetLevelMap(int nextLevel);
        public abstract ISkill GetSkill();
    }
}