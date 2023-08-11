using System.Collections;
using AOT.Core.Systems.Coroutines;
using GameClient.Controllers;
using GameClient.System;
using UnityEngine;

namespace GameClient.Models
{
    public class DiziStaminaManager
    {
        private Dizi _dizi;
        private DiziStamina _stamina;
        private ICoroutineInstance _staminaService;

        public DiziStaminaManager(Dizi dizi, int initialStamina)
        {
            _dizi = dizi;
            _stamina = new DiziStamina(Game.Controllers.Get<StaminaController>(), initialStamina,
                (int)Game.Config.DiziCfg.StaminaCfg.PerStamina.TotalSeconds);
            StartStaminaService();
        }

        public IDiziStamina Stamina => _stamina;

        private void StartStaminaService()
        {
            _staminaService = Game.CoService.RunCo(StaminaPolling(), _dizi.Name, "体力服务器");

            IEnumerator StaminaPolling()
            {
                while (true)
                {
                    yield return new WaitForSeconds(1);
                    StaminaUpdate(Stamina.ZeroTicks);
                }
            }
        }

        internal bool StaminaUpdate(long ticks)
        {
            var lastValue = _stamina.Con.Value;
            _stamina.Update(ticks);
            var updatedValue = _stamina.Con.Value;
            return lastValue != updatedValue;
        }

        internal void StopStaminaService()
        {
            _staminaService?.StopCo();
            _staminaService = null;
        }

        internal void Destroy() => _staminaService?.Destroy();
    }
}