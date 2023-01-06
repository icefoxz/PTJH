using System;
using _GameClient.Models;
using Server.Configs.Characters;
using Utls;

namespace Server.Controllers
{
    public class StaminaController : IGameController
    {
        private Stamina StaminaGenerator { get; }
        public int MaxStamina = 100;

        internal StaminaController(Config.Dizi diziCfg)
        {
            StaminaGenerator = diziCfg.StaminaCfg;
        }

        public void ConsumeStamina(Dizi dizi,int stamina)
        {
            var con = dizi.Stamina.Con;
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