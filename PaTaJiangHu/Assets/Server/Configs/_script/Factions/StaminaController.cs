using System;
using _GameClient.Models;
using Server.Configs._script.Characters;
using Utls;

namespace Server.Configs._script.Factions
{
    public class StaminaController : IGameController
    {
        private StaminaGenerateSo StaminaGenerator { get; }
        public int MaxStamina = 100;

        internal StaminaController(Configure.DiziConfigure diziCfg)
        {
            StaminaGenerator = diziCfg.StaminaGenerator;
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