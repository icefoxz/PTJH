using System;
using System.Collections;
using Systems.Coroutines;
using UnityEngine;

namespace _GameClient.Models
{
    /// <summary>
    /// 协程更新实体, 主要利用<see cref="Game.CoService"/>来实现简单更新的处理逻辑
    /// </summary>
    public class CoPollingInstance
    {
        public bool IsRunning { get; private set; }
        private Action OnServiceEnd;
        private Action OnUpdate;
        private ICoroutineInstance CoInstance { get; set; }
        private int UpdateSecs { get; }

        public CoPollingInstance(int updateSecs,Action onUpdate,Action onServiceEnd = null)
        {
            UpdateSecs = updateSecs;
            OnUpdate = onUpdate;
            OnServiceEnd = onServiceEnd;
        }
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="parentName">GameObject父件,如果没有改父件将创建一个</param>
        public void StartService(string parentName)
        {
            IsRunning = true;
            CoInstance = Game.CoService.RunCo(CoUpdate(), OnServiceEnd, parentName);
        }
        /// <summary>
        /// 只一次触发有效.
        /// </summary>
        public void StopService()
        {
            CoInstance.StopCo();
            IsRunning = false;
        }

        public void UpdateCoName(string title, bool excludeInstanceId = false) =>
            CoInstance.name = excludeInstanceId ? title : CoInstance.GetInstanceID() + title;

        private IEnumerator CoUpdate()
        {
            while (IsRunning)
            {
                yield return new WaitForSeconds(UpdateSecs);
                OnUpdate?.Invoke();
            }
        }
    }
}