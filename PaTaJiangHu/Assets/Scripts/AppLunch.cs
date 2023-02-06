using System;
using System.Collections;
using Systems;
using UnityEngine;
using Utls;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
/// <summary>
/// 应用入口
/// </summary>
public class AppLunch : UnitySingleton<AppLunch>
{
    private enum GameRun
    {
        Game,
        Test,
    }

    [SerializeField] private GameRun _mode;
#if UNITY_EDITOR
    [Header("自动或手动启动游戏，在打包此设定无效--自动")]
    [SerializeField] private bool AutoStart;
#else
    private bool AutoStart => true;
#endif

    private static IlRuntimeManager ILRuntimeMgr { get; set; }
    private static Res Res { get; set; }
    private static AppDomain _appDomain;
    //************场景引用**************//
    [SerializeField] private MainUi _mainUi;
    [SerializeField] private Canvas _sceneCanvas;
    [SerializeField] private ConfigureSo _configureSo;
    [SerializeField] private Preloader _preloader;

    //*************初始化Unity项目底层框架：*****************
    private IEnumerator CheckHotFix()
    {
        this.Log();
        //执行资源下载策略
        yield break;
    }

    protected override void OnAwake()
    {
        _appDomain = new AppDomain();
        ILRuntimeMgr = new IlRuntimeManager(_appDomain);
        StartCoroutine(CoInit());
    }


    //初始化热更依赖以及底层框架，如果是开启AutoStart会自动执行StartApp()，
    // info: 如果是打包会强制执行 StartApp()
    private IEnumerator CoInit()
    {
        Res = gameObject.AddComponent<Res>();
        var isResourceInit = false;
        Res.Initialize(() => { isResourceInit = true; }, ResourcesProgress);
        yield return new WaitUntil(() => isResourceInit);
        yield return CheckHotFix();
#if UNITY_EDITOR
        yield return ILRuntimeMgr.StartIlRuntimeServiceWithPdb(Debug.unityLogger);
        IlRuntimeManager.StartDebug();
        if (AutoStart) StartGame();
#else
        var handle = Res.LoadAssetAsyncHandler<TextAsset>("Game");
        yield return handle;
        yield return ILRuntimeMgr.StartIlRuntimeService(handle.Result.bytes, Debug.unityLogger);
        StartGame();
#endif
        _preloader?.SetFinish();

        void ResourcesProgress(float progress)
        {
            var percentage = (int)(progress * 100);
            _preloader?.SetProgress(progress);
            XDebug.Log($"Resources loading ...... {percentage}%");
        }
    }

#if UNITY_EDITOR
    public void ManualStartGame()
    {
        if (Game.IsInit) return;
        StartGame();
    }
#endif

    //开始游戏
    private void StartGame()
    {
        var ilService = new IlService(_appDomain);
        //实例+初始化游戏控件
        var game = Instance.gameObject.AddComponent<Game>();
        game.Init(Res, ilService, _mainUi, _configureSo.Config);
        //调用热更逻辑来启动游戏
        switch (_mode)
        {
            case GameRun.Game:
                ILRuntimeMgr.LaunchAppRun();
                break;
            case GameRun.Test:
                ILRuntimeMgr.LaunchAppRunTest();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //资源管理，
    //网络管理，
    //声音管理，
    void OnApplicationQuit()
    {
        StopAllCoroutines();
        ILRuntimeMgr?.Dispose();
        GC.Collect();
    }
}
