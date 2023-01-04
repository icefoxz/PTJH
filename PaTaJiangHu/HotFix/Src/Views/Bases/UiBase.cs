using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Views;
using Object = UnityEngine.Object;

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
        public IView View { get; private set; }

        protected UiBase(IView v, bool display)
        {
            View = v;
            gameObject = v.GameObject;
            Display(display);
        }


        protected UiBase(GameObject gameObject,bool display)
        {
            View = gameObject.GetComponent<View>();
            this.gameObject = gameObject;
            Display(display);
        }
        public void Display(bool display) => gameObject.SetActive(display);
        public void Destroy()
        {
            Display(false);
            Object.Destroy(gameObject);
        }
    }

    internal class ListViewUi<T> : UiBase
    {
        private List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        private View Prefab { get; }

        public ListViewUi(View prefab, GameObject contentGo, bool hideChildrenViews = true) : base(contentGo, true)
        {
            Prefab = prefab;
            if (hideChildrenViews) HideChildren();
        }
        public ListViewUi(View prefab, RectTransform transform, bool hideChildrenViews = true) : this(prefab,
            transform.gameObject, hideChildrenViews)
        {
        }

        public ListViewUi(View prefab, ScrollRect scrollRect, bool hideChildrenViews = true) : this(prefab,
            scrollRect.content, hideChildrenViews)
        {
        }

        public void HideChildren()
        {
            foreach (Transform view in transform)
                view.gameObject.SetActive(false);
        }

        public T Instance(Func<View,T> func)
        {
            var obj = Object.Instantiate(Prefab, gameObject.transform);
            obj.gameObject.SetActive(true);
            var ui = func.Invoke(obj);
            _list.Add(ui);
            return ui;
        }
        public void ClearList(Action<T> onRemoveFromList)
        {
            foreach (var ui in _list) onRemoveFromList(ui);
            _list.Clear();
        }
        public void Remove(T obj) => _list.Remove(obj);
    }
}