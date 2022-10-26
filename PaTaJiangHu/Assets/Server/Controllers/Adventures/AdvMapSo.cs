using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    internal interface IAdvEvent
    {
        //public int Id { get; }
        public AdvTypes AdvType { get; }
        public int TypeId { get; }
        public IAdvEvent[] PossibleEvents { get; }
        public void RegEventResult(Action<IAdvEvent> onResultCallback);
    }
    /// <summary>
    /// 副本地图
    /// </summary>
    [CreateAssetMenu(fileName = "id_地图名",menuName = "副本/地图")]
    internal class AdvMapSo :ScriptableObject
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [MustBeAssigned][ConditionalField(true, nameof(RefreshAllEvent))][SerializeField]private AdvEventSoBase 事件;
        public int Id => _id;
        public string Name => _name;
        public IAdvEvent StartEvent => 事件;
        [Header("下列事件自动生成，别自行添加")] [ReadOnly] [SerializeField] private AdvEventSoBase[] _allEvents;
        private const int RecursiveLimit = 9999;
        public IAdvEvent[] AllEvents => _allEvents;
        private bool RefreshAllEvent()
        {
            if (!事件)
            {
                _allEvents = Array.Empty<AdvEventSoBase>();
                return true;
            }
            _allEvents = GetAllEvents().Select(e=>(AdvEventSoBase)e).ToArray();
            return true;
        }
        private IAdvEvent[] GetAllEvents()
        {
            var uncheckList = new List<IAdvEvent> { StartEvent };
            var checkedList = new List<IAdvEvent>();
            var recursiveIndex = 0;
            while (uncheckList.Count > 0)
            {
                var note = uncheckList[0];
                if (note == null) 
                    throw new NullReferenceException($"Recursive index = {recursiveIndex}");
                uncheckList.Remove(note);
                checkedList.Add(note);
                var notInList = note.PossibleEvents.Except(checkedList).ToList();
                uncheckList.AddRange(notInList);
                if (recursiveIndex > RecursiveLimit)
                    throw new StackOverflowException($"事件超过={RecursiveLimit}!");
                recursiveIndex++;
            }
            return checkedList.Distinct().ToArray();
        }
    }
}