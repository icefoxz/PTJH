using System;
using DiziM;
using Server.Controllers;

namespace _GameClient.Models
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

        public int SecsPerRecover => (int)Game.Config.DiziCfg.StaminaCfg.PerStamina.TotalSeconds;

        public DiziStamina(StaminaController controller,long zeroTicks, int max):base(zeroTicks)
        {
            Controller = controller;
            _con = new ConValue(max);
        }
        public DiziStamina(StaminaController controller, int max):base(controller.GetZeroTicksFromStamina(max))
        {
            Controller = controller;
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