using System;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Managers.GameScene;
using HotFix_Project.UiEffects;
using Server.Controllers;
using UnityEngine;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

//Demo_v1 的Ui代理
internal class Demo_v1Agent : MainUiAgent
{
    private Demo_View_PagesMgr Demo_View_PageMgr { get; }
    private Demo_View_Faction_InfoMgr Demo_View_Faction_InfoMgr { get; }
    private Demo_Dizi_InfoMgr Demo_Dizi_InfoMgr { get; }
    private Demo_View_DiziListMgr Demo_View_DiziListMgr { get; }
    private Demo_View_ConsumeResMgr Demo_View_ConsumeResMgr { get; }
    private Demo_View_ConPropsMgr Demo_View_ConPropsMgr { get; }
    private Demo_View_DiziActivityMgr Demo_View_DiziActivityMgr { get; }
    private Demo_Win_RewardMgr Demo_Win_RewardMgr { get; }
    private Demo_Win_ItemMgr Demo_Win_ItemMgr { get; }
    private DiziRecruitManager DiziRecruitManager { get; }
    private Demo_View_EquipmentMgr Demo_View_EquipmentMgr { get; }
    private Demo_View_AdventureMapsMgr Demo_View_AdventureMapsMgr { get; }
    private Demo_Game_ViewMgr Demo_Game_ViewMgr { get; }
    private Demo_View_ChallengeStageSelectorMgr Demo_View_ChallengeStageSelectorMgr { get; }
    private Demo_Game_BattleBannerMgr Demo_Game_BattleBannerMgr { get; }

    private ChallengeStageController ChallengeController => Game.Controllers.Get<ChallengeStageController>();

    internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
    {
        //注册战斗事件,实现战斗特效生成
        EffectView.OnInstance += OnEffectInstance;
        //窗口管理器
        Demo_View_PageMgr = new Demo_View_PagesMgr(this);
        Demo_View_Faction_InfoMgr = new Demo_View_Faction_InfoMgr(this);
        Demo_Dizi_InfoMgr = new Demo_Dizi_InfoMgr(this);
        Demo_View_DiziListMgr = new Demo_View_DiziListMgr(this);
        Demo_View_ConsumeResMgr = new Demo_View_ConsumeResMgr(this);
        Demo_View_ConPropsMgr = new Demo_View_ConPropsMgr(this);
        Demo_View_DiziActivityMgr = new Demo_View_DiziActivityMgr(this);
        Demo_View_EquipmentMgr = new Demo_View_EquipmentMgr(this);
        DiziRecruitManager = new DiziRecruitManager(this);
        Demo_View_AdventureMapsMgr = new Demo_View_AdventureMapsMgr(this);
        Demo_Game_ViewMgr = new Demo_Game_ViewMgr(this);
        Demo_Game_BattleBannerMgr = new Demo_Game_BattleBannerMgr(this);

        Demo_View_ChallengeStageSelectorMgr = new Demo_View_ChallengeStageSelectorMgr(this);
        //窗口 Windows
        Demo_Win_RewardMgr = new Demo_Win_RewardMgr(this);
        Demo_Win_ItemMgr = new Demo_Win_ItemMgr(this);
        
    }

    private void OnEffectInstance(IEffectView view)
    {
        switch (view.name)
        {
            case "demo_game_charPopValue": new Demo_game_charPopValue(view);
                break;
            default: throw new ArgumentException($"找不到特效的控制类! view = {view.name}");
        }
    }

    private Dizi SelectedDizi { get; set; }
    /// <summary>
    /// 弟子信息(相关板块)显示, 作为主页上整合显示所有板块的方法
    /// </summary>
    /// <param name="guid"></param>
    internal void SetDiziView(string guid = null)
    {
        if (guid != null)
        {
            SelectedDizi = Game.World.Faction.GetDizi(guid);
        }
        var dizi = SelectedDizi;
        if (dizi == null) //如果没有缓存弟子,就会获取门派中的第一个弟子
        {
            dizi = Game.World.Faction.DiziList.FirstOrDefault();
            if (dizi == null)
            {
                XDebug.LogWarning("当前门派并没有弟子!");
                return;
            }
        }
        Demo_View_ConsumeResMgr.Set(dizi);
        Demo_View_ConPropsMgr.Set(dizi);
        Demo_View_DiziActivityMgr.Set(dizi);
        Demo_View_EquipmentMgr.Set(dizi);
        Demo_Dizi_InfoMgr.Set(dizi);
        Demo_Game_ViewMgr.Set(dizi);
        Demo_View_AdventureMapsMgr.Hide();
        Show(new UiManagerBase[]
        {
            Demo_View_ConsumeResMgr,
            Demo_View_ConPropsMgr,
            Demo_View_DiziActivityMgr,
            Demo_View_EquipmentMgr,
            Demo_Dizi_InfoMgr,
        }, true);
    }

    /// <summary>
    /// 历练地图选择
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="mapType"></param>
    internal void AdvMapSelection(string guid,int mapType)
    {
        Demo_View_AdventureMapsMgr.Set(guid, mapType);
        Demo_View_AdventureMapsMgr.Show();
    }

    /// <summary>
    /// 获取挑战关卡并打开选择窗口
    /// </summary>
    internal void ShowChallengeState()
    {
        Show(Demo_View_ChallengeStageSelectorMgr, false);
        var challenges = ChallengeController.GetChallenges();
        Demo_View_ChallengeStageSelectorMgr.SetChallenges(challenges);
    }

    /// <summary>
    /// 开始关卡挑战
    /// </summary>
    /// <param name="challengeIndex"></param>
    internal void StartChallenge(int challengeIndex)
    {
        ChallengeController.StartChallenge(SelectedDizi.Guid, challengeIndex);
        Demo_Game_ViewMgr.Hide();
    }

    public void SetBattleFinalize()
    {
        ChallengeController.FinalizeBattle();
        Demo_Game_ViewMgr.Show();
        Demo_Game_ViewMgr.Set(SelectedDizi);
        Demo_Game_BattleBannerMgr.Reset();
    }
}