using System;
using GameClient.Controllers;
using GameClient.Modules.DiziM;

namespace GameClient.Models
{
    public interface IDiziStamina
    {
        IGameCondition Con { get; }
        long ZeroTicks { get; }
        TimeSpan GetCountdown();
        int SecsPerRecover { get; }
        (int stamina, int max, int min, int sec) GetStaminaValue();
    }

    /// <summary>
    /// 弟子体力信息
    /// </summary>
    public class DiziStamina : TimeValueTicker, IDiziStamina
    {
        private ConValue _con;

        private StaminaController Controller { get; }
        public IGameCondition Con => _con;

        public TimeSpan GetCountdown() => Controller.GetCountdown(ZeroTicks);// 5 -> 6 : 

        public int SecsPerRecover { get; }

        public DiziStamina(StaminaController controller,long zeroTicks, int max, int secsPerRecover):base(zeroTicks)
        {
            Controller = controller;
            SecsPerRecover = secsPerRecover;
            _con = new ConValue(max);
        }
        public DiziStamina(StaminaController controller, int max, int secsPerRecover):base(controller.GetZeroTicksFromStamina(max))
        {
            Controller = controller;
            SecsPerRecover = secsPerRecover;
            _con = new ConValue(max);
        }

        public (int stamina, int max, int min, int sec) GetStaminaValue()
        {
            var isFull = Con.Max == Con.Value;
            var min = -1;
            var sec = -1;
            if (!isFull)
            {
                var cd = GetCountdown();
                min = (int)cd.TotalMinutes;
                sec = cd.Seconds;
            }
            return (Con.Value, Con.Max, min, sec);
        }

        protected override void OnUpdate()
        {
            var current = Controller.GetDiziStamina(ZeroTicks, _con.Max);
            Con.Set(current);
        }
    }
}