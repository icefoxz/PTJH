using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;


public interface IMainUi : ISingletonDependency
{
    void SetTop(IView view);
    void SetMid(IView view);
    void SetBtm(IView view);
    void SetPanel(IView view);
    void ShowTop();
    void ShowMid();
    void ShowBtm();
    void ShowPanel();
    void HideTop();
    void HideMid();
    void HideBtm();
    void HidePanel();
    void HideAllLayout();
    void ShowAllLayout();
    void HideAll();
    void ShowAll();
    void Hide();
    void Show();
    void ResetUi();
}

/// <summary>
/// 主Ui
/// </summary>
public class MainUi : DependencySingleton<IMainUi>, IMainUi
{
    [SerializeField] private RectTransform _topUi;
    [SerializeField] private RectTransform _midUi;
    [SerializeField] private RectTransform _btmUi;
    [SerializeField] private Image _panel;

    public RectTransform TopUi => _topUi;
    public RectTransform MidUi => _midUi;
    public RectTransform BtmUi => _btmUi;
    public Image Panel => _panel;
    private Dictionary<RectTransform,IView> _uiMap;

    public void Init()
    {
        _uiMap = new Dictionary<RectTransform, IView>
        {
            { _panel.rectTransform, null },
            { _topUi, null },
            { _midUi, null },
            { _btmUi, null },
        };
        ResetUi();
    }

    public void SetTop(IView view) => SetUi(TopUi, view);
    public void SetMid(IView view) => SetUi(MidUi, view);
    public void SetBtm(IView view) => SetUi(BtmUi, view);
    public void SetPanel(IView view) => SetUi(Panel.rectTransform, view);

    public void ShowTop() => Display(true, TopUi);
    public void ShowMid() => Display(true, MidUi);
    public void ShowBtm() => Display(true, BtmUi);
    public void ShowPanel() => Display(true, Panel);
    public void HideTop() => Display(false, TopUi);
    public void HideMid() => Display(false, MidUi);
    public void HideBtm() => Display(false, BtmUi);
    public void HidePanel() => Display(false, Panel);
    public void HideAllLayout() => Display(false, TopUi, MidUi, BtmUi);
    public void ShowAllLayout() => Display(true, TopUi, MidUi, BtmUi);
    public void HideAll() => Display(false, TopUi, MidUi, BtmUi, Panel);
    public void ShowAll() => Display(true, TopUi, MidUi, BtmUi, Panel);

    public void Hide() => Display(false, this);
    public void Show() => Display(true, this);

    public void ResetUi()
    {
        ShowAllLayout();
        HidePanel();
    }

    private void Display(bool display,params Component[] param)
    {
        for (var i = 0; i < param.Length; i++)
        {
            var com = param[i];
            com.gameObject.SetActive(display);
        }
    }

    private void SetUi(RectTransform tran, IView view)
    {
        _uiMap[tran] = view;
        view.GameObject.transform.SetParent(tran);
    }
}
