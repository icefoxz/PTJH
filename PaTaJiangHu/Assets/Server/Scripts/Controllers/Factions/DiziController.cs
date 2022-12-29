using System.Linq;
using _GameClient.Models;
using Utls;

namespace Server.Controllers.Factions
{
    public class DiziController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        public void SelectDizi(int index)
        {
            var dizi = Faction.DiziList[index];
            Game.MessagingManager.Invoke(EventString.Faction_DiziSelected,ObjectBag.Serialize(dizi));
        }

        public class DiziInfo
        {
            public string Name { get; set; }

            public DiziInfo(string name)
            {
                Name = name;
            }

            public DiziInfo()
            {
                
            }
        }
    }
}