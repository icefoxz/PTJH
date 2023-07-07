using UnityEngine;

namespace AOT._AOT
{
    public class MonoDebug : MonoBehaviour
    {
        void OnEnable()
        {
            Debug.Log($"{name}: MonoDebug OnEnable");
        }
    }
}
