using System;
using _GameClient.Models;
using Server.Configs.Adventures;

namespace Server.Controllers
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

        /// <summary>
        /// 管理弟子的装备, itemType { 0 : weapon, 1 : armor }
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="itemType">0 = weapon, 1 = armor</param>
        public void ManageDiziEquipment(string guid, int itemType)
        {
            Game.MessagingManager.SendParams(EventString.Dizi_EquipmentManagement, guid, itemType);
        }

        /// <summary>
        /// 弟子装备物件, itemType { 0 : weapon, 1 : armor }
        /// </summary>
        /// <param name="guid">弟子guid</param>
        /// <param name="index">第几个物件</param>
        /// <param name="itemType">0 = weapon, 1 = armor</param>
        public void DiziEquip(string guid, int index, int itemType)
        {
            var dizi = Faction.DiziMap[guid];
            switch (itemType)
            {
                case 0:
                {
                    if (dizi.Weapon != null) Faction.AddWeapon(dizi.Weapon);
                    var weapon = Faction.Weapons[index];
                    dizi.Wield(weapon);
                    Faction.RemoveWeapon(weapon);
                    break;
                }
                case 1:
                {
                    if (dizi.Armor!= null) Faction.AddArmor(dizi.Armor);
                    var armor = Faction.Armors[index];
                    dizi.Wear(armor);
                    Faction.RemoveArmor(armor);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            Game.MessagingManager.Send(EventString.Dizi_ItemEquipped, dizi.Guid);
        }
        public void DiziUnEquipItem(string guid, int itemType)
        {
            var dizi = Faction.DiziMap[guid];
            switch (itemType)
            {
                case 0:
                {
                    var weapon = dizi.Weapon;
                    dizi.Wield(null);
                    Faction.AddWeapon(weapon);
                    break;
                }
                case 1:
                {
                    var armor = dizi.Armor;
                    dizi.Wear(null);
                    Faction.RemoveArmor(armor);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            Game.MessagingManager.Send(EventString.Dizi_ItemUnEquipped, string.Empty);
        }

        public void AddDiziCon(string guid, IAdjustment.Types type, int adjValue)
        {
            var dizi = Faction.DiziMap[guid];
            if (type == IAdjustment.Types.Stamina)
            {
                var staminaController = Game.Controllers.Get<StaminaController>();
                staminaController.ConsumeStamina(guid, adjValue, true);
            }
            else
            {
                dizi.SetCon(type, adjValue);
            }
        }
    }
}