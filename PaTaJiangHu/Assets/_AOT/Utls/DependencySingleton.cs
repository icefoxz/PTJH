using UnityEngine;

namespace AOT._AOT.Utls
{
    public interface ISingletonDependency
    {

    }

    public class DependencySingleton<T> : MonoBehaviour, ISingletonDependency where T : ISingletonDependency
    {
        private static T _instance;
        public static T Instance => _instance;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (T)(this as ISingletonDependency);
            }
            else
            {
                Destroy(gameObject);
            }

            OnAwake();
        }

        protected virtual void OnAwake()
        {
        }
    }
}