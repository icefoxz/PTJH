using System;
using System.Collections.Generic;
using System.Linq;

namespace AOT._AOT.Core.Systems.Updaters
{
    /// <summary>
    /// 以<see cref="object"/>为key的更新器，每次更新会执行所有列表里的<see cref="Action"/>方法。
    /// </summary>
    public class ObjectUpdater 
    {
        private List<(object obj, Action update)> Updators { get; } = new List<(object obj, Action update)>();

        public void RegMonoUpdate(object key, Action update)
        {
            if (Updators.Any(m => m.obj == key))
                throw new InvalidOperationException($"{key} duplicated!");
            Updators.Add((key, update));
        }

        public void RemoveMonoUpdate(object key)
        {
            var mono = Updators.FirstOrDefault(m => m.obj == key);
            if(mono == default)
                throw new InvalidOperationException($"{key} not found!");
            Updators.Remove(mono);
        }

        public void Update()
        {
            for (var i = 0; i < Updators.Count; i++)
            {
                var (_, updateMethod) = Updators[i];
                updateMethod();
            }
        }
    }
}