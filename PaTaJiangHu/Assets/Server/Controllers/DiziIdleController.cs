using _GameClient.Models;

namespace Server.Controllers
{

    public class DiziIdleController : IGameController
    {
        internal Config.DiziIdle IdleCfg => Game.Config.Idle;
        private Faction Faction => Game.World.Faction;

        public void QueryIdleStory(string guid)
        {
            var dizi = Faction.GetDizi(guid);
        }
    }
}