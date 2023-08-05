using UnityEngine;

namespace AOT.Utls.Inputs
{
    public abstract class ClickHandlerBase : MonoBehaviour
    {
        private enum ColliderTypes
        {
            Box,
            Circle,
            Capsule
        }

        [SerializeField] private ColliderTypes _colliderType;
        private ColliderTypes ColliderType => _colliderType;

        [SerializeField] private ClickInputEventSystem.ClickPriorities _clickPriority;
        public ClickInputEventSystem.ClickPriorities ClickPriority => _clickPriority;

        private Collider2D _collider;
        public Collider2D Collider => _collider;

        private void Start()
        {
            _collider = ColliderType switch
            {
                ColliderTypes.Box => gameObject.AddComponent<BoxCollider2D>(),
                ColliderTypes.Circle => gameObject.AddComponent<CircleCollider2D>(),
                ColliderTypes.Capsule => gameObject.AddComponent<CapsuleCollider2D>(),
                _ => _collider
            };

            ClickInputEventSystem.Instance.RegClick(this);
        }

        private void OnDestroy()
        {
            ClickInputEventSystem.Instance.RemoveClick(this);
        }

        public void SwitchPriority(ClickInputEventSystem.ClickPriorities priority)
        {
            _clickPriority = priority;
        }

        /// <summary>
        /// 之类实现的OnClick方法
        /// </summary>
        public abstract void OnClick();
    }
}