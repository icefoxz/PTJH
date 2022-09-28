using ILAdaptors;
using Systems;
using UnityEngine;

namespace HotFix_Project
{
    /// <summary>
    /// 游戏App程序
    /// 不可以继承Monobehaviour
    /// </summary>
    public class App
    {
        public static void Init()
        {
            Debug.Log($"{nameof(App)}.{nameof(Init)}!");
            //初始化热更新工程框架模块：UI管理，事件订阅...
        }
    }
}