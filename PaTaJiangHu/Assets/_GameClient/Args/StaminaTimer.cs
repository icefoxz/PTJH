using System;
using System.Collections;
using AOT.Utls;
using GameClient.SoScripts.Characters;
using UnityEngine;

namespace GameClient.Args
{
    public interface IStaminaTimer : ISingletonDependency
    {
        int CurrentStamina { get; }
        long LastTicks { get; }
        int BaseStamina { get; }
        bool IsInit { get; }
        TimeSpan MinPerStamina { get; }
        TimeSpan Countdown { get; }
        event Action StaminaUpdate;
        void Init(int baseStamina, long lastTick, StaminaConfigSo staminaGenerator);
        void Set(int baseStamina, long lastTick);
    }

    public class StaminaTimer : DependencySingleton<IStaminaTimer>, IStaminaTimer
    {
        private StaminaConfigSo StaminaGen { get; set; }
        public int CurrentStamina => AdditionStamina + BaseStamina;
        public int AdditionStamina => StaminaGen.GetStamina(LastTicks);
        public TimeSpan Countdown => StaminaGen.GetNextStaminaTimeInterval(LastTicks + StaminaGen.PerStaminaTicks * AdditionStamina);
        public TimeSpan MinPerStamina => StaminaGen.PerStamina;
        public long LastTicks { get; private set; }
        public int BaseStamina { get; private set; }
        public bool IsInit { get; private set; }
        public event Action StaminaUpdate;

        public void Init(int baseStamina, long lastTick, StaminaConfigSo staminaGenerator)
        {
            IsInit = true;
            StaminaGen = staminaGenerator;
            Set(baseStamina, lastTick);
        }

        public void Set(int baseStamina, long lastTick)
        {
            StopAllCoroutines();
            BaseStamina = baseStamina;
            LastTicks = lastTick;
            StartCoroutine(UpdateEverySeconds());
        }

        private IEnumerator UpdateEverySeconds()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                StaminaUpdate?.Invoke();
            }
        }
    }
}