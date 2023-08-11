using AOT.Core;
using GameClient.Modules.Adventure;

namespace GameClient.Models
{
    /// <summary>
    /// 弟子失踪, 用于管理弟子失踪相关信息
    /// </summary>
    public class Lost_ActivityManager : DiziActivityModel<LostDelayedPoller>
    {
        protected override string LogPrefix { get; } = "失踪";

        public void ActivityStart(Dizi dizi, DiziActivityLog log)
        {
            Add(dizi.Guid, new LostDelayedPoller(dizi, log));
            SendEvent(EventString.Dizi_Lost_Start, dizi.Guid);
        }

        public Dizi GetDizi(string guid) => GetPoller(guid).Dizi;

        public void RestoreDizi(string guid)
        {
            Remove(guid);
            SendEvent(EventString.Dizi_Lost_End, guid);
        }
    }

    public class LostDelayedPoller : DelayedPoller
    {
        public Dizi Dizi { get; }
        public DiziActivityLog Log { get; }

        public LostDelayedPoller(Dizi dizi, DiziActivityLog log) : base(dizi)
        {
            Dizi = dizi;
            Log = log;
        }
    }
}