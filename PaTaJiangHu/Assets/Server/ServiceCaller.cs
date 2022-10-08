using System;
using Server.Controllers.Adventures;
using Utls;

namespace Server
{
    public interface IServiceCaller : ISingletonDependency
    {
        void StartAdventure(int id, AdvUnit[] units);
        void AdventureNext(Adventure adv);
    }

    public class ServiceCaller : DependencySingleton<IServiceCaller>, IServiceCaller
    {
        private AdventureController AdvController { get; } = new AdventureController();

        public void StartAdventure(int id, AdvUnit[] units)
        {
            var ad = AdvController.InstanceAdventure(id, units);
            Game.MessagingManager.Invoke(EventString.Adventure_Start, ad.ToParam());
        }
        public void AdventureNext(Adventure adv)
        {
            AdvController.NextEvent(adv);
            Game.MessagingManager.Invoke(EventString.Adventure_Event, adv.ToParam());
        }
    }
}