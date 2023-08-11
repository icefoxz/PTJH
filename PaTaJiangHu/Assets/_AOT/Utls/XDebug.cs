using System.Runtime.CompilerServices;
using UnityEngine;

namespace AOT.Utls
{
    public static class XDebug
    {
        public static void Event([CallerMemberName] string callerName = null) => Debug.Log($"{callerName}.Invoke()!");
        public static void Log(string message, [CallerMemberName] string callerName = null)
        {
#if UNITY_EDITOR
            Debug.Log($"{callerName}: {message}");
#endif
        }

        public static void LogError(string message, Object obj = null, [CallerMemberName] string callerName = null) => Debug.LogError($"{callerName}: {message}", obj);
        public static void LogWarning(string message, Object obj = null, [CallerMemberName] string callerName = null) => Debug.LogWarning($"{callerName}: {message}", obj);
    }
}