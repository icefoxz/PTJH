using System;
using System.Collections;
using System.Linq;
using _GameClient.Models;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs;
using Server.Controllers;
using Systems.Messaging;
using UnityEngine;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class TestAutoAdvManager
{
    private DiziAdvController AdvController { get; set; }
    private Test_AutoAdv AutoAdv { get; set; }
    private string DiziGuid { get; set; }

    public void Init()
    {
        AdvController = Game.Controllers.Get<DiziAdvController>();
        Game.UiBuilder.Build("test_autoAdv",
            v =>
            {
                AutoAdv = new Test_AutoAdv(v, () => AdvController.AdventureStart(DiziGuid,0),
                    mile=>
                    {
                        var dizi = Game.World.Faction.GetDizi(DiziGuid);
                        var lastMile = dizi.Adventure is { State: AutoAdventure.States.Progress }
                            ? dizi.Adventure.LastMile
                            : 0;
                        AdvController.OnAdventureProgress(0,SysTime.UnixNow, lastMile, mile, DiziGuid);

                    }, () => AdvController.AdventureRecall(DiziGuid));
            },RegEvent);
    }

    private void RegEvent()
    {
        Game.MessagingManager.RegEvent(EventString.Test_AutoAdvDiziInit, bag =>
        {
            DiziGuid = TestCaller.Instance.InitAutoAdventure();
            AutoAdv.Display(true);
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_Start, bag =>
        {
            AutoAdv.Display(true);
            AutoAdv.ClearMessages();
            var major = AdvController.GetMajorMiles(0);
            var minor = AdvController.GetMinorMiles(0);
            AutoAdv.UpdateMiles(major.Concat(minor).Distinct().ToArray());
        });
        Game.MessagingManager.RegEvent(EventString.Dizi_Adv_EventMessage, bag =>
        {
            var message = bag.Get<string>(1);
            var isStoryEnd = bag.Get<bool>(2);
            AutoAdv.UpdateMessage(message, isStoryEnd);
        });
        //Game.MessagingManager.RegEvent(EventString.Test_AutoAdv_ListMajorMiles, bag =>
        //{
        //    var list = bag.Get<int[]>(0);
        //    AutoAdv.UpdateMiles(list);
        //});
    }


    private class Test_AutoAdv : UiBase
    {
        private event Action<int> OnMilesTriggerAction;
        private Button Btn_advStart { get; }
        private Button Btn_advRecall { get; }
        private ListViewUi<Prefab_log> LogView { get; }
        private ListViewUi<Prefab_place> PlaceView { get; }
        public Test_AutoAdv(IView v,Action onStartAction,Action<int> onMilesTriggerAction,Action onRecallAction) : base(v, false)
        {
            OnMilesTriggerAction = onMilesTriggerAction;
            Btn_advStart = v.GetObject<Button>("btn_advStart");
            Btn_advRecall = v.GetObject<Button>("btn_advRecall");
            LogView = new ListViewUi<Prefab_log>(v, "prefab_log", "scroll_log");
            PlaceView = new ListViewUi<Prefab_place>(v, "prefab_place", "scroll_place");
            Btn_advStart.OnClickAdd(onStartAction);
            Btn_advRecall.OnClickAdd(onRecallAction);
        }

        public void ClearMessages() => LogView.ClearList(ui => ui.Destroy());
        public void UpdateMessage(string message, bool isStoryEnd)
        {
            var ui = LogView.Instance(v => new Prefab_log(v));
            ui.SetMessage(message);
            if (isStoryEnd)
            {
                var endUi = LogView.Instance(v => new Prefab_log(v));
                endUi.SetMessage("-----------------");
            }
            Game.CoService.RunCo(ResetLogView());
            IEnumerator ResetLogView()
            {
                yield return new WaitForSeconds(0.2f);
                LogView.SetVerticalScrollPosition(0);
            }
        }
        public void UpdateMiles(int[] miles)
        {
            PlaceView.ClearList(ui => ui.Destroy());
            foreach (var mile in miles)
            {
                PlaceView.Instance(v => new Prefab_place(v, mile + "里", () => OnMilesTriggerAction?.Invoke(mile)));
            }
        }

        private class Prefab_log : UiBase
        {
            private Text Text_message { get; }
            public Prefab_log(IView v) : base(v, true)
            {
                Text_message = v.GetObject<Text>("text_message");
            }
            public void SetMessage(string text) => Text_message.text = text;
        }

        private class Prefab_place : UiBase
        {
            private Text Text_placeName { get; }

            public Prefab_place(IView v, string name, Action onclickAction) : base(v, true)
            {
                var btn = v.GameObject.GetComponent<Button>();
                btn.OnClickAdd(onclickAction);
                Text_placeName = v.GetObject<Text>("text_placeName");
                Text_placeName.text = name;
            }
        }
    }
}