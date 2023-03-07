using Utls;

namespace HotFix_Project.Managers.GameScene;

internal class TestUiManager
{
    private TestAdventureManager TestAdventureManager { get; set; } = new TestAdventureManager();
    private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();
    private AdvMapTestManager AdvMapTestManager { get; set; } = new AdvMapTestManager();
    private TestAutoAdvManager TestAutoAdvManager { get; set; } = new TestAutoAdvManager();

    public void Test()
    {
        XDebug.Log($"{nameof(TestUiManager)}.{nameof(Test)} Run!");
        TestAdventureManager.Init();
        DiziTestManager.Init();
        AdvMapTestManager.Init();
        TestAutoAdvManager.Init();
    }
}