using UnityEngine;
using UnityEngine.Events;

namespace AOT.Utls.Inputs
{
    /// <summary>
    /// 物件点击器, 基于<see cref="ClickInputEventSystem"/>实现了基础的点击指令, <br/>
    /// 当点击到物件时, 会调用<see cref="_onClickEvent"/>.
    /// </summary>
    public class Clicker : ClickHandlerBase
    {
        [SerializeField]private UnityEvent _onClickEvent = new UnityEvent();
        public UnityEvent OnClickEvent => _onClickEvent;
        public override void OnClick()
        {
            // Your logic here.
            _onClickEvent.Invoke();
        }
    }
}