using UnityEngine;

namespace HotFix_Project.Managers;

internal class UiManager
{
    private IMainUi MainUi { get; }
    private FactionInfoManager FactionInfo { get; set; } = new FactionInfoManager();
    private DiziInfoSection DiziInfoSection { get; set; } = new DiziInfoSection();
    private DiziRecruitManager DiziRecruitManager { get; set; } = new DiziRecruitManager();
    private DiziListViewManager DiziListViewManager { get; set; } = new DiziListViewManager();
    private DiziAdvManager DiziAdvManager { get; set; } = new DiziAdvManager();

    public UiManager(IMainUi mainUi)
    {
        MainUi = mainUi;
    }

    public void LoadAllUis()
    {
        InstanceTopUis();
        InstanceMidUis();
        InstanceBtmUis();
        InitUis();
        Debug.Log($"{nameof(UiManager)} Init!");
    }

    private void InitUis()
    {
        FactionInfo.Init();
        DiziInfoSection.Init();
        DiziRecruitManager.Init();
        DiziListViewManager.Init();
        DiziAdvManager.Init();
    }


    private void InstanceTopUis()
    {
        
    }

    private void InstanceBtmUis()
    {
        
    }

    private void InstanceMidUis()
    {
        
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
        });
    }

}