using UnityEngine;
using Views;

namespace HotFix_Project.Views.Bases
{
    /// <summary>
    /// Ui物件的基类，提供最基础的<see cref="gameObject"/>与<see cref="transform"/>，与显示<see cref="Display"/>方法
    /// </summary>
    internal abstract class UiBase
    {
        protected GameObject gameObject { get; }
        protected Transform transform => gameObject.transform;
        protected Transform parent => gameObject.transform.parent;
        protected UiBase(GameObject gameObject)
        {
            this.gameObject = gameObject;
            Display(false);
        }
        public void Display(bool display) => gameObject.SetActive(display);
    }
}