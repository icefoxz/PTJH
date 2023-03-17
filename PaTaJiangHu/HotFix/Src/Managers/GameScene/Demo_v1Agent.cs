
using _GameClient.Models;
using HotFix_Project.Managers.Demo_v1;
using System.Linq;
using Utls;

namespace HotFix_Project.Managers.GameScene;

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
    internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
    {
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
        //窗口 Windows
        Demo_Win_RewardMgr = new Demo_Win_RewardMgr(this);
        Demo_Win_ItemMgr = new Demo_Win_ItemMgr(this);
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
        Show(new UiManagerBase[]
        {
            Demo_View_ConsumeResMgr,
            Demo_View_ConPropsMgr,
            Demo_View_DiziActivityMgr,
            Demo_View_EquipmentMgr,
            Demo_Dizi_InfoMgr,
        });
    }

    public void MapSelection(string guid,int mapType)
    {
        Demo_View_AdventureMapsMgr.Set(guid, mapType);
        Demo_View_AdventureMapsMgr.Show();
    }
}