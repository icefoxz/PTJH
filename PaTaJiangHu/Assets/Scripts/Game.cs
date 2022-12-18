using System;
using Server;
using Systems;
using Systems.Coroutines;
using Systems.Messaging;
using Systems.Updaters;
using UnityEngine;
using Utls;
using Views;

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

    public static bool IsInit { get; private set; }
    public void Init(Res res, IlService ilService, MainUi mainUi)
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
        //SceneCanvas = sceneCanvas;
    }

    /// <summary>
    /// 完全初始化后，游戏开始的入口
    /// </summary>
    public static void Run()
    {
        Instance.Log();
        Game.UiBuilder.Build("view_fractionInventory", v =>
        {
            MainUi.SetPanel(v);
            var rect = (RectTransform)v.GameObject.transform;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = Vector2.zero;
            MainUi.ShowPanel();
        });
    }

    void Update()
    {
        FrameUpdater?.Update();
        UpdateAwaiterMgr.GameAwaitersUpdate();
    }
}