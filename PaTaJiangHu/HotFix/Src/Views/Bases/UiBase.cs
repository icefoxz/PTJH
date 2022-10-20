using System;
using System.Collections.Generic;
using UnityEngine;
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
        protected UiBase(GameObject gameObject,bool display)
        {
            this.gameObject = gameObject;
            Display(display);
        }
        public void Display(bool display) => gameObject.SetActive(display);
    }

    internal class ListViewUi<T> : UiBase
    {
        private List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        private View Prefab { get; }

        public ListViewUi(View prefab, GameObject go, bool hideChildrenViews = true) : base(go, true)
        {
            Prefab = prefab;
            if (hideChildrenViews) HideChildren();
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