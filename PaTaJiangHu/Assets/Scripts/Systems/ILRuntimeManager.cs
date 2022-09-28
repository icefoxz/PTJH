using System;
using System.IO;
using System.Linq;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;
namespace Systems
{
    /// <summary>
    /// ILRuntime热更新管理器
    /// </summary>
    public class ILRuntimeManager : IDisposable
    {
        private const string HotfixProjectMain = "HotFix_Project.Main";
        private const string HotFixProjectRun = "Run";

        //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
        //大家在正式项目中请全局只创建一个AppDomain
        private static AppDomain Appdomain { get; } = new AppDomain();
        public bool IsInit { get; private set; }
        private Stream pdb { get; set; }
        private Stream dll { get; set; }

        public void LoadHotFixAssembly(byte[] dllBytes, byte[] pdbBytes)
        {
            dll = new MemoryStream(dllBytes);
            pdb = pdbBytes == null ? null : new MemoryStream(pdbBytes);
            Appdomain.LoadAssembly(dll, pdb, new PdbReaderProvider());
            InitializeILRuntime();
            IsInit = true;
        }

        public static void StartDebug() => Appdomain.DebugService.StartDebugService(56000);

        public static Type[] LoadedTypes()
        {
            var types = Appdomain.LoadedTypes.Values.ToList();
            return types.Select(t => t.ReflectionType).ToArray();
        }
        public static IType GetLoadedTypes(string className) => Appdomain.LoadedTypes[className];

        public static void Invoke(IMethod method, object instance, params object[] param) =>
            Appdomain.Invoke(method, instance, param);
        private void InitializeILRuntime()
        {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
            //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
            Appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            //这里做一些ILRuntime的注册，HelloWorld示例暂时没有需要注册的
        }

        public void InvokeHotfixMain()
        {
            //HelloWorld，第一次方法调用
            Appdomain.Invoke(HotfixProjectMain, HotFixProjectRun, null, null);
        }

        public void Dispose()
        {
            pdb?.Dispose();
            dll?.Dispose();
        }
    }
}
