using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AOT.Utls;
using UnityEngine;

namespace AOT.Core.Systems.Coroutines
{
    public interface ICoroutineService : ISingletonDependency
    {
        /// <summary>
        /// 开启协程服务
        /// </summary>
        /// <param name="enumerator"></param>
        /// <param name="onFinishCallback"></param>
        /// <param name="parentName">GameObject父件,如果没有改父件将创建一个</param>
        /// <param name="method"></param>
        /// <returns></returns>
        ICoroutineInstance RunCo(IEnumerator enumerator, Action onFinishCallback = null, string parentName = null, [CallerMemberName] string method = null);
        ICoroutineInstance RunCo(IEnumerator enumerator, string parentName, [CallerMemberName] string method = null);
        void RemoveCoParent(string name);
    }

    public class CoroutineService : DependencySingleton<ICoroutineService>, ICoroutineService
    {
        //private ObjectPool<CoroutineInstance> _pool;
        private readonly Dictionary<int, CoroutineInstance> _map = new Dictionary<int, CoroutineInstance>();
        private readonly Dictionary<string,GameObject> _parent = new Dictionary<string, GameObject>();
        [SerializeField] private CoroutineInstance _prefab;
        private const string Co = ".Co";
        private const char Dot = '.';

        //protected override void OnAwake()
        //{
        //    base.OnAwake();
        //    _pool = new ObjectPool<CoroutineInstance>(createFunc: () =>
        //        {
        //            var co = Instantiate(_prefab, transform);
        //            _map.Add(co.GetInstanceID(), co);
        //            return co;
        //
        //        }, actionOnGet: co =>
        //        {
        //            //RunCo 实现
        //        }, actionOnRelease: co =>
        //        {
        //            var instanceId = co.GetInstanceID();
        //            co.name = instanceId + Co;
        //        },
        //        actionOnDestroy: co => _map.Remove(co.GetInstanceID()));
        //}

        public ICoroutineInstance RunCo(IEnumerator enumerator, Action onFinishCallback, string parentName = null,
            [CallerMemberName] string method = null)
        {
            var co = InstanceCo(parentName);
            var instanceId = co.GetInstanceID();
            _map.Add(instanceId, co);
            co.name = instanceId + Dot + method;
            co.StartCo(enumerator, () => onFinishCallback?.Invoke(), () => StopCo(co));
            return co;
        }

        public ICoroutineInstance RunCo(IEnumerator enumerator, string parentName, string method = null) =>
            RunCo(enumerator, null, parentName, method);

        public void RemoveCoParent(string name)
        {
            var go = _parent.FirstOrDefault(p => p.Key == name).Value;
            if (go == null) return;
            _parent.Remove(name);
            Destroy(go);
        }

        private CoroutineInstance InstanceCo(string parentName)
        {
            var parent = transform;
            if (!string.IsNullOrWhiteSpace(parentName))
            {
                if (!_parent.ContainsKey(parentName))
                {
                    var go = new GameObject(parentName);
                    go.transform.SetParent(transform);
                    _parent.Add(parentName,go);
                }

                parent = _parent[parentName].transform;
            }
            var co = Instantiate(_prefab, parent); //_pool.Get();
            return co;
        }

        private void StopCo(CoroutineInstance co)
        {
            _map.Remove(co.GetInstanceID());
            Destroy(co.GameObject);
            //_pool.Release(co);
        }
    }
}