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
        private readonly List<CoroutineInstance> _list = new List<CoroutineInstance>();
        [SerializeField] private CoroutineInstance _prefab;

        protected override void OnAwake()
        {
            base.OnAwake();
            _pool = new ObjectPool<CoroutineInstance>(InstanceCo);
        }

        public int RunCo(IEnumerator enumerator)
        {
            var co = InstanceCo();
            co.StartCo(enumerator, () => Remove(co));
            return _list.IndexOf(co);
        }

        public void StopCo(int index)
        {
            var co = _list[index];
            co.StopCo();
            Remove(co);
        }

        private void Remove(CoroutineInstance co)
        {
            _list.Remove(co);
            _pool.Release(co);
            Destroy(co.gameObject);
        }

        private CoroutineInstance InstanceCo()
        {
            var co = Instantiate(_prefab, transform);
            _list.Add(co);
            return co;
        }
    }

}