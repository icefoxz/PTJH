using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Views;
using AOT.Views.Abstract;

namespace HotUpdate._HotUpdate
{
    /// <summary>
    /// 所有UiManager的代理,主要是管理MainUi上的操作
    /// </summary>
    internal class MainUiAgent
    {
        protected MainUiAgent(IMainUi mainUi)
        {
            MainUi = mainUi;
            UiMappers = new List<UiMapper>();
        }

        protected List<UiMapper> UiMappers { get; }
        protected IMainUi MainUi { get; }

        public enum Sections
        {
            Top,
            Page,
            Bottom,
            Window,
            Game,
            Panel,
            Toast
        }

        private void RegPage<T>(Sections section, T manager) where T : UiManagerBase
        {
            if (!UiMappers.Any(m => m.Manager == manager && section == m.Section))
                UiMappers.Add(new UiMapper(section, manager.GetType().Name, manager));
        }

        public void SetToMainUi(UiManagerBase manager, IView view, Sections section, bool isFixPixel)
        {
            RegPage(section, manager);
            switch (section)
            {
                case Sections.Top:
                    MainUi.SetTop(view, isFixPixel);
                    break;
                case Sections.Page:
                    MainUi.SetMid(view, isFixPixel);
                    break;
                case Sections.Bottom:
                    MainUi.SetBtm(view, isFixPixel);
                    break;
                case Sections.Window:
                    MainUi.SetWindow(view, isFixPixel);
                    break;
                case Sections.Panel:
                    MainUi.SetPanel(view);
                    break;
                case Sections.Game:
                    MainUi.SetGame(view);
                    break;
                case Sections.Toast:
                    MainUi.SetToast(view);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Show(UiManagerBase manager)
        {
            var map = UiMappers.FirstOrDefault(m => m.Manager == manager);
            OnShowUi(map.Section, map);
        }

        public void ShowWindow(IView view) => MainUi.ShowWindow(view);
        public void DisplayWindowPanel(bool display) => MainUi.DisplayWindowPanel(display);

        private void OnShowUi(Sections section, params UiMapper[] maps)
        {
            if (section is Sections.Page)
                CloseAllUi(section, maps);
            foreach (var mapper in maps) mapper.Manager.Show();
        }

        public void CloseAllPages() => CloseAllUi(Sections.Page);

        private void CloseAllUi(Sections section, params UiMapper[] skipsMaps)
        {
            foreach (var mapper in UiMappers.Where(m => m.Section == section).ToList())
            {
                if (skipsMaps.Contains(mapper)) continue;
                mapper.Manager.Hide();
            }
        }

        protected class UiMapper
        {
            public Sections Section { get; }
            public string Key { get; }
            public UiManagerBase Manager { get; }

            public UiMapper(Sections section, string key, UiManagerBase manager)
            {
                Section = section;
                Key = key;
                Manager = manager;
            }
        }

        public void HideWindows() => MainUi.HideWindows();
    }
}