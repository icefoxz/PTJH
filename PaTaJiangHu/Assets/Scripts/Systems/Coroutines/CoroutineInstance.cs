using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Systems.Coroutines
{
    internal class CoroutineInstance : MonoBehaviour
    {
        public Coroutine Coroutine { get; set; }

        public void StartCo(IEnumerator enumerator, UnityAction callBackAction)
        {
            Coroutine = StartCoroutine(CoroutineMethod(enumerator, callBackAction));
        }

        private IEnumerator CoroutineMethod(IEnumerator co, UnityAction callBackAction)
        {
            yield return co;
            callBackAction?.Invoke();
        }
        public void StopCo() => StopCoroutine(Coroutine);
    }
}