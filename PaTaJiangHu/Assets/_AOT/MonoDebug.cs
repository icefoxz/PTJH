using UnityEngine;

namespace AOT
{
    public class MonoDebug : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.Log($"{name}: MonoDebug OnEnable");
        }
    }
}
