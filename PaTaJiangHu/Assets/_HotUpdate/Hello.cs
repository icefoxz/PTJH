using AOT._AOT;
using UnityEngine;

namespace HotUpdate._HotUpdate
{
    public class Hello
    {
        public static void Run()
        {
            var msg = "Hello, HybridCLR 2";
            Debug.Log(msg);
            Test_LoadDll.SetMessage(msg);
        }
    }
}