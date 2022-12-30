using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using NameM;
using Server.Controllers.Characters;
using Utls;

namespace Server.Controllers.Factions
{
    public class RecruitController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private GradeConfigSo GradeConfig { get; }
        private RecruitConfigSo RecruitConfig { get; }
        private List<Dizi> TempDiziList { get; } = new List<Dizi>();
        internal RecruitController(Configure.RecruitConfigure cfg)
        {
            GradeConfig = cfg.GradeConfig;
            RecruitConfig = cfg.RecruitConfig;
        }

        public bool GenerateDizi()
        {
            var cost = RecruitConfig.GetCost();
            var yuanBaoCost = cost.yuanBao;
            if (Faction.YuanBao < yuanBaoCost)
            {
                XDebug.Log($"元宝不足 = {yuanBaoCost}, 不可招募弟子!");
                return false;
            }
            Faction.AddYuanBao(-yuanBaoCost);

            var name = NameGen.GenName();
            var allGrades = Enum.GetValues(typeof(GradeConfigSo.Grades)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(allGrades.Length);
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(randomGrade);
            
            var diziIndex = TempDiziList.Count;
            var dizi = new Dizi(name.Text, strength, agility, hp, mp, 1, randomGrade, stamina,
                bag, 1, 1, 1);
            TempDiziList.Add(dizi);
            var list = new List<int> { diziIndex };
            Game.MessagingManager.Send(EventString.Recruit_DiziGenerated, new DiziInfo(dizi));
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
