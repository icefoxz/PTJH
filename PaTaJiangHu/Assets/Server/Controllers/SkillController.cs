using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using Server.Configs.Items;
using Server.Configs.Skills;
using Utls;

namespace Server.Controllers
{
    public class SkillController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private DataController DataController => Game.Controllers.Get<DataController>();

        /// <summary>
        /// 领悟技能 
        /// </summary>
        /// <param name="diziGuid"></param>
        /// <param name="skillIndex"></param>
        /// <param name="comprehendItems"></param>
        /// <param name="skillType"></param>
        public void Comprehend(string diziGuid, SkillType skillType, int skillIndex,
            (int id, int amount)[] comprehendItems)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var skill = dizi.Skill.GetSkill(skillType, skillIndex);
            Comprehend(dizi.Guid, skill.Book.Id, comprehendItems);
        }

        /// <summary>
        /// 领悟技能
        /// </summary>
        /// <param name="diziGuid"></param>
        /// <param name="bookId"></param>
        /// <param name="comprehendItems"></param>
        public void Comprehend(string diziGuid , int bookId, (int id, int amount)[] comprehendItems)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var book = DataController.GetBook(bookId);
            var skill = book.GetSkill();
            var currentLevel = dizi.Skill.GetLevel(skill);
            var nextLevel = currentLevel + 1;
            var comprehend = book.GetLevelMap(nextLevel);
            var luck = Sys.Luck;
            var comprehendRate = comprehend.Rate + GetAddOnFrom(comprehendItems, nextLevel);
            if (comprehendRate > luck)
            {
                Game.MessagingManager.SendParams(dizi.Guid, EventString.Dizi_Skill_ComprehendFailed, "领悟失败");
                return;
            }
            dizi.SkillLevelUp(skill);
        }

        private float GetAddOnFrom((int id, int amount)[] arg,int level)
        {
            var addOn = 0f;
            var items = new List<(IComprehendItem item, int quantity)>();
            var invalidItems = items.Where(a => !a.item.IsLevelAvailable(level)).ToArray();
            if (invalidItems.Length > 0)
                throw new InvalidOperationException($"不合法物品: {string.Join(",", invalidItems.Select(a => $"{a.item.Id}.{a.item.Name}"))}");
            foreach (var (id, amount) in arg)
                items.Add(((IComprehendItem)DataController.GetFunctionItem(id), amount));
            return items.Sum(a => a.item.Rate * a.quantity);
        }

        /// <summary>
        /// 使用技能
        /// </summary>
        /// <param name="diziGuid"></param>
        /// <param name="skillType"></param>
        /// <param name="skillIndex"></param>
        public void UseSkill(string diziGuid, SkillType skillType, int skillIndex)
        {
            var dizi = Faction.GetDizi(diziGuid);
            dizi.UseSkill(skillType, skillIndex);
        }
    }
}