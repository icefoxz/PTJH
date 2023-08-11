using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Utls;
using GameClient.Modules.DiziM;
using GameClient.SoScripts.BattleSimulation;
using GameClient.SoScripts.Items;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
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
    /// 事件总参数处理接口
    /// </summary>
    public interface IAdvArg : IAdvEventArg, IAdjustment
    {
    }
    /// <summary>
    /// 事件参数, 基于<see cref="IAdvEvent"/>实现的接口规范,<br/>
    /// 记录参数, 并且透过<see cref="AdvEventSoBase"/>触发事件信息
    /// </summary>
    public interface IAdvEventArg
    {
        string DiziName { get; }
        ITerm Term { get; }
        int InteractionResult { get; }
        ISimulationOutcome SimOutcome { get; }
        IAdjustment Adjustment { get; }
        IRewardHandler RewardHandler { get; }
        IList<string> ExtraMessages { get; }
    }

    /// <summary>
    /// 事件接口, 作为事件触发器的规范.
    /// <see cref="EventInvoke"/>为事件执行入,执行下列各种效果触发:<br/>
    /// 必须触发一次 <see cref="OnNextEvent"/>: 下个事件触发器,如果没有或是结束传入(null)<br/>
    /// 1. <see cref="OnAdjustmentEvent"/>: 调整事件触发器<br/>
    /// 2. <see cref="OnRewardEvent"/>: 奖励事件触发器<br/>
    /// 3. <see cref="OnLogsTrigger"/>: 文本Log事件触发器<br/>
    /// </summary>
    public interface IAdvEvent
    {
        string Name { get; }
        /// <summary>
        /// 执行事件
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        void EventInvoke(IAdvEventArg arg);
        /// <summary>
        /// 所有事件
        /// </summary>
        IAdvEvent[] AllEvents { get; }
        /// <summary>
        /// 事件类别
        /// </summary>
        AdvTypes AdvType { get; }
        /// <summary>
        /// 下个事件触发器
        /// </summary>
        event Action<IAdvEvent> OnNextEvent;
        /// <summary>
        /// 调整事件触发器
        /// </summary>
        event Action<string[]> OnAdjustmentEvent;
        /// <summary>
        /// 奖励事件触发器
        /// </summary>
        event Action<IGameReward> OnRewardEvent;
        /// <summary>
        /// 文本Log事件触发器
        /// </summary>
        event Action<string[]> OnLogsTrigger;
    }

    public interface IAdvStory
    {
        string Name { get; }
        bool ContinueOnExhausted { get; }
        IAdvEvent StartAdvEvent { get; }
        IAdvEvent[] AllAdvEvents { get; }
    }

    internal abstract class AdvStorySoBase : AutoDashNamingObject
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
                    try
                    {
                        var notInList = note.AllEvents.Except(checkedList).ToList();
                        uncheckList.AddRange(notInList);
                    }
                    catch (Exception e)
                    {
                        XDebug.LogError(e.Message, this);
                        throw;
                        break;
                    }
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