using System;
using System.Collections.Generic;
using System.Linq;
using Utls;
using Views;

namespace HotFix_Project.Managers.GameScene;
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

    protected MainPageManager MainPageMgr { get; } = new MainPageManager();
    protected List<UiManagerBase> MainPageUis { get; } = new List<UiManagerBase>();
    protected List<UiMapper> UiMappers { get; }
    protected IMainUi MainUi { get; }

    public enum Sections
    {
        Top,
        Page,
        Bottom,
        Window,
        Panel,
        MainPage
    }

    public void RegPage<T>(Sections section, T manager) where T : UiManagerBase
    {
        if (!UiMappers.Any(m => m.Manager == manager && section == m.Section))
            UiMappers.Add(new UiMapper(section, typeof(T).Name, manager));
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
            case Sections.MainPage:
                XDebug.LogError($"{view.name}注册异常! MainPage 不应该直接注册UiManager, 因为需要独立注册上中下, 所以得另处理!");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SetToMainPage(UiManagerBase manager, IView view, MainPageLayout.Sections section, bool isFixPixel)
    {
        MainPageUis.Add(manager);
        MainPageMgr.Reg(section, manager);
        RegPage(Sections.MainPage, manager);
        var mainPage = Game.MainUi.MainPage;
        mainPage.Set(view, section, isFixPixel);
    }

    public void Show<T>(Action<T> mgrAction) where T : UiManagerBase
    {
        var type = typeof(T);
        var map = UiMappers.First(m => m.Key == type.Name);
        mgrAction?.Invoke((T)map.Manager);
    }
    public void Show(UiManagerBase manager)
    {
        var map = UiMappers.FirstOrDefault(m => m.Manager == manager);
        OnShowUi(map);
    }
    public void Show(Type type)
    {
        var map = UiMappers.FirstOrDefault(m => m.Key == type.Name);
        OnShowUi(map);
    }

    private void OnShowUi(UiMapper map)
    {
        switch (map.Section)
        {
            case Sections.Top:
            case Sections.Bottom:
            case Sections.Panel:
            case Sections.Window:
            case Sections.Page:
                CloseAllUi(map, map.Section);
                MainUi.HideMainPage();
                map.Manager.Show();
                break;
            case Sections.MainPage:
                CloseAllUi(map, Sections.Page);
                MainUi.ShowMainPage();
                MainPageMgr.Show(map);
                break; //MainPage
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    private void CloseAllUi(UiMapper map, Sections section)
    {
        foreach (var mapper in UiMappers.Where(m => m.Section == section).ToList())
        {
            if (mapper == map) continue;
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

    protected class MainPageManager
    {
        private Dictionary<MainPageLayout.Sections, List<UiManagerBase>> Data { get; } =
            new Dictionary<MainPageLayout.Sections, List<UiManagerBase>>();
        private Dictionary<UiManagerBase, MainPageLayout.Sections> Nav { get; } =
            new Dictionary<UiManagerBase, MainPageLayout.Sections>();

        public void Show(UiMapper map)
        {
            foreach (var manager in Data[Nav[map.Manager]])
            {
                if(manager == map.Manager) continue;
                manager.Hide();
            }
            map.Manager.Show();
        }

        public void Reg(MainPageLayout.Sections section, UiManagerBase manager)
        {
            if (!Data.ContainsKey(section))
                Data.Add(section, new List<UiManagerBase>());
            if (!Nav.ContainsKey(manager))
                Nav.Add(manager, section);
            Data[section].Add(manager);
        }
    }
}