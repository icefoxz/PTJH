using System;
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
        public int TypeId { get; }
        public IAdvEvent[] PossibleEvents { get; }
        public void RegEventResult(Action<IAdvEvent> onResultCallback);
    }
    /// <summary>
    /// 副本地图
    /// </summary>
    [CreateAssetMenu(fileName = "id_地图名",menuName = "副本/地图")]
    internal class AdvMap :ScriptableObject
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [MustBeAssigned][SerializeField]private AdvEventSoBase 事件;
        public int Id => _id;
    }
}