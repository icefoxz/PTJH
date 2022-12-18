using HotFix_Project.Serialization.LitJson;
using Server;

namespace HotFix_Project.Managers;

internal class GameManager
{
    private AdventureManager AdventureManager { get; set; } = new AdventureManager();
    private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();
    private SkillTestManager SkillTestManager { get; set; } = new SkillTestManager();
    private AdvMapTestManager AdvMapTestManager { get; set; } = new AdvMapTestManager();
    private SimulationTestManager SimulationTestManager { get; set; } = new SimulationTestManager();

    public void Test()
    {
        AdventureManager.Init();
        DiziTestManager.Init();
        SkillTestManager.Init();
        AdvMapTestManager.Init();
        SimulationTestManager.Init();
    }
}