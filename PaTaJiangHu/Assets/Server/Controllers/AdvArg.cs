using System;
using _GameClient.Models;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;
using Utls;

namespace Server.Controllers
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
        public IRewardHandler Handler { get; }

        public AdvArg(Dizi dizi, IRewardHandler handler)
        {
            Term = Dizi = dizi;
            Handler = handler;
        }

        public void SetSimulationOutcome(ISimulationOutcome simulationOutcome)
            => SimOutcome = simulationOutcome;

        public void SetInteractionResult(int result) => InteractionResult = result;

        void IAdjustment.Set(IAdjustment.Types type, int value, bool percentage)
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
        }
    }
}