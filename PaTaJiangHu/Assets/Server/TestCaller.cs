using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using Data.LitJson;
using Server.Controllers;
using Server.Controllers.Adventures;
using Server.Controllers.Characters;
using Server.Controllers.Items;
using UnityEngine;
using Utls;

namespace Server
{
    public interface ITestCaller : ISingletonDependency
    {
        void OnGenerateDizi(int grade);
        void OnSetStamina(int minutePerSta, int currentStamina);
        void SetHpValue(int value);
        void SetHpMax(int value);
        void SetHpFix(int value);
        void SetMpValue(int value);
        void SetMpMax(int value);
        void SetMpFix(int value);
        void UseMedicine(int id);
        void OnStartMapEvent(int mapId);
        void OnEventInvoke(int mapId, int index);
    }
    /// <summary>
    /// 请求中间管道。处理请求与事件的数据交互
    /// </summary>
    public class TestCaller : DependencySingleton<ITestCaller>, ITestCaller
    {
        //在测试模式中，controller是模拟服务器的数据控制器，在客户端使用都是直接向服务器请求的
        private AdventureController AdvController { get; set; }
        private DiziController DiziController { get; set; }
        [SerializeField] private DiziConfig 弟子配置;
        private DiziConfig DiziCfg => 弟子配置;
        [SerializeField] private ItemConfig 物品配置;
        private ItemConfig ItemCfg => 物品配置;
        [SerializeField] private AdvConfigs 副本配置;
        private AdvConfigs AdvConfig => 副本配置;

        protected override void OnAwake()
        {
            AdvController = new AdventureController();
            DiziController = new DiziController(
                DiziCfg.LevelConfig, 
                DiziCfg.GradeConfigSo, 
                DiziCfg.StaminaGenerator);
        }

        public void StartAdventureMaps()
        {
            var list = AdvConfig.Maps.Select(map => AdvController.InstanceMapData(map)).ToList();
            Game.MessagingManager.Invoke(EventString.Test_AdventureMap, list.ToArray());
        }
        public void OnEventInvoke(int mapId, int index)
        {
            var map = AdvConfig.Maps.First(m => m.Id == mapId);
            var advEvent = map.AllEvents[index];
            var eventData = AdvController.InstanceEventData(map, advEvent);
            Game.MessagingManager.Invoke(EventString.Test_AdvEventInvoke, eventData);
        }

        public void OnStartMapEvent(int mapId)
        {
            var map = AdvConfig.Maps.First(m => m.Id == mapId);
            var eventData = AdvController.InstanceEventData(map, map.StartEvent);
            Game.MessagingManager.Invoke(EventString.Test_AdvEventInvoke, eventData);
        }

        public void OnGenerateDizi(int grade)
        {
            var dizi = DiziController.GenerateDizi(grade);
            Game.MessagingManager.Invoke(EventString.Test_DiziGenerate, dizi);
        }

        public void OnSetStamina(int currentStamina,int minutePass)
        {
            var lastTicks = SysTime.UnixTicksFromNow(TimeSpan.FromMinutes(-minutePass));
            DiziController.SetStamina(currentStamina, lastTicks);
        }

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

        public void StartTestMedicineWindow()
        {
            ItemCfg.Init();
            Game.MessagingManager.Invoke(EventString.Test_StatusUpdate, TestStatus.GetCombatStatus());
            Game.MessagingManager.Invoke(EventString.Test_MedicineWindow, ItemCfg.Medicines.Values.ToList());
        }

        public void SetHpValue(int value) => SetCon(TestStatus.Hp,value, EventString.Test_UpdateHp);
        public void SetHpMax(int value) => SetConMax(TestStatus.Hp,value, EventString.Test_UpdateHp);
        public void SetHpFix(int value) => SetConFix(TestStatus.Hp,value, EventString.Test_UpdateHp);
        public void SetMpValue(int value) => SetCon(TestStatus.Mp,value, EventString.Test_UpdateMp);
        public void SetMpMax(int value) => SetConMax(TestStatus.Mp,value, EventString.Test_UpdateMp);
        public void SetMpFix(int value) => SetConFix(TestStatus.Mp,value, EventString.Test_UpdateMp);
        public void UseMedicine(int id)
        {
            var cs = TestStatus.GetCombatStatus();
            ItemCfg.Medicines[id].Recover(cs);
            TestStatus.Clone(cs);
            Game.MessagingManager.Invoke(EventString.Test_StatusUpdate, cs);
        }

        [Serializable]private class AdvConfigs
        {
            [SerializeField] private AdvMapSo[] 地图;
            public AdvMapSo[] Maps => 地图;
        }
        [Serializable]private class DiziConfig
        {
            [SerializeField] private LevelConfigSo 等级配置;
            [SerializeField] private GradeConfigSo 资质配置;
            [SerializeField] private StaminaGenerateSo 体力产出配置;

            public LevelConfigSo LevelConfig => 等级配置;
            public GradeConfigSo GradeConfigSo => 资质配置;
            public StaminaGenerateSo StaminaGenerator => 体力产出配置;
        }
        [Serializable]private class ItemConfig
        {
            [SerializeField]private MedicineFieldsSo[] 丹药配置;

            public Dictionary<int, IMedicine> Medicines { get; private set; }

            public void Init()
            {
                Medicines = 丹药配置
                    .SelectMany(m => m.GetMedicines)
                    .ToDictionary(m => m.Id, m => m);
            }
        }
    }
}