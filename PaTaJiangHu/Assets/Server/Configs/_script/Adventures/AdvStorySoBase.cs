using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Server.Configs.BattleSimulation;
using Server.Configs.Items;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Server.Configs.Adventures
{
    public enum AdvTypes
    {
        Quit,
        Story,
        Dialog,
        Pool,
        Option,
        Battle,
        Term,
        Reward,
        Adjust,
        Simulation,
    }
    /// <summary>
    /// 事件参数, 用于影响事件导向的接口规范
    /// </summary>
    public interface IAdvEventArg
    {
        string DiziName { get; }
        ITerm Term { get; }
        int InteractionResult { get; }
        ISimulationOutcome SimOutcome { get; }
        IAdjustment Adjustment { get; }
        IRewardReceiver Receiver { get; }
    }

    public interface IAdjustment
    {
        public enum Types
        {
            Stamina = 0,
            Silver = 1,
            Food = 2,
            Emotion = 3,
            Injury = 4,
            Inner = 5,
            Exp = 6,
        }

        void Set(Types type, int value, bool percentage);
    }

    public interface IAdvEvent
    {
        string name { get; }
        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        void EventInvoke(IAdvEventArg arg);
        /// <summary>
        /// 下个事件触发
        /// </summary>
        event Action<IAdvEvent> OnNextEvent;
        /// <summary>
        /// 所有事件
        /// </summary>
        IAdvEvent[] AllEvents { get; }
        /// <summary>
        /// 事件类别
        /// </summary>
        AdvTypes AdvType { get; }
        /// <summary>
        /// 文本Log事件, 注意: 有些事件如:选择事件, 或是玩家交互事件有可能是不执行触发的.
        /// </summary>
        event Action<string[]> OnLogsTrigger;
    }

    public interface IAdvStory
    {
        string Name { get; }
        bool HaltOnExhausted { get; }
        IAdvEvent StartAdvEvent { get; }
        IAdvEvent[] AllAdvEvents { get; }
    }

    internal abstract class AdvStorySoBase : AutoDashNamingObject
    {
        protected bool GetItem()
        {
            if (So == null) So = this;
            return true;
        }
        [ConditionalField(true, nameof(GetItem))][ReadOnly][SerializeField] private AdvStorySoBase So;

        public IAdvEvent[] AllEvents => _allEvents;
        [Header("下列事件自动生成，别自行添加")][ReadOnly][SerializeField] private AdvEventSoBase[] _allEvents;
        private const int RecursiveLimit = 9999;
        protected abstract IAdvEvent BeginEvent { get; }
        protected bool RefreshAllEvent()
        {
            if (BeginEvent==null)
            {
                _allEvents = Array.Empty<AdvEventSoBase>();
                return true;
            }

            _allEvents = GetAllEvents().Select(e => (AdvEventSoBase)e).ToArray();
            return true;
        }

        private IAdvEvent[] GetAllEvents()
        {
            var uncheckList = new List<IAdvEvent> { BeginEvent };
            var checkedList = new List<IAdvEvent>();
            var recursiveIndex = 0;
            while (uncheckList.Count > 0)
            {
                var note = uncheckList[0];
                if (note == null)
                {
                    uncheckList.RemoveAt(0);
                }
                else
                {
                    uncheckList.Remove(note);
                    checkedList.Add(note);
                    var notInList = note.AllEvents.Except(checkedList).ToList();
                    uncheckList.AddRange(notInList);
                }

                if (recursiveIndex > RecursiveLimit)
                    throw new StackOverflowException($"事件超过={RecursiveLimit}!");
                recursiveIndex++;
            }

            return checkedList.Distinct().ToArray();
        }

        public void RenameAllEvents()
        {
#if UNITY_EDITOR
            foreach (var e in _allEvents)
            {
                var path = AssetDatabase.GetAssetPath(e);
                var newName = string.Join(Dash, Id, Name, e.Name);
                var err = AssetDatabase.RenameAsset(path, newName);
                if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            }
#endif
        }

        private const char Dash = '_';
    }
}