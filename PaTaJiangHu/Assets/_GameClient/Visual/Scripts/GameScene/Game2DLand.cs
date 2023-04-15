using System;
using System.Collections;
using _GameClient.Models;
using UnityEngine;

/// <summary>
/// 游戏场景实现接口
/// </summary>
public interface IGame2DLand
{
    CharacterUiSyncHandler CharacterUiSyncHandler { get; }
    Transform Transform { get; }
    void InitBattle(string guid, DiziBattle battle);
    IEnumerator PlayRound(DiziRoundInfo info);
    Vector2 ConvertWorldPosToCanvasPos(RectTransform mainCanvasRect, RectTransform targetParent, Vector3 objWorldPos);
    void FinalizeBattle();
    void PlayDizi(string guid);
}

/// <summary>
/// 游戏2d场景
/// </summary>
public class Game2DLand : MonoBehaviour, IGame2DLand
{
    [SerializeField] private ParallaxBackgroundController _parallaxBgController;
    [SerializeField] private BattleStage2D _battleStage;
    [SerializeField] private CharacterUiSyncHandler _characterUiSyncHandler;
    private BattleStage2D BattleStage => _battleStage;
    private ParallaxBackgroundController ParallaxBgController => _parallaxBgController;
    public CharacterUiSyncHandler CharacterUiSyncHandler => _characterUiSyncHandler;
    public Transform Transform => _battleStage.transform;
    private Canvas MainCanvas => Game.SceneCanvas;
    private RectTransform MainCanvasRect { get; set; }

    private Faction Faction => Game.World.Faction;
    private UnitLand2dHandler UnitLandHandler { get; set; }
    private Dizi CurrentDizi { get; set; }

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

    public void InitBattle(string guid, DiziBattle battle)
    {
        CurrentDizi = Faction.GetDizi(guid);
        BattleStage.InitBattle(battle);
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