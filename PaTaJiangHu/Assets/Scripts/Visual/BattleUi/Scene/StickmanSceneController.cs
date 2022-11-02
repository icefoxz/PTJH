using System;
using BattleM;
using UnityEngine;
using Utls;

namespace Visual.BattleUi.Scene
{
    /// <summary>
    /// 火柴人战斗场景控制器，控制火柴人位置的放置逻辑
    /// </summary>
    public class StickmanSceneController : MonoBehaviour
    {
        public enum Placing
        {
            Left,
            Right,
            Random
        }
        [SerializeField] private BattleSlotController _slotController;
        private IBattleSlotController SlotController => _slotController;

        public void Init(RectTransform mainCanvas)
        {
            SlotController.Init(mainCanvas);
        }
        /// <summary>
        /// 摆放在中间，一般都是玩家单位。别同时调用多次
        /// </summary>
        /// <param name="stickman"></param>
        public void PlaceCenter(Stickman stickman)
        {
            var index = SlotController.GetIndexInScreen(BattleSlotController.Positions.Center);
            SlotController.PlaceObject(index, stickman.gameObject);
            stickman.ResetPosition();
        }
        /// <summary>
        /// 自动摆放，但需要玩家已经摆放在中间<see cref="PlaceCenter"/>才能合理移位
        /// </summary>
        public void AutoPlace(Stickman stickman, Stickman target, CombatUnit combat, bool centralize,Placing placing)
        {
            var targetIndex = CountIndex(target, Math.Abs(combat.Position - combat.Target.Position), placing);

            SlotController.PlaceObject(targetIndex, stickman.gameObject);
            stickman.ResetPosition();
            if (centralize)
                SlotController.Centralize(targetIndex);
            UpdateStickmanOrientation(stickman, target);
        }

        public void AutoPlace(Stickman stickman, Stickman target, CombatUnit combat, bool centralize, Placing placing,Action afterPlaceCallback)
        {
            var targetIndex = CountIndex(target, Math.Abs(combat.Position - combat.Target.Position), placing);
            var oriIndex = SlotController.IndexOf(stickman.gameObject);
            stickman.SetOriented(oriIndex > targetIndex);
            SlotController.PlaceObject(targetIndex, stickman.gameObject);
            stickman.Dash(0.5f,() =>
            {
                UpdateStickmanOrientation(stickman, target);
                if (centralize)
                    SlotController.Centralize(targetIndex);
                afterPlaceCallback?.Invoke();
            });
        }

        private int CountIndex(Stickman target, int distance, Placing placing)
        {
            var targetIndex = SlotController.IndexOf(target.gameObject);
            var randomDirection = placing switch
            {
                Placing.Left => -1,
                Placing.Right => 1,
                Placing.Random => Sys.RandomBool() ? -1 : 1,
                _ => throw new ArgumentOutOfRangeException(nameof(placing), placing, null)
            };
            var placeIndex = Math.Abs(targetIndex + distance * randomDirection);
            return placeIndex;
        }

        public void ResetUi() => SlotController.ResetUi();

        public void UpdateStickmanOrientation(Stickman stickman, Stickman target)
        {
            var index = SlotController.IndexOf(stickman.gameObject);
            var tarIndex = SlotController.IndexOf(target.gameObject);
            stickman.SetOriented(index > tarIndex);
        }

        public void Centralize(Stickman player) => _slotController.Centralize(SlotController.IndexOf(player.gameObject));
    }
}