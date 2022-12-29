using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utls
{
    public static class XDebug
    {
        public static void Log(string message, [CallerMemberName] string callerName = null) => Debug.Log($"{callerName}: {message}");

        public static void LogError(string message, Object obj = null, [CallerMemberName] string callerName = null) =>
            Debug.LogError($"{callerName}: {message}", obj);
    }
}