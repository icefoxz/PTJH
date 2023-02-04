using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utls;
using Views;

namespace HotFix_Project.Managers;

internal class UiManager
{

    public enum Sections
    {
        Top,
        Page,
        Bottom,
        Window,
        Panel,
        MainPage
    }

    private List<UiManagerBase> MainPageUis { get; } = new List<UiManagerBase>();
    private List<UiManagerBase> MainMidUis { get; } = new List<UiManagerBase>();
    //页面映像管理
    private List<UiMapper> UiMappers { get; }

    private IMainUi MainUi { get; }
    private FactionInfoManager FactionInfo { get; set; }
    private DiziInfoSectManager DiziInfoSectManager { get; set; }
    private DiziRecruitManager DiziRecruitManager { get; set; }
    private DiziListViewManager DiziListViewManager { get; set; }
    private DiziAdvManager DiziAdvManager { get; set; }
    private WinConItemSelectorManager WinConItemSelectorManager { get; set; }
    private WinAdvConfirmManager WinAdvConfirmManager { get; set; }
    private WinEquipmentManager WinEquipmentManager { get; set; }
    private WinRewardManager WinRewardManager { get; set; }
    private BtmPageBtnsManager BtmPageBtnsManager { get; set; }
    private TreasureHouseManager TreasureHouseManager { get; set; }

    public UiManager(IMainUi mainUi)
    {
        MainUi = mainUi;
        UiMappers = new List<UiMapper>();
    }

    public void RegPage<T>(Sections section, T manager) where T : UiManagerBase
    {
        if (!UiMappers.Any(m => m.Manager == manager && section == m.Section))
            UiMappers.Add(new UiMapper(section, typeof(T).Name, manager));
    }

    public void LoadAllUis()
    {
        //板块 section
        DiziInfoSectManager = new DiziInfoSectManager(this);
        DiziRecruitManager = new DiziRecruitManager(this);
        DiziListViewManager = new DiziListViewManager(this);
        DiziAdvManager = new DiziAdvManager(this);
        TreasureHouseManager = new TreasureHouseManager(this);
        //窗口 windows
        WinConItemSelectorManager = new WinConItemSelectorManager(this);
        WinAdvConfirmManager = new WinAdvConfirmManager(this);
        WinEquipmentManager = new WinEquipmentManager(this);
        WinRewardManager = new WinRewardManager(this);
        FactionInfo = new FactionInfoManager(this);
        BtmPageBtnsManager = new BtmPageBtnsManager(this);

        Debug.Log($"{nameof(UiManager)} Init!");
    }

    private void TestFactionInventory()
    {
        Game.UiBuilder.Build("view_fractionInventory", v =>
        {
            MainUi.SetPanel(v);
            var rect = (RectTransform)v.GameObject.transform;
            rect.sizeDelta = Vector2.zero;
            rect.pivot = Vector2.zero;
            MainUi.ShowPanel();
        },null);
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
        if (section == MainPageLayout.Sections.Mid)
            MainMidUis.Add(manager);
        RegPage(Sections.MainPage, manager);
        var mainPage = Game.MainUi.MainPage;
        mainPage.Set(view, section, isFixPixel);
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
                CloseMainPage();
                break;
            case Sections.MainPage:
                CloseAllUi(map, Sections.Page);
                if(MainMidUis.Contains(map.Manager))//如果是中间板块
                    foreach (var ui in MainMidUis)//关闭其它中间板块,仅留下当前显示板块
                    {
                        if(ui == map.Manager)
                            ui.Show();
                        else ui.Hide();
                    }

                break; //MainPage
            default:
                throw new ArgumentOutOfRangeException();
        }

        map.Manager.Show();

        void CloseMainPage() => MainPageUis.ForEach(p => p.Hide());
    }


    private void CloseAllUi(UiMapper map,Sections section)
    {
        foreach (var mapper in UiMappers.Where(m => m.Section == section).ToList())
        {
            if (mapper == map) continue;
            mapper.Manager.Hide();
        }
    }

    private class UiMapper
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
}