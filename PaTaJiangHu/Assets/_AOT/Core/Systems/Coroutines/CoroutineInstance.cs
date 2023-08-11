using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace AOT.Core.Systems.Coroutines
{
    public interface ICoroutineInstance
    {
        void StopCo();
        int GetInstanceID();
        Coroutine Instance { get; }
        string name { get; set; }
        GameObject GameObject { get; }
        void Destroy();
    }

    public class CoroutineInstance : MonoBehaviour, ICoroutineInstance
    {
        private enum CoStates
        {
            None,
            Start,
            Stop
        }
        private Coroutine Coroutine { get; set; }
        private CoStates State { get; set; }
        private event UnityAction OnStopAction;

        public void StartCo(IEnumerator enumerator, UnityAction callBackAction, UnityAction onStopAction)
        {
            if(State == CoStates.Start) return;
            State = CoStates.Start;
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
            if(Coroutine == null || State is CoStates.Stop) return;
            State = CoStates.Stop;
            StopAllCoroutines();
            OnStopAction?.Invoke();
        }

        public Coroutine Instance => Coroutine;
        public GameObject GameObject => gameObject;
        public void Destroy() => Destroy(gameObject);
    }
}