using Server;

namespace HotFix_Project.Managers
{
    internal class GameManager
    {
        public AdventureManager AdventureManager { get; private set; } = new AdventureManager();

        public void Init()
        {
            AdventureManager.Init();
        }
    }
}