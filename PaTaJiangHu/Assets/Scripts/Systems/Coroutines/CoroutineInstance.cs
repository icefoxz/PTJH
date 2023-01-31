using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.Coroutines
{
    public interface ICoroutineInstance
    {
        void StopCo();
        int GetInstanceID();
        string name { get; set; }
    }

    public class CoroutineInstance : MonoBehaviour, ICoroutineInstance
    {
        internal Coroutine Coroutine { get; set; }
        private event UnityAction OnStopAction;

        public void StartCo(IEnumerator enumerator, UnityAction callBackAction, UnityAction onStopAction)
        {
            OnStopAction = onStopAction;
            Coroutine = StartCoroutine(CoroutineMethod(enumerator, callBackAction));
        }

        private IEnumerator CoroutineMethod(IEnumerator co, UnityAction callBackAction)
        {
            yield return co;
            callBackAction?.Invoke();
            OnStopAction?.Invoke();
        }
        public void StopCo()
        {
            StopCoroutine(Coroutine);
            OnStopAction?.Invoke();
        }
    }
}