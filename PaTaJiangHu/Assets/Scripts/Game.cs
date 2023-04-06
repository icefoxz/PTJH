using System;
using System.Collections;
using Server.Configs.Characters;
using Server.Controllers;
using Systems;
using Systems.Coroutines;
using Systems.Messaging;
using Systems.Updaters;
using Test;
using UnityEngine;
using Utls;

/// <summary>
/// 游戏主体
/// </summary>
public class Game : UnitySingleton<Game>
{
    private static Canvas _sceneCanvas;
    public static IlService IlService { get; private set; }
    public static UiBuilder UiBuilder { get; private set; } = new UiBuilder();
    public static IRes Res { get; private set; }
    /// <summary>
    /// 基于<see cref="MonoBehaviour"/>的Update方法(每帧)执行
    /// </summary>
    private static ObjectUpdater FrameUpdater { get; set; }
    /// <summary>
    /// 帧等待控制器
    /// </summary>
    public static UpdateAwaiterManager UpdateAwaiterMgr { get; private set; }
    public static MessagingManager MessagingManager { get; private set; }
    public static ICoroutineService CoService { get; private set; }
    public static GameControllerServiceContainer Controllers { get; private set; } 
    public static Canvas SceneCanvas
    {
        get
        {
            if (_sceneCanvas) return _sceneCanvas;
            _sceneCanvas = GameObject.FindGameObjectWithTag(Global.SceneCanvas).GetComponent<Canvas>();
            if (!_sceneCanvas)
                throw new NullReferenceException("Unable to find scene canvas!");
            return _sceneCanvas;
        }
        private set => _sceneCanvas = value;
    }

    public static MainUi MainUi { get; private set; }
    public static IGame2DLand Game2DLand { get; private set; }
    public static GameWorld World { get; private set; }
    internal static Config Config { get; set; }
    public static bool IsInit { get; private set; }
    internal void Init(Res res, IlService ilService, MainUi mainUi, Game2DLand game2DLand, Config config)
    {
        if (IsInit) throw new InvalidOperationException("Double Init!");
        IsInit = true;
        this.Log();
        Res = res;
        CoService = CoroutineService.Instance;
        MessagingManager = new MessagingManager();
        FrameUpdater = new ObjectUpdater();
        HotFixHelper.Init(FrameUpdater);
        UpdateAwaiterMgr = new UpdateAwaiterManager();
        IlService = ilService;
        MainUi = mainUi;
        MainUi.Init();
        Config = config;
        World = new GameWorld();//Init World Model
        Game2DLand = game2DLand;
        game2DLand.Init();
        //SceneCanvas = sceneCanvas;
        InitControllers();
    }

    public static Color GetColorFromGrade(int grade) => Config.Global.GradeColor.GetColor((GradeConfigSo.Grades)grade);
    public static Color GetColorFromItemGrade(int grade) => GetColorFromGrade(grade + 1);
    /// <summary>
    /// 完全初始化后，游戏开始的入口
    /// </summary>
    public static void Run()
    {
        Instance.Log();
        Instance.StartGameAfterASecond();
        //TestFactionInventory();
    }

    private void StartGameAfterASecond(float sec = 0.5f)
    {
        StartCoroutine(StartAfterSec(sec));

        IEnumerator StartAfterSec(float innerSec)
        {
            yield return new WaitForSeconds(innerSec);
            Hack_Faction.TestFaction();
        }
    }

    private static void InitControllers()
    {
        Controllers = new GameControllerServiceContainer();
        //***************Reg********************//
        Controllers.Reg(new RecruitController());
        Controllers.Reg(new DiziController());
        Controllers.Reg(new StaminaController());
        Controllers.Reg(new DiziAdvController());
        Controllers.Reg(new RewardController());
        Controllers.Reg(new DataController());
        Controllers.Reg(new DiziIdleController());
        Controllers.Reg(new FactionController());
        Controllers.Reg(new ChallengeStageController());
    }

    private static void TestFactionInventory()
    {
        UiBuilder.Build("view_fractionInventory", v =>
        {
            MainUi.SetPanel(v);
            var rect = (RectTransform)v.GameObject.transform;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = Vector2.zero;
            MainUi.ShowPanel();
        }, null);
    }

    void Update()
    {
        FrameUpdater?.Update();
        UpdateAwaiterMgr.GameAwaitersUpdate();
    }
}