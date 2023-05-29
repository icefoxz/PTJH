using System;
using _GameClient.Models;
using Server.Configs.Characters;
using Utls;

namespace Server.Controllers
{
    public class StaminaController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private StaminaConfigSo StaminaCfg => Game.Config.DiziCfg.StaminaCfg;
        private Config.DiziConfig DiziCfg => Game.Config.DiziCfg;
        public int MaxStamina = 100;

        public bool TryConsumeForBattle(string diziGuid)
        {
            var dizi = Faction.GetDizi(diziGuid);
            return ConsumeStamina(dizi.Guid, DiziCfg.BattleCfg.Before.StaminaCost);
        }

        /// <summary>
        /// 消费体力, 不需要特别为stamina值设成负值. 因为消费本身就是扣除值.
        /// 如果扣除失败则不执行.
        /// </summary>
        /// <param name="diziGuid"></param>
        /// <param name="stamina"></param>
        public bool ConsumeStamina(string diziGuid, int stamina)
        {
            if (DiziCfg.BattleCfg.Before.StaminaCost > stamina) return false; 
            var dizi = Faction.GetDizi(diziGuid);
            var con = dizi.Stamina.Con;
            if (con.Value < stamina) XDebug.LogWarning($"体力 = {con} 不够消费 {stamina}!");
            AddStamina(diziGuid, -stamina, true);
            return true;
        }

        public void AddStamina(string diziGuid,int stamina,bool autoAlign = false)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var con = dizi.Stamina.Con;
            if (autoAlign) stamina = Math.Min(con.Value, stamina);//大于原有体力, 体力归零
            var newStamina = con.Value - stamina;
            var newZeroTicks = GetZeroTicksFromStamina(newStamina);
            dizi.StaminaUpdate(newZeroTicks);
        }

        public void SetStaminaZero(string diziGuid,bool autoAlign = false)
        {
            var dizi = Faction.GetDizi(diziGuid);
            AddStamina(diziGuid, dizi.Stamina.Con.Value, autoAlign);
        }

        public int GetDiziStamina(long zeroTicks, int max)
        {
            var stamina = StaminaCfg.CountStamina(zeroTicks, max);
            return stamina;
        }

        public TimeSpan GetCountdown(long zeroTicks) => StaminaCfg.CountdownTimeSpan(zeroTicks);

        public long GetZeroTicksFromStamina(int stamina) => StaminaCfg.GetZeroTicksFromStamina(stamina);
    }
}