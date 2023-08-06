using UnityEngine;
using UnityEngine.EventSystems;

namespace AOT
{
    public class EventSystemDebug : MonoBehaviour
    {
        void Update()
        {
            var isPointerOverObj = EventSystem.current.IsPointerOverGameObject();
            Debug.Log($"{name}: EventSystemDebug Update: {isPointerOverObj}");
        }
    }
}