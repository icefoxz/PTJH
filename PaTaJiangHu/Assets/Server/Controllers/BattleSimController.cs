using System;
using System.Linq;
using Server.Configs.BattleSimulation;
using UnityEngine;

namespace Server.Controllers
{
    public interface IBattleSimController
    {
        void SimulateResult();
        void ForceLevelAdd(bool isPlayer);
        void CombatLevelAdd(bool isPlayer);
        void DodgeLevelAdd(bool isPlayer);
        void ForceLevelSub(bool isPlayer);
        void CombatLevelSub(bool isPlayer);
        void DodgeLevelSub(bool isPlayer);
        void ForceGradeAdd(bool isPlayer);
        void CombatGradeAdd(bool isPlayer);
        void DodgeGradeAdd(bool isPlayer);
        void ForceGradeSub(bool isPlayer);
        void CombatGradeSub(bool isPlayer);
        void DodgeGradeSub(bool isPlayer);
        void SetValue(bool isPlayer,BattleSimController.TestModel.Props prop,int value);
    }

    public class BattleSimController : IBattleSimController
    {
        private Configure Cfg { get; }
        private TestModel Model { get; }

        internal BattleSimController(Configure cfg)
        {
            Cfg = cfg;
            Model = new TestModel();
        }

        public void SimulateResult()
        {
            var outCome = Model.SimulateOutcome(Cfg.ConditionPropCfg, Cfg.BattleSimulatorCfg);
            Game.MessagingManager.Send(EventString.Test_SimulationOutcome, new Outcome(outCome,Cfg.BattleSimulatorCfg));
        }

        private PropCon GetCon(bool isPlayer) => isPlayer ? Model.Player : Model.Enemy;
        private void UpdateModel()
        {
            var pPower = GetSimulation(Model.Player);
            var ePower = GetSimulation(Model.Enemy);
            Model.Player.SetPower(pPower);
            Model.Enemy.SetPower(ePower);
            Game.MessagingManager.Send(EventString.Test_SimulationUpdateModel, Model);
        }

        private ISimCombat GetSimulation(PropCon m)
        {
            return Cfg.ConditionPropCfg.GetSimulation(m.Name,m.Strength, m.Agility, m.Weapon, m.Armor, m.Combat.GetGrade(),
                m.Combat.Level, m.Force.GetGrade(), m.Force.Level, m.Dodge.GetGrade(), m.Dodge.Level);
        }

        public void ForceLevelAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillLevelAdd(PropCon.Skills.Force);
            UpdateModel();
        }
        public void CombatLevelAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillLevelAdd(PropCon.Skills.Combat);
            UpdateModel();
        }
        public void DodgeLevelAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer); 
            mo.SkillLevelAdd(PropCon.Skills.Dodge);
            UpdateModel();
        }
        public void ForceLevelSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillLevelSub(PropCon.Skills.Force);
            UpdateModel();
        }
        public void CombatLevelSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillLevelSub(PropCon.Skills.Combat);
            UpdateModel();
        }
        public void DodgeLevelSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillLevelSub(PropCon.Skills.Dodge);
            UpdateModel();
        }
        public void ForceGradeAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeAdd(PropCon.Skills.Force);
            UpdateModel();
        }
        public void CombatGradeAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeAdd(PropCon.Skills.Combat);
            UpdateModel();
        }
        public void DodgeGradeAdd(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeAdd(PropCon.Skills.Dodge);
            UpdateModel();
        }
        public void ForceGradeSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeSub(PropCon.Skills.Force);
            UpdateModel();
        }
        public void CombatGradeSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeSub(PropCon.Skills.Combat);
            UpdateModel();
        }
        public void DodgeGradeSub(bool isPlayer)
        {
            var mo = GetCon(isPlayer);
            mo.SkillGradeSub(PropCon.Skills.Dodge);
            UpdateModel();
        }

        public void SetValue(bool isPlayer,TestModel.Props prop,int value)
        {
            GetCon(isPlayer).SetValue(prop, value);
            UpdateModel();
        }

        [Serializable] internal class Configure
        {
            [SerializeField] private ConditionPropertySo 状态属性配置;
            [SerializeField] private BattleSimulatorConfigSo 体力扣除配置;

            internal ConditionPropertySo ConditionPropCfg => 状态属性配置;
            internal BattleSimulatorConfigSo BattleSimulatorCfg => 体力扣除配置;
        }

        public class TestModel 
        {
            public enum Props
            {
                Strength,
                Agility,
                Weapon,
                Armor
            }
            public PropCon Player { get; set; }
            public PropCon Enemy { get; set; }

            public TestModel()
            {
                Player = new PropCon();
                Enemy = new PropCon();
            }

            internal ISimulationOutcome SimulateOutcome(ConditionPropertySo conditionCfg,
                BattleSimulatorConfigSo simCfg)
            {
                var p = Player;
                var e = Enemy;
                var player = conditionCfg.GetSimulation(p.Name, p.Strength, p.Agility, p.Weapon, p.Armor,
                    p.Combat.GetGrade(),
                    p.Combat.Level, p.Force.GetGrade(), p.Force.Level, p.Dodge.GetGrade(), p.Dodge.Level);
                var enemy = conditionCfg.GetSimulation(e.Name, e.Strength, e.Agility, e.Weapon, e.Armor,
                    e.Combat.GetGrade(),
                    e.Combat.Level, e.Force.GetGrade(), e.Force.Level, e.Dodge.GetGrade(), e.Dodge.Level);
                var outCome = simCfg.CountSimulationOutcome(player, enemy);
                return outCome;
            }
        }

        /// <summary>
        /// 状态属性
        /// </summary>
        public class PropCon
        {
            public enum Skills
            {
                Force,
                Combat,
                Dodge
            }

            public string Name { get; set; }
            public int Strength { get; set; }
            public int Agility { get; set; }
            public int Weapon { get; set; }
            public int Armor { get; set; }
            public float Offend { get; set; }
            public float Defend { get; set; }
            public float StrengthPower { get; set; }
            public float AgilityPower { get; set; }
            public float WeaponPower { get; set; }
            public float ArmorPower { get; set; }
            public Skill Combat { get; set; }
            public Skill Force { get; set; }
            public Skill Dodge { get; set; }

            public PropCon()
            {
                Combat = new Skill();
                Force = new Skill();
                Dodge = new Skill();
            }

            public class Skill
            {
                public int Grade { get; set; }
                public int Level { get; set; } = 1;
                public float Power { get; set; }

                internal SkillGrades GetGrade() => (SkillGrades)Grade;

                public Skill()
                {

                }
            }

            internal void SkillLevelAdd(Skills skill)
            {
                switch (skill)
                {
                    case Skills.Force:
                        Force.Level++;
                        break;
                    case Skills.Combat:
                        Combat.Level++;
                        break;
                    case Skills.Dodge:
                        Dodge.Level++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
                }
            }
            internal void SkillLevelSub(Skills skill)
            {
                switch (skill)
                {
                    case Skills.Force:
                        Force.Level--;
                        Force.Level = Math.Clamp(Force.Level, 1, 10);
                        break;
                    case Skills.Combat:
                        Combat.Level--;
                        Combat.Level = Math.Clamp(Combat.Level, 1, 10);
                        break;
                    case Skills.Dodge:
                        Dodge.Level--;
                        Dodge.Level = Math.Clamp(Dodge.Level, 1, 10);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
                }
            }
            internal void SkillGradeAdd(Skills skill)
            {
                switch (skill)
                {
                    case Skills.Force:
                        Force.Grade++;
                        break;
                    case Skills.Combat:
                        Combat.Grade++;
                        break;
                    case Skills.Dodge:
                        Dodge.Grade++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
                }
            }
            internal void SkillGradeSub(Skills skill)
            {
                switch (skill)
                {
                    case Skills.Force:
                        Force.Grade--;
                        Force.Grade = Math.Clamp(Force.Grade, 0, 10);
                        break;
                    case Skills.Combat:
                        Combat.Grade--;
                        Combat.Grade = Math.Clamp(Combat.Grade, 0, 10);
                        break;
                    case Skills.Dodge:
                        Dodge.Grade--;
                        Dodge.Grade = Math.Clamp(Dodge.Grade, 0, 10);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
                }
            }
            internal void SetValue(TestModel.Props prop, int value)
            {
                switch (prop)
                {
                    case TestModel.Props.Strength:
                        Strength = value;
                        break;
                    case TestModel.Props.Agility:
                        Agility = value;
                        break;
                    case TestModel.Props.Weapon:
                        Weapon = value;
                        break;
                    case TestModel.Props.Armor:
                        Armor = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(prop), prop, null);
                }
            }

            internal void SetPower(ISimCombat s)
            {
                StrengthPower = s.Strength;
                AgilityPower = s.Agility;
                WeaponPower = s.Weapon;
                ArmorPower = s.Armor;
                Combat.Power = s.Combat;
                Force.Power = s.Force;
                Dodge.Power = s.Dodge;
                Offend = s.Offend;
                Defend = s.Defend;
            }
        }

        public class Outcome
        {
            public Round[] Rounds { get; set; }
            public int RemainingHp { get; set; }
            public bool IsPlayerWin { get; set; }
            public float EnemyOffend { get; set; }
            public float PlayerOffend { get; set; }
            public float EnemyDefend { get; set; }
            public float PlayerDefend { get; set; }
            public int Result { get; set; }

            public Outcome()
            {
                
            }
            internal Outcome(ISimulationOutcome o, BattleSimulatorConfigSo simCfg)
            {
                Rounds = o.Rounds.Select(r => new Round(r)).ToArray();
                IsPlayerWin = o.IsPlayerWin;
                RemainingHp = o.RemainingHp;
                PlayerOffend = o.PlayerOffend;
                EnemyOffend = o.EnemyOffend;
                PlayerDefend = o.PlayerDefend;
                EnemyDefend = o.EnemyDefend;
                Result = o.Result;
            }

            public class Round : ISimulationRound
            {
                public float PlayerDefend { get; set; }
                public float EnemyDefend { get; set; }

                public Round()
                {
                }

                public Round(ISimulationRound r)
                {
                    PlayerDefend = r.PlayerDefend;
                    EnemyDefend = r.EnemyDefend;
                }
            }
        }
    }
}