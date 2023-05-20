using System;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.UiEffects;
using Models;
using Server.Configs.Skills;
using Server.Controllers;
using Utls;
using Views;

namespace HotFix_Project.Managers.Demo_v1;

//Demo_v1 的Ui代理
internal class Demo_v1Agent : MainUiAgent
{
    public enum Pages
    {
        Main,
        Skills,
        Faction,
    }

    //main
    private Demo_View_PagesMgr Demo_View_PageMgr { get; }
    private Demo_View_Faction_InfoMgr Demo_View_Faction_InfoMgr { get; }
    private Demo_Dizi_InfoMgr Demo_Dizi_InfoMgr { get; }
    private Demo_View_DiziListMgr Demo_View_DiziListMgr { get; }
    private Demo_View_ConsumeResMgr Demo_View_ConsumeResMgr { get; }
    private Demo_View_ConPropsMgr Demo_View_ConPropsMgr { get; }
    private Demo_View_DiziActivityMgr Demo_View_DiziActivityMgr { get; }
    private DiziRecruitManager DiziRecruitManager { get; }
    private Demo_View_EquipmentMgr Demo_View_EquipmentMgr { get; }
    private Demo_View_AdventureMapsMgr Demo_View_AdventureMapsMgr { get; }
    private Demo_Game_ViewMgr Demo_Game_ViewMgr { get; }
    private Demo_View_ChallengeStageSelectorMgr Demo_View_ChallengeStageSelectorMgr { get; }
    private Demo_Game_BattleBannerMgr Demo_Game_BattleBannerMgr { get; }

    //page
    private Demo_Page_Skill Demo_Page_Skill { get; }
    private Demo_Page_Faction Demo_Page_Faction { get; }

    //window
    private Demo_Win_RewardMgr Demo_Win_RewardMgr { get; }
    private Demo_Win_ItemMgr Demo_Win_ItemMgr { get; }
    private Demo_Win_SkillComprehend Demo_Win_SkillComprehend { get; }
    private Demo_Win_ItemSelector Demo_Win_ItemSelector { get; }
    private Demo_Win_ChallengeMgr Demo_Win_ChallengeMgr { get; }

    private ChallengeStageController ChallengeController => Game.Controllers.Get<ChallengeStageController>();
    private BattleController BattleController => Game.Controllers.Get<BattleController>();
    private SkillController SkillController => Game.Controllers.Get<SkillController>();
    private GameStageController GameStageController => Game.Controllers.Get<GameStageController>();
    private IGame2DLand Game2D => Game.Game2DLand;

    internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
    {
        //注册战斗事件,实现战斗特效生成
        EffectView.OnInstance += OnEffectInstance;

        //页面管理器
        Demo_View_PageMgr = new Demo_View_PagesMgr(this);

        //main板块管理器
        Demo_View_Faction_InfoMgr = new Demo_View_Faction_InfoMgr(this);
        Demo_Dizi_InfoMgr = new Demo_Dizi_InfoMgr(this);
        Demo_View_DiziListMgr = new Demo_View_DiziListMgr(this);
        Demo_View_ConsumeResMgr = new Demo_View_ConsumeResMgr(this);
        Demo_View_ConPropsMgr = new Demo_View_ConPropsMgr(this);
        Demo_View_DiziActivityMgr = new Demo_View_DiziActivityMgr(this);
        Demo_View_EquipmentMgr = new Demo_View_EquipmentMgr(this);
        Demo_View_AdventureMapsMgr = new Demo_View_AdventureMapsMgr(this);
        Demo_Game_ViewMgr = new Demo_Game_ViewMgr(this);
        Demo_Game_BattleBannerMgr = new Demo_Game_BattleBannerMgr(this);
        Demo_View_ChallengeStageSelectorMgr = new Demo_View_ChallengeStageSelectorMgr(this);

        //Page
        Demo_Page_Skill = new Demo_Page_Skill(this);
        Demo_Page_Faction = new Demo_Page_Faction(this);
        DiziRecruitManager = new DiziRecruitManager(this);

        //窗口 Windows
        Demo_Win_RewardMgr = new Demo_Win_RewardMgr(this);
        Demo_Win_ItemMgr = new Demo_Win_ItemMgr(this);
        Demo_Win_SkillComprehend = new Demo_Win_SkillComprehend(this);
        Demo_Win_ItemSelector = new Demo_Win_ItemSelector(this);
        Demo_Win_ChallengeMgr = new Demo_Win_ChallengeMgr(this);

        CloseAllPages();
    }

    private Faction Faction => Game.World.Faction;

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
    /// <param name="page"></param>
    internal void SetDiziView(string guid = null, Pages page = Pages.Main)
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

            guid = dizi.Guid;
        }

        //main
        Game2D.PlayDizi(dizi.Guid);

        Demo_View_ConsumeResMgr.Set(dizi);
        Demo_View_ConPropsMgr.Set(dizi);
        Demo_View_DiziActivityMgr.Set(dizi);
        Demo_View_EquipmentMgr.Set(dizi);
        Demo_Dizi_InfoMgr.Set(dizi);
        Demo_Game_ViewMgr.Set(dizi);
        Demo_View_AdventureMapsMgr.Hide();

        //skill
        Demo_Page_Skill.SetDizi(dizi);
        ShowPage(page);
    }

    internal void OpenFactionPage() => ShowPage(Pages.Faction);
    private void ShowPage(Pages page)
    {
        switch (page)
        {
            case Pages.Main:
                SelectedDizi ??= Game.World.Faction.DiziList.FirstOrDefault();
                Show(new UiManagerBase[]
                {
                    Demo_View_ConsumeResMgr,
                    Demo_View_ConPropsMgr,
                    Demo_View_DiziActivityMgr,
                    Demo_View_EquipmentMgr,
                    Demo_Dizi_InfoMgr,
                }, true);
                Game2D.SwitchPage(CameraFocus.Focus.DiziView);
                break;
            case Pages.Skills:
                SelectedDizi ??= Game.World.Faction.DiziList.FirstOrDefault();
                Show(new UiManagerBase[]
                {
                    Demo_Page_Skill,
                }, true);
                break;
            case Pages.Faction:
                Show(new UiManagerBase[]
                {
                    Demo_Page_Faction,
                }, true);
                Game2D.SwitchPage(CameraFocus.Focus.FactionView);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(page), page, null);
        }
    }

    /// <summary>
    /// 历练地图选择
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="mapType"></param>
    internal void AdvMapSelection(string guid,int mapType)
    {
        ShowPage(Pages.Main);
        Demo_View_AdventureMapsMgr.Set(guid, mapType);
        Demo_View_AdventureMapsMgr.Show();
    }

    /// <summary>
    /// 获取挑战关卡并打开选择窗口
    /// </summary>
    internal void ShowChallengeState()
    {
        ShowPage(Pages.Main);
        Show(Demo_View_ChallengeStageSelectorMgr, false);
        Demo_View_ChallengeStageSelectorMgr.UpdateChallengeNpcs();
    }

    /// <summary>
    /// 开始关卡挑战
    /// </summary>
    /// <param name="challengeIndex"></param>
    internal void StartChallengeBattle(int challengeIndex)
    {
        ShowPage(Pages.Main);
        ChallengeController.StartChallenge(SelectedDizi.Guid, challengeIndex);
        Demo_Game_ViewMgr.Hide();
    }

    /// <summary>
    /// 战斗结束,ui转场
    /// </summary>
    public void SetBattleFinalize()
    {
        BattleController.FinalizeBattle();
        Demo_Game_ViewMgr.Show();
        Demo_Game_ViewMgr.Set(SelectedDizi);
        Demo_Game_BattleBannerMgr.Reset();
    }

    /// <summary>
    /// 领悟技能(有目标技能)
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="type"></param>
    /// <param name="index"></param>
    public void SetSkillComprehend(string guid, SkillType type, int index)
    {
        SelectedDizi = Faction.GetDizi(guid);
        Demo_Win_SkillComprehend.Set(SelectedDizi, type, index);
        MainUi.ShowWindow(Demo_Win_SkillComprehend.View);
    }

    public void SetBookComprehend(string guid, SkillType type, int index)
    {
        SelectedDizi = Faction.GetDizi(guid);
        var maps = SelectedDizi.Skill.Maps.ToArray();
        var books = Faction.GetBooksForSkill(type);
        var items = books.Where(b => maps.All(m => !m.IsThis(b.GetSkill()))).Select(b => (b.Id, b.Name, b.Icon))
            .ToArray();
        Demo_Win_ItemSelector.SetItems(items, OnBookConfirmed);
        MainUi.ShowWindow(Demo_Win_ItemSelector.View);

        void OnBookConfirmed(int id)
        {
            var book = Faction.GetBook(id);
            Demo_Win_SkillComprehend.Set(SelectedDizi, book.GetSkill(), index);
            MainUi.ShowWindow(Demo_Win_SkillComprehend.View);
        }
    }

    /// <summary>
    /// 装备管理,(窗口替换装备)
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="itemType"></param>
    public void EquipmentManagement(string guid, int itemType) => Demo_Win_ItemMgr.Set(guid, itemType, slot: 0);

    public void RequestNewChallenge() => ChallengeController.RequestNewChallenge();

    public void ChallengeGiveUp()
    {
        ChallengeController.RequestChallengeGiveup();
    }

    public void PromptChallengeWindow() => Demo_Win_ChallengeMgr.ShowChallengeWindow();

    public void PromptChallengeReward() => ChallengeController.GetReward();
}