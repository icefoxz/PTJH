using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Views
{
    public interface IView
    {
        IReadOnlyDictionary<string, GameObject> GetMap();
        GameObject GameObject { get; }
        GameObject[] GetObjects();
        GameObject GetObject(string objName);
        T GetObject<T>(string objName);
        T GetObject<T>(int index);
    }

    public class View : MonoBehaviour, IView
    {
        [SerializeField] private GameObject[] _components;
        public IReadOnlyDictionary<string, GameObject> GetMap() => _components.ToDictionary(c => c.name, c => c);
        public GameObject GameObject => gameObject;
        public GameObject[] GetObjects() => _components.ToArray();
        public GameObject GetObject(string objName)
        {
            var obj = _components.FirstOrDefault(c => c.name == objName);
            if (!obj) throw new NullReferenceException($"View.{name} 找不到物件名：{objName}");
            return obj;
        }
        public T GetObject<T>(string objName) => GetObject(objName).GetComponent<T>();
        public T GetObject<T>(int index) => _components[index].GetComponent<T>();
    }
}
