using System;
using System.Collections;
using System.Net.Http;
using HotFix_Project.Managers;
using HotFix_Project.Managers.Demo_v1;
using Systems;
using Systems.Coroutines;
using UnityEngine;
using UnityEngine.UI;

namespace HotFix_Project
{
    /// <summary>
    /// 游戏App程序
    /// 不可以继承Monobehaviour
    /// </summary>
    internal class App 
    {
        private static MainUiAgent MainUiAgent { get; set; }
        public void Init()
        {
            Debug.Log($"{nameof(App)}.{nameof(Init)}!");
            //初始化热更新工程框架模块：UI管理，事件订阅...
            InitUiManager();
            Game.Run();//游戏启动
        }

        private void InitUiManager()
        {
            MainUiAgent = AppLunch.UiAgent switch
            {
                "Demo_v1" => new Demo_v1Agent(Game.MainUi),
                _ => throw new NotImplementedException($"找不到指定的MainUi代理= {AppLunch.UiAgent}!")
            };
        }

        public void InitTest()
        {
            //TestAsync();
            //TestCoroutine();
            //TestViewBag();
            //TestButtonEvent();
            Debug.Log($"{nameof(App)}.{nameof(InitTest)}!");
        }

        private static void TestCoroutine()
        {
            var waitTill = false;
            var fiveSec = CoroutineService.Instance.RunCo(CoWaitForSeconds(5));
            CoroutineService.Instance.RunCo(CoWaitForSeconds(3));
            CoroutineService.Instance.RunCo(CoWaitUntil());

            IEnumerator CoWaitForSeconds(int secs)
            {
                Debug.Log($"StartWaitForSec....{secs}");
                yield return new WaitForSeconds(secs);
                Debug.Log($"StopWaitForSec....{secs}");
                waitTill = !waitTill;
            }

            IEnumerator CoWaitUntil()
            {
                Debug.Log("StartWaitUntil");
                yield return new WaitUntil(() => waitTill);
                fiveSec.StopCo();
                Debug.Log("StopWaitUntil");
            }
        }
        private static void TestAsync()
        {
            var client = new HttpClient();
            HotFixHelper.RunTask(() => client.GetAsync("https://www.google.com"), RespondCallback);
            TestDelayWithReturn(1, 3);
            TestDelayWithReturn(2, 2);
            TestDelayWithReturn(3, 1);
            TestDelayWithReturn(4, 3);

            void RespondCallback(HttpResponseMessage res)
            {
                Debug.Log($"Response = {res.StatusCode}");
            }

            void TestDelayWithReturn(int i, int sec)
            {
                Debug.Log($"Test DelayWithReturn[{i}]....Sec = {sec}");
            }
        }
        private static void TestViewBag()
        {
            //var bags = Data.Views.TestViewBag.GetBags(1000);//最低开销
            //var bags = Data.Views.TestViewBag.GetObjects(1000);//第二，差第一的1倍
            //var bags = Data.Views.TestViewBag.GetJsons(1000);//与第二差不多。
            //foreach (var s in bags)
            //{
            //    var bag = s;//ViewBag.ToObject<ViewBag>(s);
            //    Debug.Log($"{bag.Name}");
            //}
        }
        private static void TestButtonEvent()
        {
            var go = new GameObject("TestButton");
            go.transform.SetParent(Game.SceneCanvas.transform);
            var btn = go.AddComponent<Button>();
            //btn.OnClickAdd(() => Game.MessagingManager.Invoke(nameof(TestAction1), null));
            //Game.MessagingManager.RegEvent(nameof(TestAction1), TestAction1);
            //Game.MessagingManager.RegEvent(nameof(TestAction1), TestAction2);
            //
            //void TestAction1(string args)
            //{
            //    Debug.Log("Test1 Action Invoke!");
            //    Game.MessagingManager.RemoveEvent(nameof(TestAction1), TestAction1);
            //}
            //
            //void TestAction2(string args)
            //{
            //    Debug.Log("Test2 Action Invoke!");
            //}
        }
    }
}