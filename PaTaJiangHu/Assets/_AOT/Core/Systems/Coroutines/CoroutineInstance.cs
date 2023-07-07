using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AOT._AOT.Core.Systems.Coroutines
{
    public interface ICoroutineInstance
    {
        void StopCo();
        int GetInstanceID();
        Coroutine Instance { get; }
        string name { get; set; }
    }

    public class CoroutineInstance : MonoBehaviour, ICoroutineInstance
    {
        private Coroutine Coroutine { get; set; }
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

        public Coroutine Instance => Coroutine;
    }
}