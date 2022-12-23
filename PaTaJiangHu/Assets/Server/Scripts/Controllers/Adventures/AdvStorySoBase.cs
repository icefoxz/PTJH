using System;
using System.Collections.Generic;
using System.Linq;
using _GameClient.Models;
using MyBox;
using UnityEngine;

namespace Server.Controllers.Adventures
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
        Simulation
    }
    /// <summary>
    /// 事件参数, 用于影响事件导向的接口规范
    /// </summary>
    public interface IAdvEventArg
    {
        ITerm Term { get; }
        int Result { get; }
        ISimulationOutcome SimOutcome { get; }
    }

    public interface IAdvEvent
    {
        string name { get; }
        /// <summary>
        /// 获取下个事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        IAdvEvent GetNextEvent(IAdvEventArg arg);
        /// <summary>
        /// 所有事件
        /// </summary>
        IAdvEvent[] AllEvents { get; }
        /// <summary>
        /// 事件类别
        /// </summary>
        AdvTypes AdvType { get; }
    }

    public interface IAdvAutoEvent : IAdvEvent
    {
        /// <summary>
        /// OnLogsTrigger(string charName, out string[] logs)
        /// </summary>
        event Action<string[]> OnLogsTrigger;
    }

    public interface IAdvInterStory
    {
        IAdvEvent StartAdvEvent { get; }
        IAdvEvent[] AllAdvEvents { get; }
    }

    public interface IAdvAutoStory
    {
        int Id { get; }
        string Name { get; }
        IAdvAutoEvent StartAutoEvent { get; }
    }

    internal abstract class AdvStorySoBase : ScriptableObject
    {
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

            _allEvents = Enumerable.Select<IAdvEvent, AdvEventSoBase>(GetAllEvents(), e => (AdvEventSoBase)e).ToArray();
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
    }
}