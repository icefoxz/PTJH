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
    [SerializeField] private Canvas _sceneCanvas;

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
        Res.Initialize(() => isResourceInit = true);
        yield return new WaitUntil(() => isResourceInit);
        yield return CheckHotFix();
        var handle = Res.LoadAssetAsyncHandler<TextAsset>("Game");
        yield return handle;
        yield return ILRuntimeMgr.StartIlRuntimeService(handle.Result.bytes, Debug.unityLogger);
#if DEBUG
        IlRuntimeManager.StartDebug();
#endif
        if(AutoStart) StartGame();
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
        game.Init(Res,ilService);
        //调用热更逻辑来启动游戏
        ILRuntimeMgr.LaunchAppDomain();
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
