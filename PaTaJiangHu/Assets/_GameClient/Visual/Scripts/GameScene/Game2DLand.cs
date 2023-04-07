using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 游戏场景实现接口
/// </summary>
public interface IGame2DLand
{
    CharacterUiSyncHandler CharacterUiSyncHandler { get; }
    void InitBattle(DiziBattle battle);
    IEnumerator PlayRound(DiziRoundInfo info);
    Vector2 ConvertWorldPosToCanvasPos(RectTransform mainCanvasRect, RectTransform targetParent, Vector3 objWorldPos);
    void FinalizeBattle();
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
    private Canvas MainCanvas => Game.SceneCanvas;
    private RectTransform MainCanvasRect { get; set; }

    public void Init()
    {
        MainCanvasRect = MainCanvas.transform as RectTransform;
        CharacterUiSyncHandler.Init(this, MainCanvas, Camera.main);
    }

    public void InitBattle(DiziBattle battle) => BattleStage.InitBattle(battle);
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

    public void FinalizeBattle() => BattleStage.FinalizeBattle();
}