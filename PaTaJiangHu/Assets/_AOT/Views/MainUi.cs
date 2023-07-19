using System.Collections;
using System.Collections.Generic;
using AOT.Utls;
using AOT.Views.Abstract;
using UnityEngine;
using UnityEngine.UI;

namespace AOT.Views
{
    public interface IMainUi : ISingletonDependency
    {
        void SetTop(IView view, bool resetPosition = false);
        void SetMid(IView view, bool resetPosition = false);
        void SetBtm(IView view, bool resetPosition = false);
        void SetWindow(IView view, bool resetPos);
        void SetGame(IView view);
        void SetPanel(IView view);
        void ShowTop();
        void ShowBtm();
        void ShowGame();
        void ShowWindow(IView v);
        void ShowPanel();
        void HideTop();
        void HideBtm();
        void HideGame();
        void HideWindows();
        void HidePanel();
        void Hide();
        void Show();
        void SetToast(IView view);
        void DisplayWindowPanel(bool display);
    }

    /// <summary>
    /// 主Ui
    /// </summary>
    public class MainUi : DependencySingleton<IMainUi>, IMainUi
    {
        [SerializeField] private RectTransform _topUi;
        [SerializeField] private RectTransform _midUi;
        [SerializeField] private RectTransform _btmUi;
        [SerializeField] private RectTransform _gameUi;
        [SerializeField] private RectTransform _window;
        [SerializeField] private RectTransform _toast;
        [SerializeField] private Transform _windowPanel;
        [SerializeField] private Image _panel;
        [SerializeField] private GameObject _pool;

        public RectTransform TopUi => _topUi;
        public RectTransform GameUi => _gameUi;
        public RectTransform MidUi => _midUi;
        public RectTransform BtmUi => _btmUi;
        public RectTransform Window => _window;
        public RectTransform Toast => _toast;
        public Transform WindowPanel => _windowPanel;
        public GameObject Pool => _pool;

        public Image Panel => _panel;
        private Dictionary<RectTransform,IView> _uiMap;
        private Dictionary<IView, View> _windowMap = new Dictionary<IView, View>();
        public IReadOnlyDictionary<IView, View> WindowMap => _windowMap;

        public void Init()
        {
            _uiMap = new Dictionary<RectTransform, IView>
            {
                { _panel.rectTransform, null },
                { _topUi, null },
                { _gameUi, null },
                { _midUi, null },
                { _btmUi, null },
            };
            ResetUi();
        }

        public void SetTop(IView view, bool resetPos) => SetUi(TopUi, view, resetPos);
        public void SetMid(IView view, bool resetPos) => SetUi(MidUi, view, resetPos);
        public void SetBtm(IView view, bool resetPos) => SetUi(BtmUi, view, resetPos);
        public void SetWindow(IView view, bool resetPos)
        {
            _windowMap.Add(view, view.GetView());
            SetUi(Window, view, resetPos);
        }
        public void SetGame(IView view) => SetUi(GameUi, view);
        public void SetPanel(IView view) => SetUi(Panel.rectTransform, view);
        public void SetToast(IView view) => SetUi(Toast, view, true);
        public void DisplayWindowPanel(bool display) => WindowPanel.gameObject.SetActive(display);

        public void ShowTop() => Display(true, TopUi);
        public void ShowMid() => Display(true, MidUi);
        public void ShowBtm() => Display(true, BtmUi);
        public void ShowGame() => Display(true, GameUi);
        public void ShowWindow(IView v)
        {
            var view = WindowMap[v];
            HideLayoutChildren(Window);
            Display(true, Window, WindowPanel, view);
        }

        public void ShowPanel() => Display(true, Panel);
        public void HideTop() => Display(false, TopUi);
        public void HideBtm() => Display(false, BtmUi);
        public void HideGame() => Display(false, GameUi);
        public void HideWindows()
        {
            HideLayoutChildren(Window);
            Display(true, WindowPanel);
            Display(false, Window);
        }

        public void HidePanel() => Display(false, Panel);

        public void Hide() => Display(false, this);
        public void Show() => Display(true, this);

        private void ShowAllLayout() => Display(true, TopUi, MidUi, GameUi, BtmUi);
        public void ResetUi()
        {
            ResetAllLayoutChildren();
            ShowAllLayout();
            HidePanel();
            HideWindows();
        }

        private void ResetAllLayoutChildren()
        {
            HideLayoutChildren(TopUi);
            HideLayoutChildren(MidUi);
            HideLayoutChildren(BtmUi);
            HideLayoutChildren(GameUi);
            HideLayoutChildren(Panel.transform);
            HideLayoutChildren(Toast);
        }

        private static void HideLayoutChildren(Transform tran)
        {
            foreach (Transform o in tran) o.gameObject.SetActive(false);
        }

        private void Display(bool display, params Component[] param)
        {
            for (var i = 0; i < param.Length; i++)
            {
                var com = param[i];
                com.gameObject.SetActive(display);
            }
        }

        private void SetUi(RectTransform tran, IView view, bool resetPos = false)
        {
            StartCoroutine(Set());
            IEnumerator Set()
            {
                var defaultActive = view.GameObject.activeSelf;
                view.GameObject.SetActive(false);
                _uiMap[tran] = view;
                view.GameObject.transform.SetParent(tran);
                if (resetPos)
                {
                    var rect = (RectTransform)(view.GameObject.transform);
                    rect.ResetToZero();
                    //((RectTransform)(view.GameObject.transform)).rect.size.Set(0, 0);
                }

                yield return new WaitForEndOfFrame();
                view.GameObject.SetActive(true);
                view.GameObject.SetActive(defaultActive);
            }
        }
    }
}