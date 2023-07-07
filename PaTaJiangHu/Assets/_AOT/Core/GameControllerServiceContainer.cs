using System;
using System.Collections.Generic;

namespace AOT._AOT.Core
{
    /// <summary>
    /// 游戏逻辑控制器, 主要控制与模型和数据的交互
    /// </summary>
    public interface IGameController
    {
    }
    /// <summary>
    /// 基于<see cref="IGameController"/>的DI容器
    /// </summary>
    public class GameControllerServiceContainer
    {
        private Dictionary<Type,IGameController> Container { get; set; } = new Dictionary<Type,IGameController>();

        public T Get<T>() where T : class, IGameController
        {
            var type = typeof(T);
            if (!Container.TryGetValue(type, out var obj))
                throw new NotImplementedException($"{type.Name} hasn't register!");
            return obj as T;
        }

        public void Reg<T>(T controller) where T : class, IGameController => Container.Add(typeof(T), controller);
    }
}