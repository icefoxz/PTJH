using System;
using System.Linq;
using Core;
using Data;
using MyBox;
using Server.Configs.Battles;
using Server.Configs.Skills;
using UnityEngine;
using static Server.Configs.TestControllers.AdvMapController;

namespace Server.Configs.Items
{
    public interface IBook : IGameItem
    {
        ISkillLevelMap GetLevelMap(int nextLevel);
    }

    [CreateAssetMenu(fileName = "id_秘籍名字",menuName = "物件/弟子/秘籍")]
    internal class BookFieldSo : BookSoBase
    {
        private bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private BookFieldSo So;
        [SerializeField] private string _name;
        [SerializeField] private int id;
        [SerializeField] private int 价钱;
        [SerializeField] private SkillFieldSo 武学;
        [SerializeField]private SkillLevelingSo 等级策略;
        
        [SerializeField][TextArea] private string 说明;
        public override int Id => id;
        public override string Name => _name;
        public override string About => 说明;
        public override int Price => 价钱;
        public SkillFieldSo SkillFieldSo => 武学;
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
    }

    public abstract class BookSoBase : ScriptableObject, IBook
    {
        public abstract int Id { get; }
        public abstract string Name { get; }
        public abstract string About { get; }
        public ItemType Type => ItemType.Book;
        public abstract int Price { get; }
        public abstract ISkillLevelMap GetLevelMap(int nextLevel);
    }
}