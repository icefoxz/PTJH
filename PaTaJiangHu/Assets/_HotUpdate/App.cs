using System;
using GameClient.System;
using HotUpdate._HotUpdate.Demo_v1;
using UnityEngine;

namespace HotUpdate._HotUpdate
{
    /// <summary>
    /// 游戏App程序
    /// 不可以继承Monobehaviour
    /// </summary>
    public class App 
    {
        private static MainUiAgent MainUiAgent { get; set; }
        public static void Run()
        {
            Debug.Log($"{nameof(App)}.{nameof(Run)}!");
            //初始化热更新工程框架模块：UI管理，事件订阅...
            InitUiManager();
            Game.Run();//游戏启动
        }

        private static void InitUiManager()
        {
            MainUiAgent = AppLaunch.UiAgent switch
            {
                "Demo_v1" => new Demo_v1Agent(Game.MainUi),
                _ => throw new NotImplementedException($"找不到指定的MainUi代理= {AppLaunch.UiAgent}!")
            };
        }
    }
}