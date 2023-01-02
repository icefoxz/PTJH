using _GameClient.Models;
using UnityEngine;

namespace Server.Configs._script.Factions
{
    public class DiziController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        public void SelectDizi(int index)
        {
            var dizi = Faction.DiziList[index];
            Game.MessagingManager.Send(EventString.Faction_DiziSelected, new DiziDto(dizi));
            Game.MessagingManager.Send(EventString.Dizi_AdvManagement, new DiziDto(dizi));
        }

        public void ListDiziConditionItems()
        {
            //var items = Faction. Game.MessagingManager.Send(EventString.Faction_ListDiziConditionItems,, Faction.Silver);
        }
    }
}