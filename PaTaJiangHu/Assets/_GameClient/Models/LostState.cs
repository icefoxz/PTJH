using Server.Controllers;

namespace _GameClient.Models
{
    /// <summary>
    /// 失踪状态
    /// </summary>
    public class LostState
    {
        public long StartTime { get; }
        public DiziDynamicProps Props { get; }
        public DiziActivityLog LastActivityLog { get; }

        public LostState(Dizi dizi, long startTime, DiziActivityLog lastActivityLog)
        {
            StartTime = startTime;
            Props = new DiziDynamicProps(dizi);
            LastActivityLog = lastActivityLog;
        }
    }
}