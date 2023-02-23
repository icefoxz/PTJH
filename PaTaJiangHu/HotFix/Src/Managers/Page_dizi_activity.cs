using Views;

namespace HotFix_Project.Managers;

internal class Page_dizi_activity :MainPageBase
{
    public Page_dizi_activity(UiManager uiManager) : base(uiManager)
    {
    }

    protected override string ViewName => "page_dizi_activity";
    protected override bool IsDynamicPixel => true;
    protected override void Build(IView view)
    {
        throw new System.NotImplementedException();
    }

    protected override void RegEvents()
    {
        throw new System.NotImplementedException();
    }

    public override void Show()
    {
        throw new System.NotImplementedException();
    }

    public override void Hide()
    {
        throw new System.NotImplementedException();
    }

    protected override MainPageLayout.Sections MainPageSection => MainPageLayout.Sections.Page;
}