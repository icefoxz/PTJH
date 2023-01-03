using _GameClient.Models;
using UnityEngine;

namespace Server.Configs._script.Factions
{
    public class DiziController : IGameController
    {
        private Faction Faction => Game.World.Faction;

        public void SelectDizi(string guid)
        {
            var selectedDizi = Faction.DiziMap[guid];
            Game.MessagingManager.Send(EventString.Faction_DiziSelected, selectedDizi.Guid);
            Game.MessagingManager.Send(EventString.Dizi_AdvManagement, selectedDizi.Guid);
        }

        public void ManageDiziCondition(string guid)
        {
            Game.MessagingManager.Send(EventString.Dizi_ConditionManagement, guid);
            //var items = Game.MessagingManager.Send(EventString.Faction_ListDiziConditionItems,, Faction.Silver);
        }
    }
}