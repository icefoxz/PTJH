using Server;

namespace HotFix_Project.Managers
{
    internal class GameManager
    {
        private AdventureManager AdventureManager { get; set; } = new AdventureManager();
        private DiziTestManager DiziTestManager { get; set; } = new DiziTestManager();

        public void Init()
        {
            AdventureManager.Init();
            DiziTestManager.Init();
        }
    }
}