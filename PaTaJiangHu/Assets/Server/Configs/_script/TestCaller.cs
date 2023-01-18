using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using BattleM;
using Server.Configs.Adventures;
using Server.Configs.Items;
using Server.Configs.Skills;
using Server.Configs.TestControllers;
using Server.Controllers;
using Test;
using UnityEngine;
using Utls;

namespace Server.Configs
{
    public interface ITestCaller : ISingletonDependency
    {
        ISkillController InstanceSkillController();
        ITestDiziController InstanceDiziController();
        ITestAdventureController InstanceAdventureController();
        IAdvMapController InstanceAdvMapController();
        IBattleSimController InstanceBattleSimController();
        string InitAutoAdventure();
        void SetHpValue(int value);
        void SetHpMax(int value);
        void SetHpFix(int value);
        void SetMpValue(int value);
        void SetMpMax(int value);
        void SetMpFix(int value);
    }
    /// <summary>
    /// 请求中间管道。处理请求与事件的数据交互
    /// </summary>
    public class TestCaller : DependencySingleton<ITestCaller>, ITestCaller
    {
        //在测试模式中，controller是模拟服务器的数据控制器，在客户端使用都是直接向服务器请求的
        private TestAdventureController AdvController { get; set; }
        private TestTestDiziController TestTestDiziController { get; set; }
        private SkillController SkillController { get; set; }
        private AdvMapController MapController { get; set; }
        private BattleSimController BattleSimController { get; set; }

        [SerializeField] private AdvMapController.Configure 地图;
        private AdvMapController.Configure MapConfig => 地图;
        [SerializeField] private TestTestDiziController.DiziConfig 弟子配置;
        private TestTestDiziController.DiziConfig DiziCfg => 弟子配置;
        [SerializeField] private TestAdventureController.AdvConfig 副本配置;
        private TestAdventureController.AdvConfig AdvConfig => 副本配置;
        [SerializeField] private SkillController.Configure 技能配置;
        private SkillController.Configure SkillConfig => 技能配置;
        [SerializeField] private BattleSimController.Configure 模拟战斗配置;
        private BattleSimController.Configure BattleSimConfig => 模拟战斗配置;
        [SerializeField] private AutoAdventureConfig 历练配置;
        private AutoAdventureConfig AutoAdventureCfg => 历练配置;

        public IBattleSimController InstanceBattleSimController()
        {
            BattleSimController = new BattleSimController(BattleSimConfig);
            return BattleSimController;
        }
        public IAdvMapController InstanceAdvMapController()=> MapController = new AdvMapController(MapConfig);
        public ISkillController InstanceSkillController() => SkillController = new SkillController(SkillConfig);
        public ITestDiziController InstanceDiziController() => TestTestDiziController = new TestTestDiziController(DiziCfg);
        public ITestAdventureController InstanceAdventureController() => AdvController = new TestAdventureController(AdvConfig);

        public void StartAdvMapLoad() => MapController.LoadMap();
        public void StartCombatLevelTest() => SkillController.ListCombatSkills();
        public void StartForceLevelTest() => SkillController.ListForceSkills();
        public void StartDodgeLevelTest() => SkillController.ListDodgeSkills();
        public void StartAdventureMaps() => AdvController.StartAdventureMaps();
        public void StartSimulationSo()
        {
            Game.MessagingManager.Send(EventString.Test_SimulationStart, string.Empty);
            //BattleSimController.Start();
        }

        public void StartAutoAdventure() => Game.MessagingManager.Send(EventString.Test_AutoAdvDiziInit, string.Empty);
        public string InitAutoAdventure()
        {
            Test_Faction.TestFaction();
            var faction = Game.World.Faction;
            var p = AutoAdventureCfg.Player;
            var dizi = new Dizi(guid: Guid.NewGuid().ToString(), name: p.Name,
                strength: new GradeValue<int>(p.Strength, 0),
                agility: new GradeValue<int>(p.Agility, 0),
                hp: new GradeValue<int>(p.Hp, 0),
                mp: new GradeValue<int>(p.Mp, 0),
                level: 1, grade: 0, stamina: 50, bag: 5, combatSlot: 1, forceSlot: 1, dodgeSlot: 1,
                combatSkill: p.GetCombat(), forceSkill: p.GetForce(), dodgeSkill: p.GetDodge());
            faction.AddDizi(dizi);
            return dizi.Guid;
        }

        public UnitStatus TestStatus { get; } = new(100, 100, 100);

        private void SetCon(ConValue con, int value, string method)
        {
            con.Set(value);
            Game.MessagingManager.Send(method, con);
        }
        private void SetConMax(ConValue con, int value, string method)
        {
            con.SetMax(value);
            Game.MessagingManager.Send(method, con);
        }
        private void SetConFix(ConValue con, int value, string method)
        {
            con.SetFix(value);
            Game.MessagingManager.Send(method, con);
        }

        public void SetHpValue(int value) => SetCon(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetHpMax(int value) => SetConMax(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetHpFix(int value) => SetConFix(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetMpValue(int value) => SetCon(TestStatus.Mp, value, EventString.Test_UpdateMp);
        public void SetMpMax(int value) => SetConMax(TestStatus.Mp, value, EventString.Test_UpdateMp);
        public void SetMpFix(int value) => SetConFix(TestStatus.Mp, value, EventString.Test_UpdateMp);

        public class Status
        {
            public GameCon Hp { get; set; }
            public GameCon Mp { get; set; }

            public Status()
            {
                
            }
            public Status(ICombatStatus s)
            {
                Hp = new GameCon(s.Hp);
                Mp = new GameCon(s.Mp);
            }

            public class GameCon
            {
                public int Max { get; set; }
                public int Value { get; set; }
                public int Fix { get; set; }

                public GameCon()
                {
                    
                }
                public GameCon(IGameCondition c)
                {
                    Max = c.Max;
                    Value = c.Value;
                    Fix = c.Fix;
                }
            }
        }

        [Serializable]internal class AutoAdventureConfig
        {
            [SerializeField] private CombatNpcSo 测试玩家;
            public CombatNpcSo Player => 测试玩家;
        }
    }
}