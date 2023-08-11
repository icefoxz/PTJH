﻿using System;
using System.Collections.Generic;
using AOT.Utls;
using GameClient.Models;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.Adventures;
using GameClient.SoScripts.BattleSimulation;
using GameClient.System;

namespace GameClient.Controllers
{
    /// <summary>
    /// 事件参数
    /// </summary>
    internal class AdvArg : IAdvArg
    {
        public string DiziName => Dizi.Name;
        private Dizi Dizi { get; }
        public ITerm Term { get; }

        /// <summary>
        /// 事件交互结果/玩家选择
        /// </summary>
        public int InteractionResult { get; private set; }

        public ISimulationOutcome SimOutcome { get; private set; }
        public IAdjustment Adjustment => this;
        public IRewardHandler RewardHandler { get; }
        public IList<string> ExtraMessages { get; private set; } = new List<string>();

        public AdvArg(Dizi dizi, IRewardHandler handler)
        {
            Term = Dizi = dizi;
            RewardHandler = handler;
        }

        public void SetSimulationOutcome(ISimulationOutcome simulationOutcome)
            => SimOutcome = simulationOutcome;

        public void SetInteractionResult(int result) => InteractionResult = result;
        string IAdjustment.Set(IAdjustment.Types type, int value, bool percentage)
        {
            var controller = Game.Controllers.Get<DiziController>();
            var adjValue = value;
            if (percentage)
            {
                var conMax = type switch
                {
                    IAdjustment.Types.Stamina => Dizi.Stamina.Con.Max,
                    IAdjustment.Types.Silver => Dizi.Silver.Max,
                    IAdjustment.Types.Food => Dizi.Food.Max,
                    IAdjustment.Types.Emotion => Dizi.Emotion.Max,
                    IAdjustment.Types.Injury => Dizi.Injury.Max,
                    IAdjustment.Types.Inner => Dizi.Inner.Max,
                    IAdjustment.Types.Exp => Dizi.Exp.Max,
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
                adjValue = value.PercentInt(conMax);
            }

            controller.AddDiziCon(Dizi.Guid, type, adjValue);
            if (adjValue == 0) return string.Empty;
            var plusSign = adjValue > 0 ? "+" : string.Empty;
            return Dizi.Name + type.GetText() + plusSign + $"({adjValue})";
        }
    }
}