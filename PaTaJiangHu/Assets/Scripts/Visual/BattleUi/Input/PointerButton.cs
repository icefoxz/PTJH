using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Visual.BaseUis;

namespace Visual.BattleUi.Input
{
    /// <summary>
    /// 扩展按键，扩展按下和释放的事件
    /// </summary>
    [AddComponentMenu("UI/PointerButton")]
    public class PointerButton : Button, IUiBase
    {
        public UnityEvent<PointerEventData> OnPointerDownEvent { get; set; } = new UnityEvent<PointerEventData>();
        public UnityEvent<PointerEventData> OnPointerUpEvent { get; set; } = new UnityEvent<PointerEventData>();
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnPointerDownEvent.Invoke(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            OnPointerUpEvent.Invoke(eventData);
        }

        public void Show() => gameObject.SetActive(true);

        public void Hide() => gameObject.SetActive(false);

        public void ResetUi() { }
    }
}