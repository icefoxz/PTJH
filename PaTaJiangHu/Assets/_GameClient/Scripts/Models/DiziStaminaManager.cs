using System.Collections;
using _GameClient.Models;
using Server.Controllers;
using UnityEngine;

namespace Models
{
    public class DiziStaminaManager
    {
        private Dizi _dizi;
        private DiziStamina _stamina;

        public DiziStaminaManager(Dizi dizi, int initialStamina)
        {
            _dizi = dizi;
            _stamina = new DiziStamina(Game.Controllers.Get<StaminaController>(), initialStamina);
            StartStaminaService();
        }

        public IDiziStamina Stamina => _stamina;

        private void StartStaminaService()
        {
            Game.CoService.RunCo(StaminaPolling(), null, _dizi.Name);

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
    }
}