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
        [SerializeField] private BattleSlotController _slotController;
        private IBattleSlotController SlotController => _slotController;

        public void Init(RectTransform mainCanvas)
        {
            SlotController.Init(mainCanvas);
        }

        public void PlaceCenter(Stickman stickman)
        {
            var index = SlotController.GetIndexInScreen(BattleSlotController.Positions.Center);
            SlotController.PlaceObject(index, stickman.gameObject);
            stickman.ResetPosition();
        }
        public void AutoPlace(Stickman stickman, Stickman target, CombatUnit combat, bool centralize)
        {
            var targetIndex = CountIndex(target, Math.Abs(combat.Position - combat.Target.Position));

            SlotController.PlaceObject(targetIndex, stickman.gameObject);
            stickman.ResetPosition();
            if (centralize)
                SlotController.Centralize(targetIndex);
            UpdateStickmanOrientation(stickman, target);
        }

        public void AutoPlace(Stickman stickman, Stickman target, CombatUnit combat, bool centralize, Action afterPlaceCallback)
        {
            var targetIndex = CountIndex(target, Math.Abs(combat.Position - combat.Target.Position));
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

        private int CountIndex(Stickman target, int distance)
        {
            var targetIndex = SlotController.IndexOf(target.gameObject);
            var randomDirection = Sys.RandomBool() ? -1 : 1;
            var placeIndex = Math.Abs(targetIndex - distance * randomDirection);
            return placeIndex;
        }

        public void ResetUi() => SlotController.ResetUi();

        public void UpdateStickmanOrientation(Stickman stickman, Stickman target)
        {
            var index = SlotController.IndexOf(stickman.gameObject);
            var tarIndex = SlotController.IndexOf(target.gameObject);
            stickman.SetOriented(index > tarIndex);
        }
    }
}