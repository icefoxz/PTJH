using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
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
        IDiziController InstanceDiziController();
        IAdventureController InstanceAdventureController();
        IAdvMapController InstanceAdvMapController();
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
        private DiziController DiziController { get; set; }
        private SkillController SkillController { get; set; }
        private AdvMapController MapController { get; set; }

        [SerializeField] private AdvMapController.Configure 地图;
        private AdvMapController.Configure MapConfig => 地图;
        [SerializeField] private DiziController.DiziConfig 弟子配置;
        private DiziController.DiziConfig DiziCfg => 弟子配置;
        [SerializeField] private ItemConfig 物品配置;
        private ItemConfig ItemCfg => 物品配置;
        [SerializeField] private AdventureController.AdvConfig 副本配置;
        private AdventureController.AdvConfig AdvConfig => 副本配置;
        [SerializeField]private SkillController.Configure 技能配置;
        private SkillController.Configure SkillConfig => 技能配置;

        public IAdvMapController InstanceAdvMapController()=> MapController = new AdvMapController(MapConfig);
        public ISkillController InstanceSkillController() => SkillController = new SkillController(SkillConfig);
        public IDiziController InstanceDiziController() => DiziController = new DiziController(DiziCfg);
        public IAdventureController InstanceAdventureController() => AdvController = new AdventureController(AdvConfig);

        public void StartAdvMapLoad() => MapController.LoadMap();
        public void StartCombatLevelTest() => SkillController.ListCombatSkills();
        public void StartForceLevelTest() => SkillController.ListForceSkills();
        public void StartDodgeLevelTest() => SkillController.ListDodgeSkills();
        public void StartAdventureMaps() => AdvController.StartAdventureMaps();

        public UnitStatus TestStatus { get; } = new(100, 100, 100);

        private void SetCon(ConValue con, int value, string method)
        {
            con.Set(value);
            Game.MessagingManager.Invoke(method, con);
        }
        private void SetConMax(ConValue con, int value, string method)
        {
            con.SetMax(value);
            Game.MessagingManager.Invoke(method, con);
        }
        private void SetConFix(ConValue con, int value, string method)
        {
            con.SetFix(value);
            Game.MessagingManager.Invoke(method, con);
        }
        //button event
        public void StartTestMedicineWindow()
        {
            ItemCfg.Init();
            Game.MessagingManager.Invoke(EventString.Test_StatusUpdate, TestStatus.GetCombatStatus());
            Game.MessagingManager.Invoke(EventString.Test_MedicineWindow, ItemCfg.Medicines.Values.ToList());
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
            Game.MessagingManager.Invoke(EventString.Test_StatusUpdate, cs);
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
    }
}