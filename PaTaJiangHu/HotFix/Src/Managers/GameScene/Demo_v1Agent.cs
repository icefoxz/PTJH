
using HotFix_Project.Managers.Demo_v1;
using HotFix_Project.Src.Managers.Demo_v1;

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
    internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
    {
        Demo_View_PageMgr = new Demo_View_PagesMgr(this);
        Demo_View_Faction_InfoMgr = new Demo_View_Faction_InfoMgr(this);
        Demo_Dizi_InfoMgr = new Demo_Dizi_InfoMgr(this);
        Demo_View_DiziListMgr = new Demo_View_DiziListMgr(this);
        Demo_View_ConsumeResMgr = new Demo_View_ConsumeResMgr(this);
        Demo_View_ConPropsMgr = new Demo_View_ConPropsMgr(this);
    }
}