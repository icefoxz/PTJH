using System;
using System.Collections.Generic;
using System.Linq;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.Modules.NameM;
using GameClient.SoScripts;
using GameClient.SoScripts.Characters;
using GameClient.SoScripts.Factions;
using GameClient.System;

namespace GameClient.Controllers
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
            var allGrades = Enum.GetValues(typeof(ColorGrade)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(allGrades.Length);
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(randomGrade);
            var cr = GradeConfig.GetRandomConsumeResource(randomGrade).ToDictionary(r=>r.Item1,r=>r.Item2);
            var gifted = GradeConfig.GenerateGifted(randomGrade);
            var aptitude = GradeConfig.GenerateArmedAptitude(randomGrade);
            //var combatSkill = GradeConfig.GenerateCombatSkill(randomGrade);
            //var forceSkill = GradeConfig.GenerateForceSkill(randomGrade);
            //var dodgeSkill = GradeConfig.GenerateDodgeSkill(randomGrade);
            var diziIndex = TempDiziList.Count;
            var capable = new Capable(grade: randomGrade,
                combatSlot: 2, forceSlot: 2, dodgeSlot: 2, bag: bag,
                strength: strength, agility: agility, hp: hp, mp: mp,
                food: cr[ConsumeResources.Food], wine: cr[ConsumeResources.Wine],
                herb: cr[ConsumeResources.Herb], pill: cr[ConsumeResources.Pill]);
            var guid = Guid.NewGuid().ToString();
            var dizi = new Dizi(guid: guid, name: name.Text, gender: gender, level: 1, stamina: stamina,
                capable: capable, gifted, aptitude);
            dizi.SetSkill(DiziSkill.Instance(
                (GradeConfig.GenerateCombatSkill(randomGrade), 1),
                (GradeConfig.GenerateForceSkill(randomGrade), 1),
                (GradeConfig.GenerateDodgeSkill(randomGrade), 1)));
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
            dizi.StartIdle(SysTime.UnixNow);
            XDebug.Log($"弟子:{dizi.Name} 加入门派!");
            return true;
        }
    }
}
