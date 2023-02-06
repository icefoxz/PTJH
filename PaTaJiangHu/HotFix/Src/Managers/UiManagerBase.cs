using Views;

namespace HotFix_Project.Managers;

/// <summary>
/// 基于MainPage的UiManagerBase 扩展类
/// </summary>
internal abstract class MainPageBase : UiManagerBase
{
    protected override UiManager.Sections Section => UiManager.Sections.MainPage;
    protected abstract MainPageLayout.Sections MainPageSection { get; }

    protected MainPageBase(UiManager uiManager) : base(uiManager)
    {
    }

    protected override void RegUiManager() => UiManager.SetToMainPage(this, View, MainPageSection, IsDynamicPixel);
}

/// <summary>
/// 所有Ui manager都必须继承的基础类, 主要用于注册与实现Ui上开关的统一调用
/// </summary>
internal abstract class UiManagerBase
{
    protected UiManager UiManager { get; }
    protected abstract UiManager.Sections Section { get; }
    protected abstract string ViewName { get; }

    /// <summary>
    /// 如果Ui是指定例:w 1080 h 1200, 设成true, 如果是根据硬件动态像素 = false
    /// </summary>
    protected abstract bool IsDynamicPixel { get; }
    protected IView View { get; private set; }

    protected UiManagerBase(UiManager uiManager)
    {
        UiManager = uiManager;
        Game.UiBuilder.Build(ViewName, v =>
        {
            Build(v);
            View = v;
        }, () =>
        {
            RegUiManager();
            RegEvents();
        });
    }

    /// <summary>
    /// 不去需要特别重写,仅仅是因为MainPage是特别处理才需要重新注册UiManager
    /// </summary>
    protected virtual void RegUiManager() => UiManager.SetToMainUi(this, View, Section, IsDynamicPixel);

    protected abstract void Build(IView view);
    protected abstract void RegEvents();

    /// <summary>
    /// 当请求显示Ui
    /// </summary>
    public abstract void Show();
    /// <summary>
    /// 当请求关闭Ui
    /// </summary>
    public abstract void Hide();
}