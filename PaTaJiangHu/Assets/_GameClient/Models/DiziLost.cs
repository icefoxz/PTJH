namespace GameClient.Models
{
    /// <summary>
    /// 弟子失踪, 用于管理弟子失踪相关信息
    /// </summary>
    public class DiziLost : DiziStateModel
    {
        protected override string LogPrefix { get; } = "失踪";
    }
}