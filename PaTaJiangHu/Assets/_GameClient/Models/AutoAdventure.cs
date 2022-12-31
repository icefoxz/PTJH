using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Server.Configs._script.Adventures;
using UnityEngine;
using Utls;

namespace _GameClient.Models
{
    /// <summary>
    /// (自动)挂机历练模型
    /// </summary>
    internal class AutoAdventure 
    {
        /// <summary>
        /// 挂机状态
        /// </summary>
        public enum States
        {
            /// <summary>
            /// 在旅程中
            /// </summary>
            Traveling,
            /// <summary>
            /// 故事中
            /// </summary>
            Story,
            /// <summary>
            /// 结束状态
            /// </summary>
            End
        }

        private States _state;
        public States State => _state;

        private StoryPicker Picker { get; set; }
        /// <summary>
        /// 回程秒数
        /// </summary>
        public int JourneyReturnSec { get; private set; }
        private TimeRecorder Recorder { get; set; }
        public TimeSpan TotalTimeSpan => Recorder.TotalTimeSpan();
        public long StartTime => Recorder.StartTime;
        public float[] MileMap => Recorder.MileMap.ToArray();
        private int AdventureCoId { get; set; }

        public void Init(AutoAdvMapSo mapSo)
        {
            JourneyReturnSec = mapSo.JourneyReturnSec;
            Picker = new StoryPicker(mapSo);
        }

        private void SetState(States state)
        {
            if (State == state) throw new NotImplementedException("状态相同!");
            if (State == States.End) throw new NotImplementedException("状态结束不允许再改.");
            _state = state;
        }

        /// <summary>
        /// 开始冒险
        /// </summary>
        public void StartAdventure()
        {
            Recorder = new TimeRecorder();
            SetState(States.Traveling);
            AdventureCoId = Game.CoService.RunCo(UpdateEverySec());
        }

        public void StopAdventure()
        {
            Recorder.End();
            SetState(States.End);
            Game.CoService.StopCo(AdventureCoId);
        }

        private IEnumerator UpdateEverySec(float sec = 1f)
        {
            var changeableSecs = sec;//todo 这里添加历练物品(马匹)可改变秒数
            yield return new WaitForSeconds(changeableSecs);
            Recorder.AddMile(changeableSecs);
            if (CheckIfStoryAvailable())
            {
                TryInvokeStory(Recorder.TotalMiles);
            }
        }

        private void TryInvokeStory(int miles)
        {
            var startEvent = Picker.PickStory(miles);
            if (startEvent is IAdvAutoEvent autoEvent)
            {
                autoEvent.OnLogsTrigger += OnLogEvent;
            }
            
        }

        private void OnLogEvent(string[] logs)
        {
            throw new NotImplementedException();
        }

        private bool CheckIfStoryAvailable() => State == States.Traveling;

        // 故事获取器, 仅处理获取故事的逻辑
        private class StoryPicker
        {
            private AutoAdvMapSo Map { get; }

            public StoryPicker(AutoAdvMapSo map)
            {
                Map = map;
            }

            public IAdvEvent PickStory(int mile) => PickPlace(mile).GetStory().StartAdvEvent;

            private IAdvPlace PickPlace(int mile)
            {
                var majorMile = mile % Map.MajorMile;//获取余数, 如果余数是0表示Major故事里数==当前里数
                if (majorMile == Map.MajorMile) return PickMajorPlace();
                var minorMile = mile % Map.MinorMile;//获取余数
                if (minorMile == Map.MinorMile) return PickMinorPlace();
                return null;
            }
            private IAdvPlace PickMajorPlace() => Map.PickMajorPlace();
            private IAdvPlace PickMinorPlace() => Map.PickMinorPlace();
        }
        // 时间
        private record TimeRecorder
        {
            private List<float> _mileMap;

            /// <summary>
            /// 开始时间(Unix戳 milliseconds)
            /// </summary>
            public long StartTime { get; }
            public long EndTime { get; private set; }

            /// <summary>
            /// 每一里的秒数记录器
            /// </summary>
            public IReadOnlyList<float> MileMap => _mileMap;

            public int TotalMiles => _mileMap.Count;

            public TimeRecorder()
            {
                _mileMap = new List<float>();
                StartTime = SysTime.UnixNow;
            }
            public TimeSpan TotalTimeSpan() => TimeSpan.FromMilliseconds(SysTime.UnixNow - StartTime);
            public void AddMile(float sec) => _mileMap.Add(sec);
            public void End()=> EndTime = SysTime.UnixNow;
        }
    }

}