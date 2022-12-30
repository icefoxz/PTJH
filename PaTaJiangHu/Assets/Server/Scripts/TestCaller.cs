using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using QFramework;
using Server.Controllers.Adventures;
using Server.Controllers.Characters;
using Server.Controllers.Items;
using UnityEngine;
using Utls;

namespace Server
{
    public interface ITestCaller : ISingletonDependency
    {
        ISkillController InstanceSkillController();
        ITestDiziController InstanceDiziController();
        IAdventureController InstanceAdventureController();
        IAdvMapController InstanceAdvMapController();
        IBattleSimController InstanceBattleSimController();
        void SetHpValue(int value);
        void SetHpMax(int value);
        void SetHpFix(int value);
        void SetMpValue(int value);
        void SetMpMax(int value);
        void SetMpFix(int value);
        void UseMedicine(int id);
    }
    /// <summary>
    /// 请求中间管道。处理请求与事件的数据交互
    /// </summary>
    public class TestCaller : DependencySingleton<ITestCaller>, ITestCaller
    {
        //在测试模式中，controller是模拟服务器的数据控制器，在客户端使用都是直接向服务器请求的
        private AdventureController AdvController { get; set; }
        private TestTestDiziController TestTestDiziController { get; set; }
        private SkillController SkillController { get; set; }
        private AdvMapController MapController { get; set; }
        private BattleSimController BattleSimController { get; set; }

        [SerializeField] private AdvMapController.Configure 地图;
        private AdvMapController.Configure MapConfig => 地图;
        [SerializeField] private TestTestDiziController.DiziConfig 弟子配置;
        private TestTestDiziController.DiziConfig DiziCfg => 弟子配置;
        [SerializeField] private ItemConfig 物品配置;
        private ItemConfig ItemCfg => 物品配置;
        [SerializeField] private AdventureController.AdvConfig 副本配置;
        private AdventureController.AdvConfig AdvConfig => 副本配置;
        [SerializeField] private SkillController.Configure 技能配置;
        private SkillController.Configure SkillConfig => 技能配置;
        [SerializeField] private BattleSimController.Configure 模拟战斗配置;
        private BattleSimController.Configure BattleSimConfig => 模拟战斗配置;

        public IBattleSimController InstanceBattleSimController()
        {
            BattleSimController = new BattleSimController(BattleSimConfig);
            return BattleSimController;
        }
        public IAdvMapController InstanceAdvMapController()=> MapController = new AdvMapController(MapConfig);
        public ISkillController InstanceSkillController() => SkillController = new SkillController(SkillConfig);
        public ITestDiziController InstanceDiziController() => TestTestDiziController = new TestTestDiziController(DiziCfg);
        public IAdventureController InstanceAdventureController() => AdvController = new AdventureController(AdvConfig);

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
        //button event
        public void StartTestMedicineWindow()
        {
            ItemCfg.Init();
            Game.MessagingManager.Send(EventString.Test_StatusUpdate, new Status(TestStatus.GetCombatStatus()));
            Game.MessagingManager.Send(EventString.Test_MedicineWindow, ItemCfg.Medicines.Values.ToList());
        }

        public void SetHpValue(int value) => SetCon(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetHpMax(int value) => SetConMax(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetHpFix(int value) => SetConFix(TestStatus.Hp, value, EventString.Test_UpdateHp);
        public void SetMpValue(int value) => SetCon(TestStatus.Mp, value, EventString.Test_UpdateMp);
        public void SetMpMax(int value) => SetConMax(TestStatus.Mp, value, EventString.Test_UpdateMp);
        public void SetMpFix(int value) => SetConFix(TestStatus.Mp, value, EventString.Test_UpdateMp);
        public void UseMedicine(int id)
        {
            var cs = TestStatus.GetCombatStatus();
            ItemCfg.Medicines[id].Recover(cs);
            TestStatus.Clone(cs);
            Game.MessagingManager.Send(EventString.Test_StatusUpdate, new Status(cs));
        }

        [Serializable]internal class ItemConfig
        {
            [SerializeField]private MedicineFieldsSo[] 丹药配置;

            public Dictionary<int, IMedicine> Medicines { get; private set; }

            public void Init()
            {
                Medicines = 丹药配置.SelectMany(m => m.GetMedicines).ToDictionary(m => m.Id, m => m);
            }
        }

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
    }
}