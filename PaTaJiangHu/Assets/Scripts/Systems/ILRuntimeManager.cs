using System;
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntimeDemo;
using Server.Controllers.Adventures;
using UnityEngine;
using UnityEngine.Networking;
using Views;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Application = UnityEngine.Application;
namespace Systems
{
    /// <summary>
    /// ILRuntime热更新管理器
    /// </summary>
    public class IlRuntimeManager : IDisposable
    {
        private const string HotfixProjectMain = "HotFix_Project.Main";
        private const string HotFixProjectRun = "Run";

        private const string HotFixPath = "/HotFix/HotFix_Project.dll";
        private const string HotFixPdbPath = "/HotFix/HotFix_Project.pdb";
        public static string HotfixDllPath => $"{Application.streamingAssetsPath}{HotFixPath}";
        //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个
        private static AppDomain Appdomain { get; set; } 
        public bool IsInit { get; private set; }
        private Stream Pdb { get; set; }//作为字段留下是为了全局调用，一旦游戏结束就释放
        private Stream Dll { get; set; }//作为字段留下是为了全局调用，一旦游戏结束就释放

        public IlRuntimeManager(AppDomain appDomain)
        {
            Appdomain = appDomain;
        }
        private void LoadHotFixAssembly(Stream dll,Stream pdb)
        {
            Appdomain.LoadAssembly(dll, pdb, new PdbReaderProvider());
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，
            //为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            Appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            RegCLRBinding();
            IsInit = true;
        }
        //IlRuntime注册
        private void RegCLRBinding()
        {
            Appdomain.DelegateManager.RegisterFunctionDelegate<Task<HttpResponseMessage>>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<Task<object>>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<Task<int>>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<Task<string>>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<Task<bool>>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, AdvUnit>();

            Appdomain.DelegateManager.RegisterFunctionDelegate<object>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<string>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<bool>();
            Appdomain.DelegateManager.RegisterFunctionDelegate<int>();

            Appdomain.DelegateManager.RegisterMethodDelegate<HttpResponseMessage>();
            Appdomain.DelegateManager.RegisterMethodDelegate<object>();
            Appdomain.DelegateManager.RegisterMethodDelegate<int>();
            Appdomain.DelegateManager.RegisterMethodDelegate<string>();
            Appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            Appdomain.DelegateManager.RegisterMethodDelegate<object[]>();
            Appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            Appdomain.DelegateManager.RegisterMethodDelegate<GameObject, IView>();
            Appdomain.DelegateManager.RegisterMethodDelegate<Adventure>();

            Appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            Appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            Appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            Appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            Appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            Appdomain.RegisterCrossBindingAdaptor(new TestClassBaseAdapter());

            //CLR绑定初始化
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(Appdomain);
        }

        //如果要开启调试必须调用这个
        public static void StartDebug() => Appdomain.DebugService.StartDebugService(56000);

#if UNITY_EDITOR
        //如果需要，可以用这个方法启动本地的Dll
        public IEnumerator StartIlRuntimeServiceWithDll(ILogger log)
        {
            using var reqDll = new UnityWebRequest($"file:///{Application.streamingAssetsPath}{HotFixPath}");
            reqDll.downloadHandler = new DownloadHandlerBuffer();
            yield return reqDll.SendWebRequest();
            if (reqDll.result != UnityWebRequest.Result.Success)
            {
                log?.Log("网络连接失败！");
                yield return null;
            }
            var dll = reqDll.downloadHandler.data;
            Pdb = new MemoryStream(dll);
            yield return LoadPdb();
        }
        private IEnumerator LoadPdb()
        {
            using var reqPdb = new UnityWebRequest("file:///" + Application.streamingAssetsPath + HotFixPdbPath);
            reqPdb.downloadHandler = new DownloadHandlerBuffer();
            yield return reqPdb.SendWebRequest();
            if (reqPdb.result == UnityWebRequest.Result.Success)
            {
                var pdb = reqPdb.downloadHandler.data;
                //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，
                //不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
                Pdb = pdb == null ? null : new MemoryStream(pdb);
            }
        }
#endif

        /// <summary>
        /// 启动ILRuntime虚拟机服务
        /// </summary>
        /// <returns></returns>
        public IEnumerator StartIlRuntimeService(byte[] dll,ILogger log = null)
        {
            Dll = new MemoryStream(dll);
            LoadHotFixAssembly(Dll, null);
            yield break;
        }
        public void Dispose()
        {
            Pdb?.Dispose();
            Dll?.Dispose();
        }

        //调用主应用
        public void LaunchAppDomain()
        {
            //调用逻辑应用的主体
            Appdomain.Invoke(HotfixProjectMain, HotFixProjectRun, null, null);
        }


    }
}
