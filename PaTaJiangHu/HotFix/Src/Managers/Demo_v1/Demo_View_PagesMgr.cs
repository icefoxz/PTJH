using HotFix_Project.Views.Bases;
using System;
using Systems.Messaging;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1
{
    
    internal class Demo_View_PagesMgr : UiManagerBase
    {
        private View_Pages View_pages { get; set; }
        protected override MainUiAgent.Sections Section { get; } = MainUiAgent.Sections.Bottom;
        protected override string ViewName { get; } = "demo_view_pages";
        protected override bool IsDynamicPixel { get; } = true;
        private Demo_v1Agent DemoAgent { get; set; }

        public Demo_View_PagesMgr(Demo_v1Agent uiAgent) : base(uiAgent)
        {
            DemoAgent = uiAgent;
        }

        protected override void Build(IView view)
        {
            View_pages = new View_Pages(view,
                onShengongPage: () =>
                {
                    DemoAgent.SkillPage_Show(null);
                }, //MainUiAgent.Show<TreasureHouseManager>(m=> m.Show()),
                onXiuxingPage: () => { DemoAgent.DiziPage_Show(null); },
                onFactionPage: () =>
                {
                    DemoAgent.FactionPage_Show();
                },
                onRecruitPage: () => Game.MessagingManager.Send(EventString.Page_DiziRecruit, null)
            );
        }

        protected override void RegEvents()
        {
        }

        public override void Show() => View_pages.Display(true);
        public override void Hide() => View_pages.Display(false);

        private class View_Pages : UiBase
        {
            private Element ShengongElement { get; }
            private Element XiuxingElement { get; }
            private Element FactionElement { get; }
            private Element RecruitElement { get; }

            public View_Pages(IView v,
                Action onShengongPage,
                Action onXiuxingPage,
                Action onFactionPage,
                Action onRecruitPage) : base(v, true)
            {
                FactionElement = new Element(v.GetObject<View>("element_faction"), () => onFactionPage?.Invoke());
                ShengongElement = new Element(v.GetObject<View>("element_shengong"), () => onShengongPage?.Invoke());
                XiuxingElement = new Element(v.GetObject<View>("element_xiuxing"), () => onXiuxingPage?.Invoke());
                RecruitElement = new Element(v.GetObject<View>("element_recruit"), () => onRecruitPage?.Invoke());
            }

            private class Element : UiBase
            {
                private Button btn_page { get; }

                public Element(IView v, Action onClickAction) : base(v, true)
                {
                    btn_page = v.GetObject<Button>("btn_page");
                    btn_page.OnClickAdd(onClickAction);
                }
            }
        }
    }
}