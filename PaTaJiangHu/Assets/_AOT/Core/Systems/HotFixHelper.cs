using System;
using System.Threading.Tasks;
using AOT._AOT.Core.Systems.Updaters;

namespace AOT._AOT.Core.Systems
{
    /// <summary>
    /// 热更受限的代码帮助器。尤其是在异步调用。必须让主程序执行
    /// </summary>
    public class HotFixHelper
    {
        private static ObjectUpdater FrameUpdater { get; set; }
        public static void Init(ObjectUpdater frameUpdater)
        {
            FrameUpdater = frameUpdater;

        }

        public static void RegUpdate(object key, Action update) => FrameUpdater.RegMonoUpdate(key, update);
        public static void RemoveUpdate(object key) => FrameUpdater.RemoveMonoUpdate(key);

        public static async void RunTask<T>(Func<Task<T>> function, Action<T> onCompleteAction)
        {
            var result = await function();
            onCompleteAction(result);
        }

        public static async void RunObjectTask<T>(Func<Task<T>> function, Action<object> onCompleteAction)
        {
            var result = await function();
            onCompleteAction(result);
        }

        public static async void RunTask(Func<Task> function, Action onCompleteAction)
        {
            await function();
            onCompleteAction();
        }
    }
}