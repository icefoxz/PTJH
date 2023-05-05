using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

//门派
internal class Demo_Page_Faction : UiManagerBase
{
    private Faction_page FactionPage { get; set; }

    public Demo_Page_Faction(Demo_v1Agent uiAgent) : base(uiAgent)
    {
    }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Page;
    protected override string ViewName => "demo_page_faction";
    protected override bool IsDynamicPixel => true;

    protected override void Build(IView view)
    {
        FactionPage = new Faction_page(view, true);
    }

    protected override void RegEvents()
    {
    }

    public override void Show() => FactionPage.Display(true);

    public override void Hide() => FactionPage.Display(false);

    private class Faction_page : UiBase
    {
        public Faction_page(IView v, bool display) : base(v, display)
        {
        }
    }
}