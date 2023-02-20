using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using NameM;
using Server.Configs.Characters;
using Server.Configs.Factions;
using Utls;

namespace Server.Controllers
{
    public class RecruitController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private GradeConfigSo GradeConfig => RecruitCfg.GradeCfg;
        private RecruitConfigSo RecruitConfig => RecruitCfg.RecruitCfg;
        private List<Dizi> TempDiziList { get; } = new List<Dizi>();
        private Config.Recruit RecruitCfg => Game.Config.RecruitCfg;

        public bool GenerateDizi()
        {
            var cost = RecruitConfig.GetCost();
            var yuanBaoCost = cost.yuanBao;
            var silverCost = cost.silver;
            if (Faction.Silver < silverCost)
            {
                XDebug.Log($"银两不足 = {silverCost}, 不可招募弟子!");
                return false;
            }
            if (Faction.YuanBao < yuanBaoCost)
            {
                XDebug.Log($"元宝不足 = {yuanBaoCost}, 不可招募弟子!");
                return false;
            }
            Faction.AddYuanBao(-yuanBaoCost);

            var (name, gender) = NameGen.GenNameWithGender();
            var allGrades = Enum.GetValues(typeof(GradeConfigSo.Grades)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(allGrades.Length);
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(randomGrade);
            var combatSkill = GradeConfig.GenerateCombatSkill(randomGrade);
            var forceSkill = GradeConfig.GenerateForceSkill(randomGrade);
            var dodgeSkill = GradeConfig.GenerateDodgeSkill(randomGrade);
            var diziIndex = TempDiziList.Count;
            var capable = new Capable(randomGrade, 1, 1, bag, strength, agility, hp, mp, 50, 50, 50, 50, 50);
            var guid = Guid.NewGuid().ToString();
            var dizi = new Dizi(guid: guid, name: name.Text, gender: gender, level: 1, stamina: stamina,
                capable: capable, combatSkill: combatSkill.GetFromLevel(1),
                forceSkill: forceSkill.GetFromLevel(1), dodgeSkill: dodgeSkill.GetFromLevel(1));
            TempDiziList.Add(dizi);
            var list = new List<int> { diziIndex };
            Game.MessagingManager.Send(EventString.Recruit_DiziGenerated, dizi.Name);
            Game.MessagingManager.Send(EventString.Recruit_DiziInSlot, list);
            return true;
        }

        public bool RecruitDizi(int index)
        {
            var dizi = TempDiziList[index];
            if (dizi == null)
            {
                XDebug.Log($"找不到弟子! index = {index}!");
                return false;
            }
            Faction.AddDizi(dizi);
            XDebug.Log($"弟子:{dizi.Name} 加入门派!");
            return true;
        }
    }
}
