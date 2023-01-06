using HotFix_Project.Serialization.LitJson;
using Server;
using Utls;

namespace HotFix_Project.Managers;

internal class TestUiManager
{
    private AdventureManager AdventureManager { get; set; } = new AdventureManager();
    private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();
    private SkillTestManager SkillTestManager { get; set; } = new SkillTestManager();
    private AdvMapTestManager AdvMapTestManager { get; set; } = new AdvMapTestManager();
    private SimulationTestManager SimulationTestManager { get; set; } = new SimulationTestManager();

    public void Test()
    {
        XDebug.Log($"{nameof(TestUiManager)}.{nameof(Test)} Run!");
        AdventureManager.Init();
        DiziTestManager.Init();
        SkillTestManager.Init();
        AdvMapTestManager.Init();
        SimulationTestManager.Init();
    }
}