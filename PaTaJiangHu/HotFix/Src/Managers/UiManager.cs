using HotFix_Project.Src.Managers;
using UnityEngine;

namespace HotFix_Project.Managers;

internal class UiManager
{
    private IMainUi MainUi { get; }
    private FactionInfoManager FactionInfo { get; set; } = new FactionInfoManager();
    private DiziInfoSectManager DiziInfoSectManager { get; set; } = new DiziInfoSectManager();
    private DiziRecruitManager DiziRecruitManager { get; set; } = new DiziRecruitManager();
    private DiziListViewManager DiziListViewManager { get; set; } = new DiziListViewManager();
    private DiziAdvManager DiziAdvManager { get; set; } = new DiziAdvManager();
    private WinConItemSelectorManager WinConItemSelectorManager { get; set; } = new WinConItemSelectorManager();
    private WinAdvConfirmManager WinAdvConfirmManager { get; set; } = new WinAdvConfirmManager();
    private WinEquipmentManager WinEquipmentManager { get; set; } = new WinEquipmentManager();
    private WinRewardManager WinRewardManager { get; set; } = new WinRewardManager();
    private BtmPageBtnsManager BtmPageBtnsManager { get; set; } = new BtmPageBtnsManager();
    private TreasureHouseManager TreasureHouseManager { get; set; } = new TreasureHouseManager();
    public UiManager(IMainUi mainUi)
    {
        MainUi = mainUi;
    }

    public void LoadAllUis()
    {
        InstanceTopUis();
        InstanceMidUis();
        InstanceBtmUis();
        Debug.Log($"{nameof(UiManager)} Init!");
    }

    private void InitMainLayoutUis()
    {
        //板块 section
        DiziInfoSectManager.Init();
        DiziRecruitManager.Init();
        DiziListViewManager.Init();
        DiziAdvManager.Init();
        TreasureHouseManager.Init();
        //窗口 windows
        WinConItemSelectorManager.Init();
        WinAdvConfirmManager.Init();
        WinEquipmentManager.Init();
        WinRewardManager.Init();
    }

    private void InstanceTopUis()
    {
        FactionInfo.Init();
    }

    private void InstanceBtmUis()
    {
        BtmPageBtnsManager.Init();
    }

    private void InstanceMidUis()
    {
        InitMainLayoutUis();
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

}