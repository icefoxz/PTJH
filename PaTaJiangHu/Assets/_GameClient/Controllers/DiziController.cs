﻿using System;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Models;
using GameClient.SoScripts;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.Characters;
using GameClient.SoScripts.Items;
using GameClient.System;
using UnityEngine;

namespace GameClient.Controllers
{
    public class DiziController : IGameController
    {
        private Config.DiziConfig DiziConfig => Game.Config.DiziCfg; 
        private PropStateConfigSo PropStateCfg => Game.Config.DiziCfg.PropState;
        private Faction Faction => Game.World.Faction;
        public Color BuffColor(bool isDebuff = false) => PropStateCfg.GetBuffColor(isDebuff);
        public (string title, Color color) GetSilverCfg(double ratio) => PropStateCfg.GetSilverCfg(ratio);
        public (string title, Color color) GetFoodCfg(double ratio) => PropStateCfg.GetFoodCfg(ratio);
        public (string title, Color color) GetEmotionCfg(double ratio) => PropStateCfg.GetEmotionCfg(ratio);
        public (string title, Color color) GetInjuryCfg(double ratio) => PropStateCfg.GetInjuryCfg(ratio);
        public (string title, Color color) GetInnerCfg(double ratio) => PropStateCfg.GetInnerCfg(ratio);

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
            var levelConfig = DiziConfig.LevelConfigSo;
            var dizi = Faction.GetDizi(guid);
            var max = levelConfig.GetMaxExp(dizi.Level);
            var value = dizi.Exp.Value;
            int balance;
            var newValue = balance = value + exp;
            var level = dizi.Level;
            var maxLevel = levelConfig.MaxLevel;
            if (max <= newValue && //到达上限
                maxLevel > level + 1) //小于弟子配置上限
            {
                level++;
                balance = newValue - max;
                //升级
                dizi.LevelSet(level);
                max = levelConfig.GetMaxExp(level);
            }
            dizi.ExpSet(balance, max);
        }

        /// <summary>
        /// 弟子装备物件, itemType { 0 : weapon, 1 : armor, 2 : shoes, 3 : decoration}
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
                    var diziWeapon = dizi._equipment.Weapon;
                    var weapon = Faction.Weapons[index];
                    dizi.SetWeapon(weapon);
                    Faction.RemoveWeapon(weapon);
                    if (diziWeapon != null) Faction.AddWeapon(diziWeapon);
                    break;
                }
                case 1:
                {
                    var diziArmor = dizi._equipment.Armor;
                    var armor = Faction.Armors[index];
                    dizi.SetArmor(armor);
                    Faction.RemoveArmor(armor);
                    if (diziArmor!= null) Faction.AddArmor(diziArmor);
                    break;
                }
                case 2:
                {
                    var diziShoes = dizi._equipment.Shoes;
                    var shoes = Faction.Shoes[index];
                    dizi.SetShoes(shoes);
                    Faction.RemoveShoes(shoes);
                    if (diziShoes != null) Faction.AddShoes(diziShoes);
                    break;
                }
                case 3:
                {
                    var diziDecoration = dizi._equipment.Decoration;
                    var decoration = Faction.Decorations[index];
                    dizi.SetDecoration(decoration);
                    Faction.RemoveDecoration(decoration);
                    if (diziDecoration != null) Faction.AddDecoration(diziDecoration);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }
        }
        public void DiziUnEquipItem(string guid, int itemType)
        {
            var dizi = Faction.GetDizi(guid);
            switch (itemType)
            {
                case 0:
                {
                    var weapon = dizi._equipment.Weapon;
                    dizi.SetWeapon(null);
                    Faction.AddWeapon(weapon);
                    break;
                }
                case 1:
                {
                    var armor = dizi._equipment.Armor;
                    dizi.SetArmor(null);
                    Faction.AddArmor(armor);
                    break;
                }
                case 2:
                {
                    var shoes = dizi._equipment.Shoes;
                    dizi.SetShoes(null);
                    Faction.AddShoes(shoes);
                    break;
                }
                case 3:
                {
                    var decoration = dizi._equipment.Decoration;
                    dizi.SetDecoration(null);
                    Faction.AddDecoration(decoration);
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(itemType));
            }
        }

        public void AddDiziCon(string guid, IAdjustment.Types type, int adjValue)
        {
            if (adjValue == 0) return;
            var dizi = Faction.GetDizi(guid);
            switch (type)
            {
                case IAdjustment.Types.Stamina:
                {
                    var staminaController = Game.Controllers.Get<StaminaController>();
                    staminaController.AddStamina(guid, adjValue);
                    break;
                }
                case IAdjustment.Types.Exp:
                {
                    DiziExpAdd(guid, adjValue);//弟子经验值交给经验方法添加
                    break;
                }
                default:
                    dizi.ConAdd(type, adjValue);
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

        /// <summary>
        /// 把失踪的弟子召唤回来
        /// </summary>
        /// <param name="guid"></param>
        public void RecallDiziFromLost(string guid)
        {
            var dizi = Faction.GetDizi(guid);
            if (dizi.State.Current != DiziStateHandler.States.Lost)
                XDebug.LogError($"{dizi}状态不符! {dizi.State}");
            dizi.RestoreFromLost();
        }
    }
}