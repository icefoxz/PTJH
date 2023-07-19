﻿using AOT.Views.Abstract;
using GameClient.System;

namespace HotUpdate._HotUpdate
{
    /// <summary>
    /// 所有Ui manager都必须继承的基础类, 主要用于注册与实现Ui上开关的统一调用
    /// </summary>
    internal abstract class UiManagerBase
    {
        protected MainUiAgent MainUiAgent { get; }
        protected abstract MainUiAgent.Sections Section { get; }
        protected abstract string ViewName { get; }

        /// <summary>
        /// 如果Ui是指定例:w 1080 h 1200, 设成true, 如果是根据硬件动态像素 = false
        /// </summary>
        protected abstract bool IsDynamicPixel { get; }

        public IView View { get; private set; }

        protected UiManagerBase(MainUiAgent uiAgent)
        {
            MainUiAgent = uiAgent;
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
        protected virtual void RegUiManager() => MainUiAgent.SetToMainUi(this, View, Section, IsDynamicPixel);

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

    internal abstract class WinUiManagerBase : UiManagerBase
    {
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
        protected override bool IsDynamicPixel => true;

        protected WinUiManagerBase(MainUiAgent uiAgent) : base(uiAgent)
        {
        }

        public override void Hide()
        {
            View.Hide();
            MainUiAgent.HideWindows();
        }

        protected void HidePanel() => MainUiAgent.DisplayWindowPanel(false);

        public override void Show()
        {
            MainUiAgent.ShowWindow(View);
        }
    }

    internal abstract class PageUiManagerBase : UiManagerBase
    {
        protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Page;
        protected override bool IsDynamicPixel => true;

        protected PageUiManagerBase(MainUiAgent uiAgent) : base(uiAgent)
        {
        }

        public override void Hide() => View.Hide();

        public override void Show()
        {
            MainUiAgent.CloseAllPages();
            View.Show();
        }
    }
}