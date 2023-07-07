using System;
using System.Linq;
using UnityEngine;

namespace GameClient.GameScene
{
    public class CameraFocus : MonoBehaviour
    {
        public enum Focus
        {
            DiziView,
            FactionView
        }

        [SerializeField] private FocusField[] _focusFields;

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