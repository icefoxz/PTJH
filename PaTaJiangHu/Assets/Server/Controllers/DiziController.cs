using System;
using _GameClient.Models;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Server.Configs.Characters;
using Server.Configs.Items;
using UnityEngine;
using Utls;

namespace Server.Controllers
{
    public class DiziController : IGameController
    {
        private LevelConfigSo LevelConfig => Game.Config.DiziCfg.LevelConfigSo;
        private PropStateConfigSo PropStateCfg => Game.Config.DiziCfg.PropState;
        private Faction Faction => Game.World.Faction;

        public (string title, Color color) GetSilverCfg(double ratio) => PropStateCfg.GetSilverCfg(ratio);
        public (string title, Color color) GetFoodCfg(double ratio) => PropStateCfg.GetFoodCfg(ratio);
        public (string title, Color color) GetEmotionCfg(double ratio) => PropStateCfg.GetEmotionCfg(ratio);
        public (string title, Color color) GetInjuryCfg(double ratio) => PropStateCfg.GetInjuryCfg(ratio);
        public (string title, Color color) GetInnerCfg(double ratio) => PropStateCfg.GetInnerCfg(ratio);

        public void SelectDizi(string guid)
        {
            var selectedDizi = Faction.GetDizi(guid);
            Game.MessagingManager.Send(EventString.Faction_DiziSelected, selectedDizi.Guid);
            Game.MessagingManager.Send(EventString.Dizi_AdvManagement, selectedDizi.Guid);
        }

        public void ManageDiziCondition(string guid)
        {
            Game.MessagingManager.Send(EventString.Dizi_ConditionManagement, guid);
            //var items = Game.MessagingManager.Send(EventString.Faction_ListDiziConditionItems,, Faction.Silver);
        }

        /// <summary>
        /// 弟子增加经验值
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="exp"></param>
        public void DiziExpAdd(string guid, int exp)
        {
            if (exp == 0) return;
            var dizi = Faction.GetDizi(guid);
            var max = LevelConfig.GetMaxExp(dizi.Level);
            var value = dizi.Exp.Value;
            int balance;
            var newValue = balance = value + exp;
            var level = dizi.Level;
            var maxLevel = LevelConfig.MaxLevel;
            if (max <= newValue && //到达上限
                maxLevel > level + 1) //小于弟子配置上限
            {
                level++;
                balance = newValue - max;
                //升级
                dizi.LevelSet(level);
                max = LevelConfig.GetMaxExp(level);
            }
            dizi.ExpSet(balance, max);
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
            var dizi = Faction.GetDizi(guid);
            switch (itemType)
            {
                case 0:
                {
                    var diziWeapon = dizi.Weapon;
                    var weapon = Faction.Weapons[index];
                    dizi.SetWeapon(weapon);
                    Faction.RemoveWeapon(weapon);
                    if (diziWeapon != null) Faction.AddWeapon(diziWeapon);
                    break;
                }
                case 1:
                {
                    var diziArmor = dizi.Armor;
                    var armor = Faction.Armors[index];
                    dizi.SetArmor(armor);
                    Faction.RemoveArmor(armor);
                    if (diziArmor!= null) Faction.AddArmor(diziArmor);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            Game.MessagingManager.Send(EventString.Dizi_ItemEquipped, dizi.Guid);
        }
        public void DiziUnEquipItem(string guid, int itemType)
        {
            var dizi = Faction.GetDizi(guid);
            switch (itemType)
            {
                case 0:
                {
                    var weapon = dizi.Weapon;
                    dizi.SetWeapon(null);
                    Faction.AddWeapon(weapon);
                    break;
                }
                case 1:
                {
                    var armor = dizi.Armor;
                    dizi.SetArmor(null);
                    Faction.RemoveArmor(armor);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }

            Game.MessagingManager.Send(EventString.Dizi_ItemUnEquipped, string.Empty);
        }

        public void AddDiziCon(string guid, IAdjustment.Types type, int adjValue)
        {
            var dizi = Faction.GetDizi(guid);
            switch (type)
            {
                case IAdjustment.Types.Stamina:
                {
                    var staminaController = Game.Controllers.Get<StaminaController>();
                    staminaController.ConsumeStamina(guid, adjValue, true);
                    break;
                }
                case IAdjustment.Types.Exp:
                {
                    DiziExpAdd(guid, adjValue);//弟子经验值交给经验方法添加
                    break;
                }
                default:
                    dizi.ConSet(type, adjValue);
                    break;
            }
        }

        public void UseMedicine(string guid, int index)
        {
            var (med,_) = Faction.GetAllMedicines()[index];
            Faction.RemoveMedicine(med, 1);
            var dizi = Faction.GetDizi(guid);
            foreach (var treatment in med.Treatments)
            {
                var (kind, max) = treatment.Treatment switch
                {
                    Treatments.Stamina => (IAdjustment.Types.Stamina, dizi.Stamina.Con.Max),
                    Treatments.Food => (IAdjustment.Types.Food, dizi.Food.Max),
                    Treatments.Emotion => (IAdjustment.Types.Emotion, dizi.Emotion.Max),
                    Treatments.Injury => (IAdjustment.Types.Injury, dizi.Injury.Max),
                    Treatments.Inner => (IAdjustment.Types.Inner, dizi.Inner.Max),
                    _ => throw new ArgumentOutOfRangeException()
                };
                AddDiziCon(guid, kind, treatment.GetValue(max));
            }
        }

        public void UseSilver(string guid, int amount)
        {
            if(Faction.Silver <amount) 
                XDebug.LogError($"门派银两({Faction.Silver})小于消费银两({amount})!");
            Faction.AddSilver(-amount);
            AddDiziCon(guid, IAdjustment.Types.Silver, amount);
        }
    }
}