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
    void InitBattle(DiziBattle battle);
    IEnumerator PlayRound(DiziRoundInfo info);
    Vector2 ConvertWorldPosToCanvasPos(RectTransform mainCanvasRect, RectTransform targetParent, Vector3 objWorldPos);
    void FinalizeBattle();
    void SelectDizi(string guid);
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
    public Transform Transform => transform;
    private Canvas MainCanvas => Game.SceneCanvas;
    private RectTransform MainCanvasRect { get; set; }

    private Faction Faction => Game.World.Faction;
    private UnitLand2dHandler UnitLandHandler { get; set; }
    private Dizi CurrentDizi { get; set; }

    public void Init()
    {
        MainCanvasRect = MainCanvas.transform as RectTransform;
        CharacterUiSyncHandler.Init(this, MainCanvas, Camera.main);
        UnitLandHandler = new UnitLand2dHandler(CharacterUiSyncHandler, Game.Config.GameAnimCfg, ParallaxBgController);
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

    public void SelectDizi(string guid)
    {
        CurrentDizi = Faction.GetDizi(guid);
        if (CurrentDizi.State.Current == DiziStateHandler.States.Battle)
            throw new InvalidOperationException("当前战斗未完成,不允许切换弟子！");
        UnitLandHandler.PlayDizi(CurrentDizi);
    }
}