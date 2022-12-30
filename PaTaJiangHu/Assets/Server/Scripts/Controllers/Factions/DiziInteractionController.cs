using System.Linq;
using _GameClient.Models;
using Utls;

namespace Server.Controllers.Factions
{
    public class DiziInteractionController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        public void SelectDizi(int index)
        {
            var dizi = Faction.DiziList[index];
            Game.MessagingManager.Send(EventString.Faction_DiziSelected, dizi);
            Game.MessagingManager.Send(EventString.Dizi_AdvManagement, dizi);
        }
    }
}