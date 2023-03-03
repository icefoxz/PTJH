
using HotFix_Project.Managers.Demo_v1;

namespace HotFix_Project.Managers.GameScene;

//Demo_v1 的Ui代理
internal class Demo_v1Agent : MainUiAgent
{
    private Demo_View_PagesMgr Demo_View_PageMgr { get; }
    internal Demo_v1Agent(IMainUi mainUi) : base(mainUi)
    {
        Demo_View_PageMgr = new Demo_View_PagesMgr(this);
    }
}