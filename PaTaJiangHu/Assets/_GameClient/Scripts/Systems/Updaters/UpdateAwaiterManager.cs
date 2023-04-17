using System;
using System.Collections.Generic;

namespace Systems.Updaters
{
    /// <summary>
    /// 帧回调器，类似协程等待，但执行比协程快
    /// </summary>
    public interface IGameUpdateAwaiter
    {
        bool IsDone { get; }
        bool IsCancel { get; }
        Action CallBackAction { get; }
    }

    /// <summary>
    /// 帧等待控制器
    /// </summary>
    public class UpdateAwaiterManager
    {
        /// <summary>
        /// 根据每帧等待并回调的列表
        /// </summary>
        private List<IGameUpdateAwaiter> AwaiterList { get; } = new List<IGameUpdateAwaiter>();

        /// <summary>
        /// 注册帧回调器
        /// </summary>
        /// <param name="awaiter"></param>
        public void RegUpdateAwaiter(IGameUpdateAwaiter awaiter) => AwaiterList.Add(awaiter);

        public void GameAwaitersUpdate()
        {
            foreach (var awaiter in AwaiterList.ToArray())
            {
                if (awaiter.IsCancel)
                {
                    AwaiterList.Remove(awaiter);
                    continue;
                }

                if (!awaiter.IsDone) continue;
                AwaiterList.Remove(awaiter);
                awaiter.CallBackAction?.Invoke();
            }
        }
    }
}