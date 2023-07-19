using System;
using System.Linq;
using AOT.Utls;
using AOT.Views;
using AOT.Views.Abstract;
using GameClient.Controllers;
using GameClient.GameScene;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.System;
using HotUpdate._HotUpdate.Common;
using HotUpdate._HotUpdate.UiEffects;

namespace HotUpdate._HotUpdate.Demo_v1
{
    //Demo_v1 的Ui代理
    internal class Demo_v1Agent : MainUiAgent
    {
        public enum Pages
        {
            Main,
            Skills,
            Faction,
            Battle
        }

        //test recruitManger
        private DiziRecruitManager DiziRecruitManager { get; }

        // Page buttons
        private Demo_View_PagesMgr Demo_View_PageMgr { get; }

        // top
        private Demo_View_Faction_InfoMgr Demo_View_Faction_InfoMgr { get; }

        //page
        private Demo_Page_Main Demo_Page_Main { get; }
        private Demo_Page_Skill Demo_Page_Skill { get; }
        private Demo_Page_Faction Demo_Page_Faction { get; }
        private Demo_Page_Battle Demo_Page_Battle { get; }

        //window
        private Demo_Win_RewardMgr Demo_Win_RewardMgr { get; }
        private Demo_Win_ItemMgr Demo_Win_ItemMgr { get; }
        private Demo_Win_SkillComprehend Demo_Win_SkillComprehend { get; }
        private Demo_Win_ItemSelector Demo_Win_ItemSelector { get; }
        private Demo_Win_ChallengeMgr Demo_Win_ChallengeMgr { get; }
        private Demo_Win_DiziHouse Demo_Win_DiziHouse { get; }
        private Demo_Win_VisitorDizi Demo_Win_VisitorDizi { get; }

        //通用窗口, 一般上都是直接调用,不需要这里调用
        private Win_Info Win_Info { get; }
        private Win_Confirm Win_Confirm { get; }
        private ToastManager ToastManager { get; }

        private ChallengeStageController ChallengeController => Game.Controllers.Get<ChallengeStageController>();
        private IGame2DLand Game2D => Game.Game2DLand;

        internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
        {
            //注册战斗事件,实现战斗特效生成
            EffectView.OnInstance += OnEffectInstance;

            //页面管理器 btm btns
            Demo_View_PageMgr = new Demo_View_PagesMgr(this);

            //top
            Demo_View_Faction_InfoMgr = new Demo_View_Faction_InfoMgr(this);

            //Page
            Demo_Page_Main = new Demo_Page_Main(this);
            Demo_Page_Skill = new Demo_Page_Skill(this);
            Demo_Page_Faction = new Demo_Page_Faction(this);
            Demo_Page_Battle = new Demo_Page_Battle(this);
            DiziRecruitManager = new DiziRecruitManager(this);

            //窗口 Windows
            Demo_Win_RewardMgr = new Demo_Win_RewardMgr(this);
            Demo_Win_ItemMgr = new Demo_Win_ItemMgr(this);
            Demo_Win_SkillComprehend = new Demo_Win_SkillComprehend(this);
            Demo_Win_ItemSelector = new Demo_Win_ItemSelector(this);
            Demo_Win_ChallengeMgr = new Demo_Win_ChallengeMgr(this);
            Demo_Win_DiziHouse = new Demo_Win_DiziHouse(this);
            Demo_Win_VisitorDizi = new Demo_Win_VisitorDizi(this);

            Win_Info = new Win_Info(this);
            Win_Confirm = new Win_Confirm(this);
            ToastManager = new ToastManager(this);

            CloseAllPages();
            MainUi.ShowGame();
        }

        private Faction Faction => Game.World.Faction;

        private void OnEffectInstance(IEffectView view)
        {
            switch (view.name)
            {
                case "demo_game_charPopValue":
                    new Demo_game_charPopValue(view);
                    break;
                default: throw new ArgumentException($"找不到特效的控制类! view = {view.name}");
            }
        }

        private Dizi SelectedDizi { get; set; }

        // 门派页面
        internal void FactionPage_Show() => ShowPage(Pages.Faction);

        // 弟子活动页面
        internal void MainPage_Show(string guid)
        {
            ResolveValidDizi(guid);
            ShowPage(Pages.Main);
        }

        private void ResolveValidDizi(string guid)
        {
            if (!string.IsNullOrWhiteSpace(guid))
            {
                SelectedDizi = Faction.GetDizi(guid);

                // Check if the selected Dizi is no longer in the Faction.DiziList
                if (!Faction.DiziList.Any(dizi => dizi.Guid == guid))
                    // If DiziList is not empty, select the first Dizi, otherwise set SelectedDizi to null
                    SelectedDizi = Faction.DiziList.FirstOrDefault();
            }
            else
            {
                // If the input guid is null or whitespace, select the first Dizi from the Faction.DiziList or null if it's empty
                SelectedDizi = Faction.DiziList.FirstOrDefault();
            }
        }


        // 弟子技能页面
        internal void SkillPage_Show(string guid)
        {
            ResolveValidDizi(guid);
            ShowPage(Pages.Skills);
        }

        // 页面Ui布置
        private void ShowPage(Pages page)
        {
            ResolveValidDizi(SelectedDizi?.Guid);
            var dizi = SelectedDizi;
            if (dizi == null) //如果没有缓存弟子,就会获取门派中的第一个弟子
            {
                dizi = Faction.DiziList.FirstOrDefault();
                if (dizi == null)
                {
                    XDebug.LogWarning("当前门派并没有弟子!");
                    return;
                }
            }

            switch (page)
            {
                case Pages.Main:
                    Demo_Page_Main.Set(dizi);
                    Demo_Page_Main.Show();
                    Game2D.PlayDizi(dizi.Guid);
                    Game2D.SwitchPage(CameraFocus.Focus.DiziView);
                    break;
                case Pages.Skills:
                    Demo_Page_Skill.SetDizi(dizi);
                    Demo_Page_Skill.Show();
                    break;
                case Pages.Faction:
                    Demo_Page_Faction.Show();
                    Game2D.SwitchPage(CameraFocus.Focus.FactionView);
                    break;
                case Pages.Battle:
                    //Game2D
                    Demo_Page_Battle.Show();
                    Game2D.PlayDizi(dizi.Guid);
                    Game2D.SwitchPage(CameraFocus.Focus.DiziView);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }

        /// <summary>
        /// 开始关卡挑战
        /// </summary>
        /// <param name="challengeIndex"></param>
        internal void Dizi_ChallengeStart(int challengeIndex)
        {
            ShowPage(Pages.Main);
            ChallengeController.ChallengeStart(SelectedDizi.Guid, challengeIndex);
        }

        /// <summary>
        /// 领悟技能(有目标技能)
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="type"></param>
        /// <param name="index"></param>
        public void Skill_Comprehend(string guid, SkillType type, int index)
        {
            SelectedDizi = Faction.GetDizi(guid);
            Demo_Win_SkillComprehend.Set(SelectedDizi, type, index);
            MainUi.ShowWindow(Demo_Win_SkillComprehend.View);
        }

        public void Skill_BookComprehend(string guid, SkillType type, int index)
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
        public void Win_EquipmentManagement(string guid, int itemType) => Demo_Win_ItemMgr.Set(guid, itemType, slot: 0);

        public void Win_ChallengeRequestNew() => ChallengeController.RequestNewChallenge();

        public void Win_ChallengeAbandon() => ChallengeController.ChallengeAbandon();

        public void Win_ChallengeWindow() => Demo_Win_ChallengeMgr.ShowChallengeWindow();

        public void Win_ChallengeReward() => ChallengeController.GetReward();

        public void Redirect_MainPage_ChallengeSelector() => ShowPage(Pages.Main);

        public void ShowDiziHouse()=> Demo_Win_DiziHouse.Show();

        public void ShowVisitor()
        {
            var visitor = Game.World.Recruiter.CurrentVisitor;
            Demo_Win_VisitorDizi.Set(visitor.Dizi, visitor.Set);
        }
    }
}