using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AOT.Utls.Inputs
{
    /// <summary>
    /// 与<see cref="ClickHandlerBase"/>组合实现的2d物件点击事件管理器, 事件管理器仅仅是处理点击触发.<br/>
    /// 并没有实现任何扩展. 理想中可以实现更多过逻辑如Tag或是<see cref="LayerMask"/>来实现高层过滤
    /// </summary>
    public class ClickInputEventSystem : EventSystem
    {
        public enum ClickPriorities
        {
            Cover, // 当前2D物体点击覆盖UI点击
            CoveredByUi, // 当前2D物体点击被UI点击覆盖
            Concurrent // 当前2D物体点击和UI点击并发执行
        }

        public static ClickInputEventSystem Instance { get; private set; }

        private Dictionary<Collider2D, ClickHandlerBase> _handlers;
        private ClickHandlerBase _currentHandler;

        protected override void Awake()
        {
            base.Awake();
            if (Instance == null)
            {
                Instance = this;
                _handlers = new Dictionary<Collider2D, ClickHandlerBase>();
                return;
            }
            Destroy(gameObject);
        }

        public void RegClick(ClickHandlerBase handler) => 
            _handlers[handler.Collider] = handler;

        public void RemoveClick(ClickHandlerBase handler) => 
            _handlers.Remove(handler.Collider);

        private void CustomButtonDown()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider == null || !_handlers.TryGetValue(hit.collider, out var handler)) return;

            if (!handler.Collider.gameObject.activeInHierarchy) return;

            var isUiOverlapped = IsPointerOverGameObject(); // when mouse is over UI

            // when click is covered by UI
            if (handler.ClickPriority == ClickPriorities.CoveredByUi && isUiOverlapped) return;

            _currentHandler = handler; // save the current handler
        }

        private void CustomButtonUp()
        {
            if (!Input.GetMouseButtonUp(0) || _currentHandler == null) return;

            var worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null && hit.collider == _currentHandler.Collider && _handlers.TryGetValue(hit.collider, out var handler))
            {
                if (handler.Collider.gameObject.activeInHierarchy) // when game object is active
                {
                    var isUiOverlapped = IsPointerOverGameObject(); // when mouse is over UI

                    // when click is covered by UI
                    if (handler.ClickPriority != ClickPriorities.CoveredByUi || !isUiOverlapped)
                    {
                        handler.OnClick();
                        _currentHandler = null;
                        return;
                    }
                }
            }

            _currentHandler = null;
        }

        protected override void Update()
        {
            CustomButtonDown();
            CustomButtonUp();
            if (_currentHandler?.ClickPriority == ClickPriorities.Cover)
                return; // when click is covered by UI
            base.Update();
        }
    }
}