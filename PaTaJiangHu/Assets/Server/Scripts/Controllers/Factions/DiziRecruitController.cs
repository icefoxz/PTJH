using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using NameM;
using Server.Controllers.Characters;
using Utls;

namespace Server.Controllers.Factions
{
    internal class DiziRecruitController : IGameController
    {
        private GradeConfigSo GradeConfig { get; }
        private List<Dizi> DiziList { get; } = new List<Dizi>();
        public DiziRecruitController(GradeConfigSo gradeConfig)
        {
            GradeConfig = gradeConfig;
        }

        public void GenerateDizi()
        {
            var name = NameGen.GenName();
            var grades = Enum.GetValues(typeof(GradeConfigSo.Grades)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(grades.Length);
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(randomGrade);
            var diziIndex = DiziList.Count;
            var dizi = new Dizi(name.Text, strength, agility, hp, mp, 1, randomGrade, stamina, bag, 1, 1, 1);
            DiziList.Add(dizi);
            Game.MessagingManager.Invoke(EventString.Recruit_DiziGenerated, new DiziInfo(dizi));
            Game.MessagingManager.Invoke(EventString.Recruit_DiziInSlot, diziIndex.ToString());
        }

        public void RecruitDizi(int index)
        {
            var dizi = DiziList[index];
            Game.World.Faction.AddDizi(dizi);
            Game.MessagingManager.Invoke(EventString.Faction_DiziAdd, new DiziInfo(dizi));
        }

        public class DiziInfo
        {
            public string Name { get; set; }

            public DiziInfo() { }
            public DiziInfo(Dizi d)
            {
                Name = d.Name;
            }
        }
    }
}
