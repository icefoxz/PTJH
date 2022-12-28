using UnityEngine;

namespace HotFix_Project.Managers;

internal class UiManager
{
    private IMainUi MainUi { get; }
    public UiManager(IMainUi mainUi)
    {
        MainUi = mainUi;
    }

    public void LoadAllUis()
    {
        InstanceTopUis();
        InstanceMidUis();
        InstanceBtmUis();
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