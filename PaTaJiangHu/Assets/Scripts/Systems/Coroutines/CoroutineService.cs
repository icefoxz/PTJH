using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utls;

namespace Systems.Coroutines
{
    public interface ICoroutineService : ISingletonDependency
    {
        int RunCo(IEnumerator enumerator);
        void StopCo(int index);
    }

    public class CoroutineService : DependencySingleton<ICoroutineService>, ICoroutineService
    {
        private ObjectPool<CoroutineInstance> _pool;
        private readonly Dictionary<int,CoroutineInstance> _map = new Dictionary<int,CoroutineInstance>();
        [SerializeField] private CoroutineInstance _prefab;

        protected override void OnAwake()
        {
            base.OnAwake();
            _pool = new ObjectPool<CoroutineInstance>(() =>
                {
                    var co = Instantiate(_prefab, transform);
                    _map.Add(co.GetInstanceID(), co);
                    return co;

                }, null, null,
                co => _map.Remove(co.GetInstanceID()));
        }

        public int RunCo(IEnumerator enumerator)
        {
            var co = _pool.Get();
            var instanceId = co.GetInstanceID();
            co.StartCo(enumerator, () => StopCo(instanceId));
            return instanceId;
        }

        public void StopCo(int index)
        {
            var co = _map[index];
            co.StopCo();
            _pool.Release(co);
        }
    }

}