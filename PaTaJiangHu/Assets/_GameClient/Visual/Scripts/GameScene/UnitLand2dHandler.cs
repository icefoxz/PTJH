using System;
using _GameClient.Models;
using UnityEngine;

/// <summary>
/// 演示场景(非战斗演示)
/// </summary>
public class UnitLand2dHandler 
{
    private enum Facing
    {
        Right,
        Left
    }
    private CharacterUiSyncHandler UiHandler { get; }
    private Config.GameAnimConfig AnimConfig { get;  }
    private ParallaxBackgroundController ParallaxBackgroundController { get; }

    private CharacterOperator _currentOp;
    private CharacterOperator CurrentOp => _currentOp =
        _currentOp != null ? _currentOp : AnimConfig.InstanceCharacterOp(Game.Game2DLand.Transform);

    internal UnitLand2dHandler(CharacterUiSyncHandler uiHandler, Config.GameAnimConfig animConfig,
        ParallaxBackgroundController backgroundController)
    {
        UiHandler = uiHandler;
        AnimConfig = animConfig;
        ParallaxBackgroundController = backgroundController;
    }

    public void PlayDizi(Dizi dizi)
    {
        var op = CurrentOp;
        var facing = Facing.Right;
        switch (dizi.State.Current)
        {
            case DiziStateHandler.States.Lost:
                op.SetAnim(CharacterOperator.Anims.Defeat);
                ParallaxBackgroundController.StopAll();
                break;
            case DiziStateHandler.States.Idle:
            case DiziStateHandler.States.AdvWaiting:
                op.SetAnim(CharacterOperator.Anims.Idle);
                ParallaxBackgroundController.StopAll();
                break;
            case DiziStateHandler.States.AdvProgress:
                op.SetAnim(CharacterOperator.Anims.Run);
                ParallaxBackgroundController.Move(false);
                break;
            case DiziStateHandler.States.AdvProduction:
                op.SetAnim(CharacterOperator.Anims.Walk);
                ParallaxBackgroundController.Move(false, 0.5f);
                break;
            case DiziStateHandler.States.AdvReturning:
                facing = Facing.Left;
                op.SetAnim(CharacterOperator.Anims.Run);
                ParallaxBackgroundController.Move(true);
                break;
            case DiziStateHandler.States.Battle:
            default:
                throw new ArgumentOutOfRangeException();
        }
        SetFacing(op, facing);
    }

    private static void SetFacing(CharacterOperator op,Facing facing)
    {
        var scale = op.transform.localScale.x;
        var changeFacing = facing switch
        {
            Facing.Right => scale < 0,
            Facing.Left => scale > 0,
            _ => throw new ArgumentOutOfRangeException(nameof(facing), facing, null)
        };
        if (changeFacing) op.transform.SetLocalScaleX(-1 * scale);
    }
}