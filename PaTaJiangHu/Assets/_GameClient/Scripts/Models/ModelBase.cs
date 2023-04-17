using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// 模型基础类, 处理底层的模型逻辑
    /// </summary>
    public abstract class ModelBase
    {
        /// <summary>
        /// Log前缀
        /// </summary>
        protected abstract string LogPrefix { get; }

        protected void Log(string message) => XDebug.Log($"{LogPrefix}: {message}");
        protected void LogError(string message) => XDebug.LogError($"{LogPrefix}: {message}");
        protected void LogWarning(string message) => XDebug.LogWarning($"{LogPrefix}: {message}");
        protected void SendEvent(string eventString, params object[] args) =>
            Game.MessagingManager.SendParams(eventString, args);
    }
}