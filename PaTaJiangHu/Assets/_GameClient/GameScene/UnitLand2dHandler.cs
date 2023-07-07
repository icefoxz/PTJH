using System;
using System.Threading.Tasks;
using AOT._AOT.Views.BaseUis;
using GameClient.GameScene.Background;
using GameClient.Models;
using GameClient.SoScripts;
using GameClient.System;
using UnityEngine;
using Dizi = GameClient.Models.Dizi;

namespace GameClient.GameScene
{
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

        private Action<Dizi> BouncingAction { get; set; }
        private bool isWaitingForBouncing { get; set; }
        public bool IsActive => CurrentOp.gameObject.activeSelf;

        public async void PlayDizi(Dizi dizi)
        {
            //抖动行动,为了确保多次请求,只有最后一次有效
            BouncingAction = SetDiziAction;
            if(isWaitingForBouncing)
                return;
            isWaitingForBouncing = true;
            await Task.Delay(50); //wait 50ms for bouncing
            BouncingAction?.Invoke(dizi);
            BouncingAction = null;
            isWaitingForBouncing = false;
        }

        private void SetDiziAction(Dizi dizi)
        {
            var op = CurrentOp;
            var facing = Facing.Right;
            var anim = CharacterOperator.Anims.Idle;
            switch (dizi.State.Current)
            {
                case DiziStateHandler.States.Idle:
                case DiziStateHandler.States.Auto:
                    ParallaxBackgroundController.StopAll();
                    break;
                case DiziStateHandler.States.Lost:
                    anim = CharacterOperator.Anims.Defeat;
                    ParallaxBackgroundController.StopAll();
                    break;
                case DiziStateHandler.States.AdvWaiting:
                    anim = CharacterOperator.Anims.Idle;
                    facing = Facing.Left;
                    ParallaxBackgroundController.StopAll();
                    break;
                case DiziStateHandler.States.AdvProgress:
                    anim = CharacterOperator.Anims.Run;
                    ParallaxBackgroundController.Move(true, 2);
                    break;
                case DiziStateHandler.States.AdvProduction:
                    anim = CharacterOperator.Anims.Walk;
                    ParallaxBackgroundController.Move(true);
                    break;
                case DiziStateHandler.States.AdvReturning:
                    facing = Facing.Left;
                    anim = CharacterOperator.Anims.Run;
                    ParallaxBackgroundController.Move(false, 1.5f);
                    break;
                case DiziStateHandler.States.Battle:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            op.SetAnim(anim);
            op.transform.localPosition = Vector3.zero;
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

        public void HideOperator() => CurrentOp.gameObject.SetActive(false);

        public void ShowOperator() => CurrentOp.gameObject.SetActive(true);
    }
}