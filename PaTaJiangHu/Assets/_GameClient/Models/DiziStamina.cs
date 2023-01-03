using System;
using BattleM;
using Server.Configs._script.Factions;

namespace _GameClient.Models
{
    public interface IDiziStamina
    {
        IGameCondition Con { get; }
        long ZeroTicks { get; }
        TimeSpan GetCountdown();
        (int stamina, int max, int min, int sec) GetStaminaValue();
    }

    public class DiziStamina : IDiziStamina
    {
        private ConValue _con;
        private long _zeroTicks;

        private StaminaController Controller { get; }
        public IGameCondition Con => _con;

        public long ZeroTicks => _zeroTicks;
        public TimeSpan GetCountdown() => Controller.GetCountdown(ZeroTicks);
        public DiziStamina(StaminaController controller,long zeroTicks, int max)
        {
            Controller = controller;
            _con = new ConValue(max);
            Update(zeroTicks);
        }
        public DiziStamina(StaminaController controller, int max)
        {
            Controller = controller;
            _con = new ConValue(max);
            Update(controller.GetZeroTicksFromStamina(max));
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
        public void Update(long zeroTicks)
        {
            _zeroTicks = zeroTicks;
            var current = Controller.GetDiziStamina(_zeroTicks, _con.Max);
            Con.Set(current);
        }
    }
}