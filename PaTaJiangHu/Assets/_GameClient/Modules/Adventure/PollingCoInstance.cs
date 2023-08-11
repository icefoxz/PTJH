using GameClient.Models;
using UnityEngine;

namespace GameClient.Modules.Adventure
{
    /// <summary>
    /// 轮询处理器, 用于处理轮询更新, 子类实现具体的轮询的处理
    /// </summary>
    public abstract class PollingInstance 
    {
        protected CoPollingInstance CoService { get; }
        protected abstract string CoName { get; }
        protected abstract string CoParentName { get; }

        protected PollingInstance(int updateSecs = 1)
        {
            CoService = new CoPollingInstance(updateSecs, onUpdate: PollingUpdate);
            CoService.StartService(CoParentName, CoName);
        }

        /// <summary>
        /// 轮询模式, 每隔{自定义秒数}更新
        /// </summary>
        protected abstract void PollingUpdate();

        /// <summary>
        /// 只会执行一次, 第二次开始无效
        /// </summary>
        protected void StopService()
        {
            CoService.StopService();
            OnServiceStop();
        }

        protected void AttachObject(GameObject obj)
        {
            CoService.AttachObject(obj);
        }

        protected virtual void OnServiceStop() { }
    }
}