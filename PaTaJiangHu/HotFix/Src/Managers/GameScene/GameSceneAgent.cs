using HotFix_Project.Managers.Demo_v1;
using UnityEngine;

namespace HotFix_Project.Managers.GameScene;

//GameScene的Ui代理
internal class GameSceneAgent : MainUiAgent
{
    //页面映像管理

    private FactionInfoManager FactionInfo { get; set; }
    private DiziInfoSectManager DiziInfoSectManager { get; set; }//mainPage
    private DiziRecruitManager DiziRecruitManager { get; set; }//mainPage
    private DiziListViewManager DiziListViewManager { get; set; }//mainPage
    private DiziAdvManager DiziAdvManager { get; set; }//mainPage
    private WinConItemSelectorManager WinConItemSelectorManager { get; set; }
    private WinAdvConfirmManager WinAdvConfirmManager { get; set; }
    private WinEquipmentManager WinEquipmentManager { get; set; }
    private WinRewardManager WinRewardManager { get; set; }
    private BtmPageBtnsManager BtmPageBtnsManager { get; set; }
    private TreasureHouseManager TreasureHouseManager { get; set; }

    public GameSceneAgent(IMainUi mainUi):base(mainUi)
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
        Debug.Log($"{nameof(GameSceneAgent)} RegEvents!");
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