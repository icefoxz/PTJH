using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using NameM;
using Server.Controllers.Characters;
using UnityEngine;
using Utls;

namespace Server.Controllers.Factions
{
    public class DiziRecruitController : IGameController
    {
        private GradeConfigSo GradeConfig { get; }
        private List<Dizi> TempDiziList { get; } = new List<Dizi>();
        internal DiziRecruitController(GradeConfigSo gradeConfig)
        {
            GradeConfig = gradeConfig;
        }

        public void GenerateDizi()
        {
            var name = NameGen.GenName();
            var allGrades = Enum.GetValues(typeof(GradeConfigSo.Grades)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(allGrades.Length);
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(randomGrade);
            
            var diziIndex = TempDiziList.Count;
            var dizi = new Dizi(name.Text, strength, agility, hp, mp, 1, randomGrade, stamina, bag, 1, 1, 1);
            TempDiziList.Add(dizi);
            var list = new List<int> { diziIndex };
            Game.MessagingManager.Invoke(EventString.Recruit_DiziGenerated, new DiziInfo(dizi));
            Game.MessagingManager.Invoke(EventString.Recruit_DiziInSlot, list);
        }

        public void RecruitDizi(int index)
        {
            var dizi = TempDiziList[index];
            Game.World.Faction.AddDizi(dizi);
            Game.MessagingManager.Invoke(EventString.Faction_DiziAdd, new DiziInfo(dizi));
            Debug.Log($"弟子:{dizi.Name} 加入门派!");
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
