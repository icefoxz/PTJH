using HotFix_Project.Serialization.LitJson;
using Server;
using Utls;

namespace HotFix_Project.Managers;

internal class TestUiManager
{
    private TestAdventureManager TestAdventureManager { get; set; } = new TestAdventureManager();
    private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();
    private SkillTestManager SkillTestManager { get; set; } = new SkillTestManager();
    private AdvMapTestManager AdvMapTestManager { get; set; } = new AdvMapTestManager();
    private SimulationTestManager SimulationTestManager { get; set; } = new SimulationTestManager();
    private TestAutoAdvManager TestAutoAdvManager { get; set; } = new TestAutoAdvManager();

    public void Test()
    {
        XDebug.Log($"{nameof(TestUiManager)}.{nameof(Test)} Run!");
        TestAdventureManager.Init();
        DiziTestManager.Init();
        SkillTestManager.Init();
        AdvMapTestManager.Init();
        SimulationTestManager.Init();
        TestAutoAdvManager.Init();
    }
}