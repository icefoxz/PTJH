using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using _GameClient.Models;
using ILAdapters;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Intepreter;
using ILRuntimeDemo;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Utls;
using Views;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
using Application = UnityEngine.Application;
using Dizi = Models.Dizi;

namespace Systems
{
    /// <summary>
    /// ILRuntime热更新管理器
    /// </summary>
    public class IlRuntimeManager : IDisposable
    {
        private const string HotfixProjectMain = "HotFix_Project.Main";
        private const string HotFixProjectRun = "Run";

        private const string HotFixPath = "/HotFix/Hotfix_Project.dll";
        private const string HotFixPdbPath = "/HotFix/Hotfix_Project.pdb";
        public const string GeneratedCode = "Assets/ILRuntime/Generated";
        public static string HotfixDllPath => $"{Application.streamingAssetsPath}{HotFixPath}";
        //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个
        public static AppDomain Appdomain { get; set; } 
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
            RegCLRBinding(Appdomain);
            IsInit = true;
        }
        //IlRuntime注册
        public static void RegCLRBinding(AppDomain appdomain)
        {
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<HttpResponseMessage>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<object>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<int>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<string>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<bool>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<Task<float>>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, int>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, object>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, string>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, bool>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, float>();
            appdomain.DelegateManager.RegisterFunctionDelegate<IGrouping<int, ILTypeInstance>, ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, ILTypeInstance, ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, ILTypeInstance[]>();
            appdomain.DelegateManager.RegisterFunctionDelegate<IView, ILTypeInstance>();
            appdomain.DelegateManager.RegisterFunctionDelegate<int, View, IView>();
            appdomain.DelegateManager.RegisterFunctionDelegate<ILTypeInstance, ILTypeInstance>();

            //Linq 弟子
            appdomain.DelegateManager.RegisterFunctionDelegate<Dizi, bool>();

            appdomain.DelegateManager.RegisterFunctionDelegate<object>();
            appdomain.DelegateManager.RegisterFunctionDelegate<string>();
            appdomain.DelegateManager.RegisterFunctionDelegate<float>();
            appdomain.DelegateManager.RegisterFunctionDelegate<bool>();
            appdomain.DelegateManager.RegisterFunctionDelegate<int>();
            appdomain.DelegateManager.RegisterFunctionDelegate<int, ValueTuple<string, Action>>();

            appdomain.DelegateManager.RegisterMethodDelegate<HttpResponseMessage>();
            appdomain.DelegateManager.RegisterMethodDelegate<object>();
            appdomain.DelegateManager.RegisterMethodDelegate<float>();
            appdomain.DelegateManager.RegisterMethodDelegate<int>();
            appdomain.DelegateManager.RegisterMethodDelegate<string>();
            appdomain.DelegateManager.RegisterMethodDelegate<bool>();
            appdomain.DelegateManager.RegisterMethodDelegate<object[]>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject>();
            appdomain.DelegateManager.RegisterMethodDelegate<GameObject, IView>();
            appdomain.DelegateManager.RegisterMethodDelegate<JToken[]>();
            appdomain.DelegateManager.RegisterMethodDelegate<IView>();
            appdomain.DelegateManager.RegisterMethodDelegate<ILTypeInstance>();
            appdomain.DelegateManager.RegisterMethodDelegate<ObjectBag>();
            appdomain.DelegateManager.RegisterMethodDelegate<int, IView>();
            appdomain.DelegateManager.RegisterMethodDelegate<EffectView, RectTransform>();
            appdomain.DelegateManager.RegisterMethodDelegate<int, int, EffectView, RectTransform>();
            appdomain.DelegateManager.RegisterMethodDelegate<ValueTuple<int, int, EffectView, RectTransform>>();
            appdomain.DelegateManager.RegisterMethodDelegate<ValueTuple<int, int, RectTransform>>();
            appdomain.DelegateManager.RegisterMethodDelegate<IEffectView>();

            appdomain.RegisterValueTypeBinder(typeof(Vector3), new Vector3Binder());
            appdomain.RegisterValueTypeBinder(typeof(Quaternion), new QuaternionBinder());
            appdomain.RegisterValueTypeBinder(typeof(Vector2), new Vector2Binder());
            appdomain.RegisterCrossBindingAdaptor(new MonoBehaviourAdapter());
            appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());
            appdomain.RegisterCrossBindingAdaptor(new TestClassBaseAdapter());
            appdomain.RegisterCrossBindingAdaptor(new IListAdapter());
            appdomain.RegisterCrossBindingAdaptor(new ApplicationExceptionAdapter());
            appdomain.RegisterCrossBindingAdaptor(new IDictionaryEnumeratorAdapter());

            //CLR绑定初始化
#if !UNITY_EDITOR
            ILRuntime.Runtime.Generated.CLRBindings.Initialize(Appdomain);
#else
            //ILRuntime.Runtime.Generated.CLRBindings.Initialize(Appdomain);
#endif
        }

        //如果要开启调试必须调用这个
        public static void StartDebug()
        {
            var debug = Appdomain.DebugService.StartDebugService(56000);
            Debug.Log($"Appdomain Debug! {debug}");
        }

#if UNITY_EDITOR
        //如果需要，可以用这个方法启动本地的Dll
        public IEnumerator StartIlRuntimeServiceWithPdb(ILogger log)
        {
            using var reqDll = new UnityWebRequest($"file:///{HotfixDllPath}");
            reqDll.downloadHandler = new DownloadHandlerBuffer();
            yield return reqDll.SendWebRequest();
            if (reqDll.result != UnityWebRequest.Result.Success)
            {
                log?.Log("网络连接失败！");
                yield return null;
            }
            var dll = reqDll.downloadHandler.data;
            Dll = new MemoryStream(dll);
            yield return LoadPdb();
            LoadHotFixAssembly(Dll, Pdb);
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
            LoadHotFixAssembly(Dll, Pdb);
            yield break;
        }
        public void Dispose()
        {
            Pdb?.Dispose();
            Dll?.Dispose();
        }

        //调用主应用
        public void LaunchAppRun()
        {
            //调用逻辑应用的主体
            Appdomain.Invoke(HotfixProjectMain, HotFixProjectRun, null, null);
        }
        
        public void LaunchAppRunTest()
        {
            //调用逻辑应用的主体
            Appdomain.Invoke(HotfixProjectMain, "RunTest", null, null);
        }
        
    }
}
