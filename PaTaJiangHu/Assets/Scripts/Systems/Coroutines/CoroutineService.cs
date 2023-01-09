using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using Utls;

namespace Systems.Coroutines
{
    public interface ICoroutineService : ISingletonDependency
    {
        int RunCo(IEnumerator enumerator, Action onFinishCallback = null, [CallerMemberName] string method = null);
        void StopCo(int index);
    }

    public class CoroutineService : DependencySingleton<ICoroutineService>, ICoroutineService
    {
        private ObjectPool<CoroutineInstance> _pool;
        private readonly Dictionary<int, CoroutineInstance> _map = new Dictionary<int, CoroutineInstance>();
        [SerializeField] private CoroutineInstance _prefab;
        private const string Co = ".Co";
        private const char Dot = '.';

        protected override void OnAwake()
        {
            base.OnAwake();
            _pool = new ObjectPool<CoroutineInstance>(createFunc: () =>
                {
                    var co = Instantiate(_prefab, transform);
                    _map.Add(co.GetInstanceID(), co);
                    return co;

                }, actionOnGet: co =>
                {
                    //RunCo 实现
                }, actionOnRelease: co =>
                {
                    var instanceId = co.GetInstanceID();
                    co.name = instanceId + Co;
                },
                actionOnDestroy: co => _map.Remove(co.GetInstanceID()));
        }

        public int RunCo(IEnumerator enumerator, Action onFinishCallback, [CallerMemberName] string method = null)
        {
            var co = _pool.Get();
            var instanceId = co.GetInstanceID();
            co.name = instanceId + Dot + method;
            co.StartCo(enumerator, () =>
            {
                StopCo(co.GetInstanceID());
                onFinishCallback?.Invoke();
            });
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