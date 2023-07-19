using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Core;
using AOT.Core.Dizi;
using AOT.Utls;
using GameClient.Models;
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
        private GradeConfigSo GradeConfig => Recruit.GradeCfg;
        private RecruitConfigSo RecruitConfig => Recruit.Config;
        private List<Dizi> TempDiziList { get; } = new List<Dizi>();
        private Config.Recruit Recruit => Game.Config.RecruitCfg;
        private Recruiter Recruiter => Game.World.Recruiter;
        
        public bool NewDizi()
        {
            var cost = RecruitConfig.GetCost();
            var yuanBaoCost = cost.yuanBao;
            var silverCost = cost.silver;
            if (!FactionTryTrade(silverCost, yuanBaoCost)) return false;
            var allGrades = Enum.GetValues(typeof(ColorGrade)).Cast<int>().ToArray();
            var randomGrade = Sys.Random.Next(allGrades.Length);
            var dizi = GenerateDizi(randomGrade);
            var diziIndex = TempDiziList.Count;
            TempDiziList.Add(dizi);
            var list = new List<int> { diziIndex };
            Game.MessagingManager.Send(EventString.Recruit_DiziGenerated, dizi.Name);
            Game.MessagingManager.Send(EventString.Recruit_DiziInSlot, list);
            return true;
        }

        private Dizi GenerateDizi(int grade)
        {
            var (name, gender) = NameGen.GenNameWithGender();
            var (strength, agility, hp, mp, stamina, bag) = GradeConfig.GenerateFromGrade(grade);
            var cr = GradeConfig.GetRandomConsumeResource(grade).ToDictionary(r => r.Item1, r => r.Item2);
            var gifted = GradeConfig.GenerateGifted(grade);
            var aptitude = GradeConfig.GenerateArmedAptitude(grade);
            //var combatSkill = GradeConfig.GenerateCombatSkill(randomGrade);
            //var forceSkill = GradeConfig.GenerateForceSkill(randomGrade);
            //var dodgeSkill = GradeConfig.GenerateDodgeSkill(randomGrade);
            var capable = new Capable(grade: grade,
                combatSlot: 2, forceSlot: 2, dodgeSlot: 2, bag: bag,
                strength: strength, agility: agility, hp: hp, mp: mp,
                food: cr[ConsumeResources.Food], wine: cr[ConsumeResources.Wine],
                herb: cr[ConsumeResources.Herb], pill: cr[ConsumeResources.Pill]);
            var guid = Guid.NewGuid().ToString();
            var dizi = new Dizi(guid: guid, name: name.Text, gender: gender, level: 1, stamina: stamina,
                capable: capable, gifted, aptitude);
            dizi.SetSkill(DiziSkill.Instance(
                (GradeConfig.GenerateCombatSkill(grade), 1),
                (GradeConfig.GenerateForceSkill(grade), 1),
                (GradeConfig.GenerateDodgeSkill(grade), 1)));
            return dizi;
        }

        public bool RecruitDizi(int index)
        {
            var dizi = TempDiziList[index];
            if (dizi == null)
            {
                XDebug.Log($"找不到弟子! index = {index}!");
                return false;
            }
            return Faction.TryAddDizi(dizi);
        }

        public (IVisitorDizi,Dizi) GenerateVisitor()
        {
            var visitor = Recruit.Config.GetRandomVisitorDizi();
            var dizi = GenerateDizi((int)visitor.Grade);
            return (visitor, dizi);
        }

        public bool BuyVisitor()
        {
            var visitor = Recruiter.CurrentVisitor;
            if (visitor == null)
                throw new NullReferenceException("找不到来访弟子!");
            var yuanBao = Recruit.Config.GetVisitorBuyCost(visitor.Set.Grade);
            if (!FactionTryTrade(0, yuanBao)) return false;
            if (!Faction.TryAddDizi(visitor.Dizi)) return false;
            Recruiter.RemoveVisitor();
            return true;
        }

        public void SellVisitor()
        {
            var visitor = Recruiter.CurrentVisitor;
            if(visitor == null)
                throw new NullReferenceException("找不到来访弟子!");
            var yuanBao = Recruit.Config.GetVisitorSellCost(visitor.Set.Grade);
            FactionTryTrade(0, yuanBao);
            Recruiter.RemoveVisitor();
        }
        private bool FactionTryTrade(int silver, int yuanBao)
        {
            if (Faction.Silver < silver)
            {
                SendEvent(EventString.Info_Trade_Failed_Silver, silver);
                return false;
            }

            if (Faction.YuanBao < yuanBao)
            {
                SendEvent(EventString.Info_Trade_Failed_YuanBao, silver);
                return false;
            }

            Faction.AddYuanBao(-yuanBao);
            Faction.AddSilver(-silver);
            return true;
        }

        private void SendEvent(string eventName, params object[] args) =>
            Game.MessagingManager.SendParams(eventName, args);
    }
}
