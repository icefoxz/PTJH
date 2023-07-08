using System;
using System.Collections;
using AOT._AOT.Core;
using GameClient.Args;
using GameClient.GameScene.Background;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;

namespace GameClient.GameScene
{
    /// <summary>
    /// 游戏场景实现接口
    /// </summary>
    public interface IGame2DLand
    {
        CharacterUiSyncHandler CharacterUiSyncHandler { get; }
        Transform Transform { get; }
        void InitBattle(string guid, DiziBattle battle, Config.GameAnimConfig animConfig);
        IEnumerator PlayRound(DiziRoundInfo info);
        Vector2 ConvertWorldPosToCanvasPos(RectTransform mainCanvasRect, RectTransform targetParent, Vector3 objWorldPos);
        void FinalizeBattle();
        void PlayDizi(string guid);
        void SwitchPage(CameraFocus.Focus page);
    }

    /// <summary>
    /// 游戏2d场景
    /// </summary>
    public class Game2DLand : MonoBehaviour, IGame2DLand
    {
        [SerializeField] private ParallaxBackgroundController _parallaxBgController;
        [SerializeField] private BattleStage2D _battleStage;
        [SerializeField] private CharacterUiSyncHandler _characterUiSyncHandler;
        [SerializeField] private CameraFocus _cameraFocus;

        private BattleStage2D BattleStage => _battleStage;
        private ParallaxBackgroundController ParallaxBgController => _parallaxBgController;
        public CharacterUiSyncHandler CharacterUiSyncHandler => _characterUiSyncHandler;
        public Transform Transform => _battleStage.transform;
        private Canvas MainCanvas => Game.SceneCanvas;
        private RectTransform MainCanvasRect { get; set; }

        private Faction Faction => Game.World.Faction;
        private UnitLand2dHandler UnitLandHandler { get; set; }
        private Dizi CurrentDizi { get; set; }

        /// <summary>
        /// 是否在战斗演示(战斗演示期间不允许切换弟子), 并且战斗结束后必须调用FinalizeBattle
        /// </summary>
        public bool IsOnBattle => BattleStage.IsBusy || !UnitLandHandler.IsActive;

        internal void Init(Config.GameAnimConfig animConfig)
        {
            MainCanvasRect = MainCanvas.transform as RectTransform;
            CharacterUiSyncHandler.Init(this, MainCanvas, Camera.main);
            UnitLandHandler = new UnitLand2dHandler(CharacterUiSyncHandler, animConfig, ParallaxBgController);
            Game.MessagingManager?.RegEvent(EventString.Dizi_Params_StateUpdate, b =>
            {
                var dizi = Faction.GetDizi(b.GetString(0));
                if (dizi == CurrentDizi)
                    PlayDizi(dizi.Guid);
            });
        }

        public void SwitchPage(CameraFocus.Focus page)
        {
            if(IsOnBattle) FinalizeBattle();//自动清扫战场
            _cameraFocus.SetFocus(page);
        }

        public void InitBattle(string guid, DiziBattle battle, Config.GameAnimConfig animConfig)
        {
            CurrentDizi = Faction.GetDizi(guid);
            BattleStage.InitBattle(battle, animConfig);
            UnitLandHandler.HideOperator();
        }

        public IEnumerator PlayRound(DiziRoundInfo info) => BattleStage.PlayRound(info);

        public Vector2 ConvertWorldPosToCanvasPos(RectTransform mainCanvasRect,RectTransform targetParent,Vector3 objWorldPos)
        {
            // 1. Convert world position to screen position
            var objScreenPos = Camera.main.WorldToScreenPoint(objWorldPos);
            // 2. Convert screen position to a position on the main canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(MainCanvasRect, objScreenPos, null, out var canvasPos);
            // 3. Convert the position on the main canvas to the local position in the target parent
            Vector2 localPositionInParent = targetParent.InverseTransformPoint(mainCanvasRect.TransformPoint(canvasPos));
            return localPositionInParent;
        }

        public void FinalizeBattle()
        {
            if (!IsOnBattle) return;
            BattleStage.FinalizeBattle();
            UnitLandHandler.ShowOperator();
        }

        public void PlayDizi(string guid)
        {
            CurrentDizi = Faction.GetDizi(guid);
            if (CurrentDizi.State.Current == DiziStateHandler.States.Battle)
                throw new InvalidOperationException("当前战斗未完成,不允许切换弟子！");
            UnitLandHandler.PlayDizi(CurrentDizi);
        }
    }
}