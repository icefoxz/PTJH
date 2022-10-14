using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Views
{
    public interface IView
    {
        IReadOnlyDictionary<string, GameObject> GetMap();
        GameObject[] GetObjects();
        T GetObject<T>(string objName);
        T GetObject<T>(int index);
    }

    public class View : MonoBehaviour, IView
    {
        [SerializeField] private GameObject[] _components;
        public IReadOnlyDictionary<string, GameObject> GetMap() => _components.ToDictionary(c => c.name, c => c);
        public GameObject[] GetObjects() => _components.ToArray();

        public T GetObject<T>(string objName)
        {
            var obj = _components.FirstOrDefault(c => c.name == objName);
            if (obj == null)
                throw new NullReferenceException(objName); 
            return obj.GetComponent<T>();
        }

        public T GetObject<T>(int index) => _components[index].GetComponent<T>();
    }
}
