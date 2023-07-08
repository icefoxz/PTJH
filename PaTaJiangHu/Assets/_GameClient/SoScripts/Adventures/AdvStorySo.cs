﻿using System;
using System.Linq;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace GameClient.SoScripts.Adventures
{
    /// <summary>
    /// 故事
    /// </summary>
    [CreateAssetMenu(fileName = "id_故事名", menuName = "状态玩法/历练/故事")]
    internal class AdvStorySo : AdvStorySoBase, IAdvStory
    {
        [MustBeAssigned] [ConditionalField(true, nameof(RefreshAllEvent))] [SerializeField] private AdvEventSoBase 事件;
        [SerializeField] private bool 体力耗尽不强退;

        public bool ContinueOnExhausted => 体力耗尽不强退;
        public IAdvEvent StartAdvEvent => 事件;
        public IAdvEvent[] AllAdvEvents
        {
            get
            {
                var interEvents = AllEvents.ToArray();
                if (interEvents.Length != AllEvents.Length)
                    throw new InvalidCastException(
                        $"[{name}]为交互故事, 其中却包涵非交互式事件, 请确保所有事件继承交互事件接口:{nameof(IAdvEvent)}!");
                return interEvents;
            }
        }

        protected override IAdvEvent BeginEvent => StartAdvEvent;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AdvStorySo))]
    internal class AdvStorySoBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var script = (AdvStorySo)target;
            if (GUILayout.Button("为子事件添加前缀(故事名)"))
            {
                script.RenameAllEvents();
            }
        }
    }
#endif
}