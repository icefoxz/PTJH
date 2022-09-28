using System;
using System.Collections;
using Systems;
using Systems.Utls;
using UnityEngine;
using UnityEngine.Networking;
using Utls;

public class GameApp : UnitySingleton<GameApp>
{
#if UNITY_EDITOR
    [SerializeField] private bool AutoStart;
#else
    private bool AutoStart => true;
#endif
    private static ILRuntimeManager ILRuntimeMgr { get; set; }
    private static ResMgr ResMgr { get; set; }

    //*************初始化Unity项目底层框架：*****************

    private IEnumerator CheckHotFix()
    {
        this.Log();
        //执行资源下载策略
        yield break;
    }

    protected override void Awake()
    {
        base.Awake();
        ILRuntimeMgr = new ILRuntimeManager();
        StartCoroutine(CoInit());
    }
    public void StartApp() 
    { 
        var game = Instance.gameObject.AddComponent<Game>();
        game.Init(ResMgr);
        ILRuntimeMgr.InvokeHotfixMain();
    }

    private IEnumerator CoInit()
    {
        yield return CheckHotFix();
        yield return StartIlRuntimeService();
#if DEBUG
        ILRuntimeManager.StartDebug();
#endif
        ResMgr = gameObject.AddComponent<ResMgr>();
        if(AutoStart) StartApp();
    }

    private const string HotFixPath = "/HotFix/HotFix_Project.dll";
    private const string HotFixPdbPath = "/HotFix/HotFix_Project.pdb";
    /// <summary>
    /// 启动ILRuntime虚拟机服务
    /// </summary>
    private IEnumerator StartIlRuntimeService()
    {
#if UNITY_EDITOR
        using var reqDll = new UnityWebRequest("file:///" + Application.streamingAssetsPath + HotFixPath);
#else
        using var reqDll = new UnityWebRequest(Application.streamingAssetsPath + HotFixPath);
#endif
        reqDll.downloadHandler = new DownloadHandlerBuffer();
        yield return reqDll.SendWebRequest();
        if (reqDll.result != UnityWebRequest.Result.Success)
        {
            this.Log("网络连接失败！");
            yield return null;
        }
        var dll = reqDll.downloadHandler.data;

        //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
        byte[] pdb = null;
#if UNITY_EDITOR || DEBUG
        using var reqPdb = new UnityWebRequest("file:///" + Application.streamingAssetsPath + HotFixPdbPath);
        reqPdb.downloadHandler = new DownloadHandlerBuffer();
        yield return reqPdb.SendWebRequest();
        if (reqPdb.result == UnityWebRequest.Result.Success)
            pdb = reqPdb.downloadHandler.data;
#endif
        ILRuntimeMgr.LoadHotFixAssembly(dll, pdb);
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
