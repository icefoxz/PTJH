using HotFix_Project.Managers.GameScene;
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
        public Demo_View_PagesMgr(MainUiAgent uiAgent) : base(uiAgent) { }

        protected override void Build(IView view)
        {
            View_pages = new View_Pages(view,
                ()=> XDebug.LogWarning("打开宝物库"),
                ()=> XDebug.LogWarning("打开弟子页面"),
                ()=> XDebug.LogWarning("打开门派页面"),
                ()=> XDebug.LogWarning("打开弟子招募页面"));
        }
        protected override void RegEvents()
        {
        }
        public override void Show() => View_pages.Display(true);
        public override void Hide() => View_pages.Display(false);

        private class View_Pages : UiBase
        {
            private Element TreasureHouseElement { get; }
            private Element DiziElement { get; }
            private Element FactionElement { get; }
            private Element RecruitElement { get; }
            public View_Pages(IView v,
                Action onTreasureHouse,
                Action onDiziPage,
                Action onFactionPage,
                Action onRecruitPage) : base(v, true)
            {
                TreasureHouseElement = new Element(v.GetObject<View>("element_tresureHouse"),
                    () => onTreasureHouse?.Invoke());
                DiziElement = new Element(v.GetObject<View>("element_dizi"),
                    () => onDiziPage?.Invoke());
                FactionElement = new Element(v.GetObject<View>("element_faction"),
                    () => onFactionPage?.Invoke());
                //RecruitElement = new Element(v.GetObject<View>("element_recruit"), () => onRecruitPage?.Invoke());
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
