using HotFix_Project.Managers.GameScene;
using HotFix_Project.Views.Bases;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    internal class Demo_View_PagesMgr : UiManagerBase
    {
        private View_Pages View_pages { get; set; }
        protected override MainUiAgent.Sections Section { get; } = MainUiAgent.Sections.Bottom;
        protected override string ViewName { get; } = "demo_view_pages";
        protected override bool IsDynamicPixel { get; } = true;

        public Demo_View_PagesMgr(MainUiAgent uiAgent) : base(uiAgent) { }

        protected override void Build(IView view)
        {
            View_pages = new View_Pages(view);
        }

        protected override void RegEvents()
        {
        }

        public override void Show() => View_pages.Display(true);

        public override void Hide() => View_pages.Display(false);

        private class View_Pages : UiBase
        {
            public View_Pages(IView v) : base(v, true)
            {
            }
        }
    }
}
