using System;
using System.Collections;
using AOT._AOT;
using AOT._AOT.Core;
using AOT._AOT.Core.Systems;
using AOT._AOT.Core.Systems.Coroutines;
using AOT._AOT.Core.Systems.Messaging;
using AOT._AOT.Core.Systems.Updaters;
using AOT._AOT.Utls;
using AOT._AOT.Views;
using GameClient.Controllers;
using GameClient.GameScene;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.SoScripts;
using GameClient.Test;
using UnityEngine;

namespace GameClient.System
{
    /// <summary>
    /// 游戏主体
    /// </summary>
    public class Game : UnitySingleton<Game>
    {
        private static Canvas _sceneCanvas;
        public static UiBuilder UiBuilder { get; private set; } = new UiBuilder();
        public static IRes Res { get; private set; }
        /// <summary>
        /// 基于<see cref="MonoBehaviour"/>的Update方法(每帧)执行
        /// </summary>
        private static ObjectUpdater FrameUpdater { get; set; }
        /// <summary>
        /// 帧等待控制器
        /// </summary>
        public static UpdateAwaiterManager UpdateAwaiterMgr { get; private set; }
        public static MessagingManager MessagingManager { get; private set; }
        public static ICoroutineService CoService { get; private set; }
        public static GameControllerServiceContainer Controllers { get; private set; } 
        public static Canvas SceneCanvas
        {
            get
            {
                if (_sceneCanvas) return _sceneCanvas;
                _sceneCanvas = GameObject.FindGameObjectWithTag(Global.SceneCanvas).GetComponent<Canvas>();
                if (!_sceneCanvas)
                    throw new NullReferenceException("Unable to find scene canvas!");
                return _sceneCanvas;
            }
            private set => _sceneCanvas = value;
        }

        public static MainUi MainUi { get; private set; }
        public static IGame2DLand Game2DLand { get; private set; }
        public static GameWorld World { get; private set; }
        public static Config Config { get; set; }
        public static bool IsInit { get; private set; }
        public static BattleCache BattleCache { get; private set; } = new BattleCache();

        internal void Init(Res res, LoadDll hotUpdate, MainUi mainUi, Game2DLand game2DLand, Config config)
        {
            if (IsInit) throw new InvalidOperationException("Double Init!");
            IsInit = true;
            this.Log();
            Res = res;
            CoService = CoroutineService.Instance;
            MessagingManager = new MessagingManager();
            FrameUpdater = new ObjectUpdater();
            HotFixHelper.Init(FrameUpdater);
            UpdateAwaiterMgr = new UpdateAwaiterManager();
            MainUi = mainUi;
            MainUi.Init();
            Config = config;
            World = new GameWorld();//Init World Model
            Game2DLand = game2DLand;
            game2DLand.Init(config.GameAnimCfg);
            //SceneCanvas = sceneCanvas;
            InitControllers();
            hotUpdate.StartHotReloadAssembly("App", "Run");
            //***************Init********************//
        }

        public static Color GetColorFromGrade(int grade) => Config.Global.GradeColor.GetColor((ColorGrade)grade);
        public static Color GetColorFromItemGrade(int grade) => GetColorFromGrade(grade + 1);
        /// <summary>
        /// 完全初始化后，游戏开始的入口
        /// </summary>
        public static void Run()
        {
            Instance.Log();
            Instance.StartGameAfterSeconds();
        }

        /// <summary>
        /// 由于IlRuntime的初始化会因为游戏性能而导致ui延迟, 所以需要延迟一段时间再开始游戏, 否则ui的一些注册事件会失效
        /// </summary>
        /// <param name="sec"></param>
        private void StartGameAfterSeconds(float sec = 1f)
        {
            StartCoroutine(StartAfterSec(sec));

            IEnumerator StartAfterSec(float innerSec)
            {
                yield return new WaitForSeconds(innerSec);
                Hack_Faction.TestFaction();
            }
        }

        private static void InitControllers()
        {
            Controllers = new GameControllerServiceContainer();
            //***************Reg********************//
            Controllers.Reg(new RecruitController());
            Controllers.Reg(new DiziController());
            Controllers.Reg(new StaminaController());
            Controllers.Reg(new DiziAdvController());
            Controllers.Reg(new RewardController());
            Controllers.Reg(new DataController());
            Controllers.Reg(new DiziIdleController());
            Controllers.Reg(new FactionController());
            Controllers.Reg(new GameStageController());
            Controllers.Reg(new SkillController());
            Controllers.Reg(new BattleController());
            Controllers.Reg(new ChallengeStageController());
        }

        private static void TestFactionInventory()
        {
            UiBuilder.Build("view_fractionInventory", v =>
            {
                MainUi.SetPanel(v);
                var rect = (RectTransform)v.GameObject.transform;
                rect.sizeDelta = Vector2.zero;
                rect.pivot = Vector2.zero;
                MainUi.ShowPanel();
            }, null);
        }

        void Update()
        {
            FrameUpdater?.Update();
            UpdateAwaiterMgr.GameAwaitersUpdate();
        }
    }
}