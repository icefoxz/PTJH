using System;
using _GameClient.Models;
using Server.Configs.Characters;
using Utls;

namespace Server.Controllers
{
    public class StaminaController : IGameController
    {
        private Faction Faction => Game.World.Faction;
        private StaminaConfigSo StaminaGenerator => Game.Config.DiziCfg.StaminaCfg;
        public int MaxStamina = 100;

        public void ConsumeStamina(string diziGuid,int stamina,bool autoAlign = false)
        {
            var dizi = Faction.GetDizi(diziGuid);
            var con = dizi.Stamina.Con;
            if (autoAlign) stamina = Math.Min(con.Value, stamina);//大于原有体力, 体力归零
            if (con.Value < stamina)
                XDebug.LogWarning($"体力 = {con} 不够消费 {stamina}!");
            var newStamina = con.Value - stamina;
            var newZeroTicks = GetZeroTicksFromStamina(newStamina);
            dizi.UpdateStamina(newZeroTicks);
        }

        public int GetDiziStamina(long zeroTicks, int max)
        {
            var stamina = StaminaGenerator.CountStamina(zeroTicks, max);
            return stamina;
        }

        public TimeSpan GetCountdown(long zeroTicks) => StaminaGenerator.CountdownTimeSpan(zeroTicks);

        public long GetZeroTicksFromStamina(int stamina) => StaminaGenerator.GetZeroTicksFromStamina(stamina);
    }
}