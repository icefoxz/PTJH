using AOT.Utls;
using System;
using UnityEngine;
using GameClient.Models;
using GameClient.Modules.Adventure;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts;
using GameClient.SoScripts.BattleSimulation;
using GameClient.SoScripts.Characters;
using GameClient.System;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace GameClient.Test
{
    public class TestStateController : MonoBehaviour
    {
        [SerializeField] private AdventureConfigSo AdventureCfg;
        [SerializeField] private Config.BattleConfig BattleCfg;
        [SerializeField] private BattleSimulatorConfigSo BattleSimulation;
        [SerializeField] private GradeConfigSo GradeCfg;
        private AdventureMapSo[] AdvMaps => AdventureCfg.AdvMaps; //回城秒数
        private AdventureActivityHandler AdvHandler { get; set; }
        private DiziGenerator DiziGenerator { get; set; }
        [ShowInInspector]private AdventureActivity DiziActivity { get; set; }

        void Start()
        {
            DiziGenerator = new DiziGenerator(GradeCfg);
            AdvHandler = new AdventureActivityHandler(AdventureCfg, BattleCfg, BattleSimulation);
        }
        [LabelText("实例化历练")]
        [Button(ButtonSizes.Large,ButtonStyle.FoldoutButton),GUIColor("yellow")]public void InstanceActivity(int mapIndex)
        {
            var map = AdvMaps[mapIndex];
            var dizi = DiziGenerator.GenerateDizi(Random.Range(0, 5));
            Game.World.Faction.TryAddDizi(dizi);
            var activity = new Activity(map, SysTime.UnixNow, dizi);
            DiziActivity = activity;
        }
        
        private bool isRequesting = false;
        [LabelText("请求里数")]
        [Button(ButtonSizes.Large,ButtonStyle.FoldoutButton),GUIColor("blue")]public async void RequestOnMile(int minutes = 1)
        {
            if (isRequesting)
            {
                XDebug.Log("正在请求中..");
                return;
            }
            isRequesting = true;
            var activity = DiziActivity;
            var updateTime = activity.LastUpdate + TimeSpan.FromMinutes(minutes).TotalMilliseconds;
            switch (activity.State)
            {
                case AdventureActivity.States.Progress:
                {
                    await AdvHandler.UpdateActivity((long)updateTime, activity, PrintActivityLogInfo, OnLost);
                    break;
                }
                case AdventureActivity.States.Returning:
                {
                    XDebug.Log($"弟子[{DiziActivity.Dizi.Name}]回程中...");
                    activity.Set((long)updateTime, activity.LastMiles, activity.CurrentOccasion, activity.EndTime);
                    return;
                }
                case AdventureActivity.States.Waiting:
                {
                    XDebug.LogError($"弟子[{DiziActivity.Dizi.Name}]历练已经结束, 并且回到宗门了.");
                    return;
                }
                default:
                    throw new InvalidOperationException($"活动状态异常! activity.state = {activity.State}"!);
            }
            isRequesting = false;
        }

        private void OnLost(DiziActivityLog log)
        {
            XDebug.LogError($"{DiziActivity.Dizi.Name} 失踪了!!");
            PrintActivityLogInfo(log);
        }

        private void PrintActivityLogInfo(DiziActivityLog log)
        {
            Debug.Log($"Reward = {(log.Reward == null ? "0" : "1")}" +
                      $"Place = {log.Occasion}" +
                      $"Mile({log.LastMiles})\n" +
                      string.Join(',', log.AdjustEvents) +
                      string.Join(',', log.Messages));
        }

        private void PrintInfo((int mile, long updateTime, string placeName) obj) => Debug.Log($"里数:{obj.mile}, 更新时间:{obj.updateTime}, 地点:{obj.placeName}");

        private record Activity : AdventureActivity
        {
            [ShowInInspector]public DiziActivities 状态 => Activity;
            [ShowInInspector]public string 开始时间 => SysTime.LocalFromUnixTicks(StartTime).ToString("T");
            [ShowInInspector]public string 标题 => ShortTitle;
            [ShowInInspector]public string 描述 => Description;
            [ShowInInspector]public string 地点 => CurrentOccasion;
            [ShowInInspector]public string 地图 => Map.Name;
            [ShowInInspector]public string 活动类型 => Activitytype.ToString();
            [ShowInInspector]public int 里数 => LastMiles;
            [ShowInInspector]public int 记录数 => Logs.Count;

            public Activity(IAutoAdvMap map, long startTime, Dizi dizi) : base(map, startTime, dizi)
            {
            }
        }
    }
}