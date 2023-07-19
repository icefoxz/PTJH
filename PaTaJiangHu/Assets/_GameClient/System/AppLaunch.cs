using System;
using System.Collections;
using AOT;
using AOT.Core.Systems;
using AOT.Utls;
using AOT.Views;
using GameClient.GameScene;
using GameClient.SoScripts;
using UnityEngine;

namespace GameClient.System
{
    /// <summary>
    /// 应用入口
    /// </summary>
    public class AppLaunch : UnitySingleton<AppLaunch>
    {
#if UNITY_EDITOR
        [Header("自动或手动启动游戏，在打包此设定无效--自动")]
        [SerializeField] private bool AutoStart;
#else
    private bool AutoStart => true;
#endif

        //private static IlRuntimeManager ILRuntimeMgr { get; set; }
        //private static AppDomain _appDomain;

        private static Res Res { get; set; }
        //************场景引用**************//
        [SerializeField] private Canvas _sceneCanvas;//游戏画布
        [SerializeField] private MainUi _mainUi;//游戏Ui
        [SerializeField] private Game2DLand _game2DLand;//游戏场景
        [SerializeField] private ConfigSo _configureSo;//游戏配置
        [SerializeField] private Preloader _preloader;//预读
        [Header("热更新文件名")] private string _hotUpdateAssetName = "HotUpdate";
        [Header("填入所启动的UI代理器")]
        [SerializeField] private string _uiAgent;
        public static string UiAgent { get; private set; }
        private LoadDll HotUpdate { get; set; }
        //*************初始化Unity项目底层框架：*****************
        private IEnumerator CheckHotFix()
        {
            this.Log();
            //执行资源下载策略
            yield break;
        }

        protected override void OnAwake()
        {
            UiAgent = _uiAgent;
            LoadDll.LoadMetadataForAOTAssemblies(); // 加载AOT泛型函数的原始metadata
            StartCoroutine(CoInit());
        }


        //初始化热更依赖以及底层框架，如果是开启AutoStart会自动执行StartApp()，
        // info: 如果是打包会强制执行 StartApp()
        private IEnumerator CoInit()
        {
            Res = gameObject.AddComponent<Res>();
            var isResourceInit = false;
            Res.Initialize(() => { isResourceInit = true; }, ResourcesProgress);
            yield return new WaitUntil(() => isResourceInit && LoadDll.IsMetadataForAOTAssembliesLoaded);
            yield return CheckHotFix();
            HotUpdate = new LoadDll(_hotUpdateAssetName);
            yield return HotUpdate.LoadAssetTask();
#if UNITY_EDITOR
            if (AutoStart) StartGame();
#else
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
            //实例+初始化游戏控件
            var game = Instance.gameObject.AddComponent<Game>();
            game.Init(Res, HotUpdate, _mainUi, _game2DLand, _configureSo.Config);
        }

        //资源管理，
        //网络管理，
        //声音管理，
        void OnApplicationQuit()
        {
            StopAllCoroutines();
            GC.Collect();
        }
    }
}
