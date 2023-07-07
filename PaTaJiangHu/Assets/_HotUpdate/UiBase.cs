using System;
using System.Collections;
using System.Collections.Generic;
using AOT._AOT.Core.Systems.Coroutines;
using AOT._AOT.Views.Abstract;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HotUpdate._HotUpdate
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

        private ICoroutineInstance ServiceCo { get; set; }
        private List<Action> Events { get; } = new List<Action>();

        protected UiBase(IView v, bool display)
        {
            View = v ?? throw new ArgumentNullException($"{GetType().Name}: view = null!");
            gameObject = v.GameObject;
            Display(display);
        }

        protected void WhileActiveUpdatePerSec(Action action)
        {
            ServiceCo ??= Game.CoService.RunCo(UpdatePerSec(), null, gameObject?.name);
            Events.Add(action);
        }

        private IEnumerator UpdatePerSec()
        {
            while (true)
            {
                yield return new WaitUntil(() => gameObject.activeSelf);
                foreach (var action in Events)
                    action?.Invoke();
                yield return new WaitForSeconds(1);
            }
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
        public Coroutine StartCoroutine(IEnumerator enumerator) => View.StartCo(enumerator);
        public void StopCoroutine(IEnumerator coroutine) => View.StopCo(coroutine);
        public void StopAllCoroutines() => View.StopAllCo();
    }

    internal class ListBoardUi<T> : UiBase
    {
        internal ListBoardUi(IView v, string prefabName, string contentName, bool hideChildrenViews = true) : this(
            v.GetObject<View>(prefabName), v.GetObject(contentName), hideChildrenViews)
        {

        }

        internal ListBoardUi(View prefab, GameObject contentGo, bool hideChildrenViews = true) : base(contentGo, true)
        {
            if (hideChildrenViews) HideChildren();
            Prefab = prefab;
        }


        private List<T> _list { get; } = new List<T>();
        public IReadOnlyList<T> List => _list;
        protected View Prefab { get; }

        public void HideChildren()
        {
            foreach (Transform view in transform)
                view.gameObject.SetActive(false);
        }

        public T Instance(Func<View> onCreateView,Func<View, T> func)
        {
            var obj = onCreateView();
            obj.gameObject.SetActive(true);
            var ui = func.Invoke(obj);
            _list.Add(ui);
            return ui;
        }

        public T Instance(Func<View, T> func) => Instance(() => Object.Instantiate(Prefab, gameObject.transform), func);

        public void ClearList(Action<T> onRemoveFromList)
        {
            foreach (var ui in _list) onRemoveFromList(ui);
            _list.Clear();
        }

        public void Remove(T obj) => _list.Remove(obj);
    }

    internal class ListViewUi<T> : ListBoardUi<T>
    {
        private readonly ScrollRect _scrollRect;

        private ScrollRect ScrollRect
        {
            get
            {
                if (_scrollRect == null)
                    throw new InvalidOperationException("如果要调用ScrollRect,请在构造的时候传入scrollrect控件");
                return _scrollRect;
            }
        }

        public ListViewUi(View prefab, RectTransform transform, bool hideChildrenViews = true) : base(prefab,
            transform.gameObject, hideChildrenViews)
        {
        }

        public ListViewUi(View prefab, ScrollRect scrollRect, bool hideChildrenViews = true) : base(prefab,
            scrollRect.content.gameObject, hideChildrenViews)
        {
            if (hideChildrenViews) HideChildren();
            _scrollRect = scrollRect;
        }

        public ListViewUi(IView v, string prefabName, string scrollRectName, bool hideChildrenViews = true) : this(
            v.GetObject<View>(prefabName),
            v.GetObject<ScrollRect>(scrollRectName), hideChildrenViews)
        {
        }

        public void SetVerticalScrollPosition(float value)
        {
            ScrollRect.verticalNormalizedPosition = value;
        }

        public void SetHorizontalScrollPosition(float value)
        {
            ScrollRect.horizontalNormalizedPosition = value;
        }
    }
}