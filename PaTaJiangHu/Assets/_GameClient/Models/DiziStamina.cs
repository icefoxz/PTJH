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

        public void Update(long zeroTicks)
        {
            _zeroTicks = zeroTicks;
            var current = Controller.GetDiziStamina(_zeroTicks, _con.Max);
            Con.Set(current);
        }
    }
}