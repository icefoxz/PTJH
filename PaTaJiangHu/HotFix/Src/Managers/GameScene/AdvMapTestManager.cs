using System;
using System.Collections.Generic;
using System.Linq;
using HotFix_Project.Serialization;
using HotFix_Project.Views.Bases;
using Server.Configs;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers.GameScene;

public class AdvMapTestManager
{
    private TestMapWindow TestMap { get; set; }
    public void Init()
    {
        var controller = TestCaller.Instance.InstanceAdvMapController();
        Game.UiBuilder.Build("test_testMap", v =>
        {
            TestMap = new TestMapWindow(v, controller.LoadMap);
        },RegEvent);
    }

    private void RegEvent()
    {
        Game.MessagingManager.RegEvent(EventString.Test_AdvMapLoad, bag =>
        {
            TestMap.SetBag(bag.Get<string>(0));
        });
    }

    private class TestMapWindow : UiBase
    {
        private Text Text_title{ get; }
        private ScrollRect Scroll_place { get; }
        private ScrollRect Scroll_path { get; }
        private Button Btn_loadMap { get; }

        private ListViewUi<PathUi> PathView { get; }
        private ListViewUi<PlaceUi> PlaceView { get; }
        private ListViewUi<PropUi> PropView { get; }
        public TestMapWindow(IView v,Action onMapLoadAction) : base(v.GameObject, false)
        {
            Btn_loadMap = v.GetObject<Button>("btn_loadMap");
            Btn_loadMap.OnClickAdd(onMapLoadAction);
            Text_title = v.GetObject<Text>("text_title");
            Scroll_path = v.GetObject<ScrollRect>("scroll_path");
            Scroll_place = v.GetObject<ScrollRect>("scroll_place");
            PathView = new ListViewUi<PathUi>(v.GetObject<View>("prefab_path"), Scroll_path);
            PlaceView = new ListViewUi<PlaceUi>(v.GetObject<View>("prefab_place"), Scroll_place);
            PropView = new ListViewUi<PropUi>(v.GetObject<View>("prefab_prop"), v.GetObject("trans_prop"));
        }
        private Dictionary<int, Path[]> PlacePathMap { get; set; }
        private Map Map { get; set; }
        private Dictionary<int, PlaceUi> PlaceMap { get; } = new Dictionary<int, PlaceUi>();

        public void SetBag(string arg)
        {
            var map = LJson.ToObject<Map>(arg);
            PropView.ClearList(p=>p.Destroy());
            PathView.ClearList(p=>p.Destroy());
            PlaceView.ClearList(p=>p.Destroy());
            PlaceMap.Clear();
            Map = map;
            Text_title.text = $"{Map.Id}.{Map.Name}";
            PlacePathMap = Map.Paths.GroupBy(p => p.PlaceIndex)
                .Select(p => new { placeIndex = p.Key, paths = p.ToArray() })
                .Join(Map.Places, path => path.placeIndex, place => place.Index,
                    (path, place) => new { place.Index, path.paths })
                .ToDictionary(o => o.Index, o => o.paths);
            for (var i = 0; i < Map.Places.Length; i++)
            {
                var place = Map.Places[i];
                var p = PlaceView.Instance(v => new PlaceUi(v));
                var placeIndex = i;
                p.Set(place.Name, () => SetPlace(placeIndex));
                PlaceMap.Add(placeIndex, p);
            }
            Display(true);
        }

        private void SetPlace(int placeIndex)
        {
            PropView.ClearList(p => p.Destroy());
            foreach (var ui in PlaceView.List) ui.SetSelected(ui == PlaceMap[placeIndex]);
            var place = Map.Places[placeIndex];
            var paths = PlacePathMap[place.Index];
            PathView.ClearList(p => p.Destroy());
            foreach (var path in paths)
            {
                var ui = PathView.Instance(v => new PathUi(v));
                var to = Map.Places[path.ToIndex];
                ui.Set(to.Name, $"({to.Stories})", u =>
                {
                    u.SetSelected(u == ui);
                    SetPath(path);
                }, () => SetPlace(to.Index));
            }
        }

        private void SetPath(Path path)
        {
            PropView.ClearList(p => p.Destroy());
            InstanceProp("故事:", $"{path.EventMin}-{path.EventMax}");
            InstanceProp("行动令:", path.ExLingCost.ToString());
            InstanceProp("解锁等级:", path.UnlockLevel.ToString());

            void InstanceProp(string title,string value)
            {
                var ui = PropView.Instance(v => new PropUi(v));
                ui.Set(title, value);
            }
        }


        private class PropUi : UiBase 
        {
            private Text Text_title { get; }
            private Text Text_value { get; }

            public PropUi(IView v) : base(v.GameObject, true)
            {
                Text_title = v.GetObject<Text>("text_title");
                Text_value = v.GetObject<Text>("text_value");
            }

            public void Set(string title, string value)
            {
                Text_title.text = title;
                Text_value.text = value;
            }

        }
        private class PlaceUi : UiBase
        {
            private Text Text_title { get; }
            private Button Btn_place { get; }
            private Outline SelectedOutline { get; }
            public PlaceUi(IView v) : base(v.GameObject, true)
            {
                Text_title = v.GetObject<Text>("text_title");
                Btn_place = v.GetObject<Button>("btn_place");
                SelectedOutline = Btn_place.GetComponent<Outline>();
            }

            public void Set(string title,Action onclickAction)
            {
                Text_title.text = title;
                Btn_place.OnClickAdd(onclickAction);
                SetSelected(false);
            }

            public void SetSelected(bool selected) => SelectedOutline.enabled = selected;
        }
        private class PathUi : UiBase
        {
            private Text Text_title { get; }
            private Text Text_value { get; }
            private Button Button { get; }
            private Button Btn_move { get; }
            private Image Img_bg { get; }
            private Outline SelectedOutLine { get; }

            public PathUi(IView v) : base(v.GameObject, true)
            {
                Text_title = v.GetObject<Text>("text_title");
                Text_value = v.GetObject<Text>("text_value");
                Img_bg = v.GetObject<Image>("img_bg");
                Btn_move = v.GetObject<Button>("btn_move");
                SelectedOutLine = Img_bg.GetComponent<Outline>();
                Button = v.GameObject.GetComponent<Button>();
            }

            public void Set(string title, string value, Action<PathUi> onSelectAction, Action onClickAction)
            {
                Text_title.text = title;
                Text_value.text = value;
                Btn_move.OnClickAdd(onClickAction);
                Button.OnClickAdd(() => onSelectAction(this));
                SetSelected(false);
            }

            public void SetSelected(bool selected) => SelectedOutLine.enabled = selected;
        }
    }
    private class Map
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SoName { get; set; }
        public Place[] Places { get; set; }
        public Path[] Paths { get; set; }
    }
    private class Path
    {
        public int Index { get; set; }
        public int EventMax { get; set; }
        public int EventMin { get; set; }
        public int UnlockLevel { get; set; }
        public int ExLingCost { get; set; }
        public int PlaceIndex { get; set; }
        public int ToIndex { get; set; }
    }
    private class Place
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public int Stories { get; set; }
    }
}