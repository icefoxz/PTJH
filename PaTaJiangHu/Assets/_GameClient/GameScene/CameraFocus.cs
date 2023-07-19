using System;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

namespace GameClient.GameScene
{
    public class CameraFocus : MonoBehaviour
    {
        public enum Focus
        {
            [InspectorName("弟子场景")]DiziView,
            [InspectorName("门派场景")]FactionView
        }

        [SerializeField] private FocusField[] _focusFields;
#if UNITY_EDITOR
        [EnumPaging, OnValueChanged(nameof(SetFocus))]
        [InfoBox("切换游戏场景摄像头")]
        [GUIColor("yellow")]
        [SerializeField]private Focus 游戏场景;
#endif
        public void SetFocus(Focus page)
        {
            transform.localPosition = _focusFields.FirstOrDefault(f => f.Focus == page)?.Position ?? Vector2.zero;
        }
        [Serializable]private class FocusField
        {
            [SerializeField] private Focus _focus;
            [SerializeField] private Vector2 _position;
            public Focus Focus => _focus;
            public Vector2 Position => _position;
        }
    }
}