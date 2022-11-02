using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Visual.BattleUi.Scene
{
    /// <summary>
    /// 战斗格控制器，管理战斗单位的移位，控制战斗场景与镜头的位置逻辑。
    /// </summary>
    public class BattleSlotController : MonoBehaviour, IBattleSlotController
    {
        public enum Positions
        {
            First,
            Center,
            Last,
        }
        [SerializeField] private SlotUi _slotPrefab;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private List<SlotUi> _slots;
        [SerializeField] private float _scrollSecs = 0.5f;
        
        private RectTransform MainCanvas { get; set; }
        public int IndexOf(GameObject obj)
        {
            var slot = GetSlot(obj);
            if (slot == null) return -1;
            return _slots.IndexOf(slot);
        }

        public SlotUi GetSlot(GameObject obj) => _slots.SingleOrDefault(s => s.Objs.Any(o => o == obj));
        public void PlaceObject(int index, GameObject obj)
        {
            var oldSlot = _slots.SingleOrDefault(s=>s.Objs.Any(o=>o == obj));
            oldSlot?.Remove(obj);
            var newSlot = _slots[index];
            newSlot.PlaceObj(obj);
        }
        public void ResetUi()
        {
            _slots.ForEach(s=>s.ResetUi());
        }
        public int GetIndexInScreen(Positions pos)
        {
            return pos switch
            {
                Positions.First => GetFirstIndexInScene(),
                Positions.Center => GetCenterIndexInScene(),
                Positions.Last => GetLastIndexInScene(),
                _ => throw new ArgumentOutOfRangeException(nameof(pos), pos, null)
            };
        }
        private int GetLastIndexInScene()
        {
            var proportionPoint = _scrollRect.horizontalNormalizedPosition;
            var rightProportionWidthInScreen = ProportionWidthInScreen(proportionPoint, true);
            var proScrollWith = ProportionScrollWith(proportionPoint);
            var allocatedWidthToRightCorner = proScrollWith + rightProportionWidthInScreen;//从左到屏幕右角总面积
            var minAllocatedSlot = (int)Math.Floor(allocatedWidthToRightCorner / SlotWidth);//最低使用列
            return minAllocatedSlot - 1;//-1获取索引
        }
        private int GetFirstIndexInScene()
        {
            var proportionPoint =_scrollRect.horizontalNormalizedPosition;
            var proWidthInScreen = ProportionWidthInScreen(proportionPoint, false);
            var proScrollWith = ProportionScrollWith(proportionPoint);
            var hiddenWith = proScrollWith - proWidthInScreen;//列表里不显示的面积
            var hiddenSlot = (int)Math.Ceiling(hiddenWith / SlotWidth);//找出隐藏列
            return hiddenSlot;
        }
        private int GetCenterIndexInScene()
        {
            var proportionPoint =_scrollRect.horizontalNormalizedPosition;
            var proScrollWith = ProportionScrollWith(proportionPoint);
            return (int)Math.Ceiling(proScrollWith / SlotWidth);
        }
        /// <summary>
        /// 拖动栏的所占面积，常规给出左边
        /// </summary>
        /// <param name="proportionPoint"></param>
        /// <returns></returns>
        private double ProportionScrollWith(float proportionPoint) =>
            (ScrollRectWidth * proportionPoint) - SlotWidth * 0.5; //列表所占面积
        /// <summary>
        /// 屏里所占面积，常规给左边
        /// </summary>
        /// <param name="proportionPoint"></param>
        /// <param name="right">是否右边</param>
        /// <returns></returns>
        private double ProportionWidthInScreen(float proportionPoint, bool right)
        {
            var proWidthInScreen = (CanvasWidth * proportionPoint) - SlotWidth * 0.5;
            if (right) return CanvasWidth - proWidthInScreen;
            return proWidthInScreen;
        }
        public void Init(RectTransform mainCanvas)
        {
            MainCanvas = mainCanvas;
            foreach (var ui in _slotParent.GetComponentsInChildren<SlotUi>()) 
                ui.gameObject.SetActive(false);
            UpdateWidthFromCanvas();
            FullscreenUnits = (int)Math.Ceiling(CanvasWidth / SlotWidth);//全屏数量
            for (var i = 0; i < FullscreenUnits * 3; i++) InstanceSlot();
            UpdateScrollRectWidth();
            Centralize(_slots.Count / 2);
        }
        #region MathCounting
        //满屏数量
        private int FullscreenUnits { get; set; }
        private float SlotWidth { get; set; }
        private float ScrollRectWidth { get; set; }
        private float CanvasCenter { get; set; }
        private float CanvasWidth { get; set; }
        //有效总范围
        private float ValidWidth => ScrollRectWidth - CanvasWidth;
        private void UpdateScrollRectWidth()
        {
            var size = _scrollRect.content.sizeDelta;
            _scrollRect.content.sizeDelta = new Vector2(_slots.Count * SlotWidth, size.y);
            ScrollRectWidth = _scrollRect.content.rect.width; //slots总面积
        }
        private void UpdateWidthFromCanvas()
        {
            CanvasWidth = MainCanvas.rect.width; //屏幕面积
            CanvasCenter = CanvasWidth * 0.5f; //屏幕中心点
            SlotWidth = ((RectTransform)_slotPrefab.transform).sizeDelta.x;
        }
        // 计算占比点
        private float CountProportionPoint(int index)
        {
            var selectedWidth = SlotWidth * (index + 1) - (SlotWidth * 0.5f); //减去半个单slot的面积获取中间值
            var validSelectedWidth = selectedWidth - CanvasCenter;
            var proportion = validSelectedWidth / ValidWidth; //有效位置占比
            return proportion;
        }
        #endregion
        private void SetSlotSelected(int index)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                slot.SetCenterColor(index == i);
            }
        }
        public void Centralize(int index)
        {
            UpdateWidthFromCanvas();
            var slot = _slots[index];
            var proportion = CountProportionPoint(index);//获取占比点
            if (proportion is < 0 or > 1)//如果占比点超出滑动范围
            {
                var overflow = proportion > 1 ? proportion - 1 : proportion;//获取溢出面积
                var toBeAdd = Math.Ceiling(Math.Abs(overflow * ValidWidth) / SlotWidth) + FullscreenUnits;//计算需要添加的slot + offset
                for (int i = 0; i < toBeAdd; i++) SlotIn(proportion < 0);//添加slot
                index = _slots.IndexOf(slot);//重新获取物件新位置
                proportion = CountProportionPoint(index);//重新计算占比点
            }
            //设置占比点(把物件位置滑动到中心)
            SetSlotSelected(index);
            StartCoroutine(StartScroll(proportion, slot));
        }
        private void SlotAlignment(SlotUi obj)
        {
            var index = _slots.IndexOf(obj); //获取目标位置
            if (index >= FullscreenUnits && index + FullscreenUnits <= _slots.Count) return;
            for (var i = 0; i < FullscreenUnits; i++) SlotIn(index < FullscreenUnits); //添加slot
            var proportion = CountProportionPoint(_slots.IndexOf(obj));
            _scrollRect.horizontalNormalizedPosition = proportion;
        }
        private IEnumerator StartScroll(float scrollValue, SlotUi slotUi)
        {
            yield return _scrollRect.DOHorizontalNormalizedPos(scrollValue, _scrollSecs)
                .OnComplete(() => SlotAlignment(slotUi));
        }
        private void SlotIn(bool firstIndex)
        {
            var ui = InstanceSlot();
            SlotUi removeObj = null;
            if (firstIndex)
            {
                removeObj = _slots[^1];
                ui.transform.SetSiblingIndex(0);
                _slots.Insert(0, ui);
            }
            else
            {
                removeObj = _slots[0];
                _slots.Add(ui);
            }

            _slots.Remove(removeObj);
            Destroy(removeObj.gameObject);
        }
        private SlotUi InstanceSlot()
        {
            var ui = Instantiate(_slotPrefab, _slotParent);
            ui.Init();
            _slots.Add(ui);
            return ui;
        }
    }
    public interface IBattleSlotController
    {
        void Init(RectTransform mainCanvas);
        void Centralize(int index);
        int IndexOf(GameObject obj);
        void ResetUi();
        int GetIndexInScreen(BattleSlotController.Positions pos);
        void PlaceObject(int index, GameObject obj);
    }
}
