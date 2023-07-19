using System;
using System.Collections.Generic;
using System.Linq;
using AOT.Views.Abstract;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AOT.Views
{
    public class ToastHandler : MonoBehaviour
    {
        public float gap = 10f; // 气泡之间的间隙
        public int maxMessages = 10; // 最大消息数量
        public float messageDuration = 10f; // 消息显示的持续时间（秒）
        [SerializeField] private RectTransform _slidingArea;
        [SerializeField] private AnimationCurve _moveCurve;  // 动画曲线

        private Queue<Toast> messageQueue { get; } = new Queue<Toast>(); // 存储所有消息气泡的队列

        private void Update()
        {
            // 更新所有气泡的位置
            foreach (Toast toast in messageQueue)
                toast.Update(Time.deltaTime, _moveCurve);

            if (messageQueue.Count == 0) return;

            // 如果消息数量超过最大值或者最旧的消息显示时间超过了设定的持续时间，则移除最旧的消息
            while (messageQueue.Count > 0 &&
                   (messageQueue.Count > maxMessages ||
                    (DateTime.Now - messageQueue.Peek().CreateTime).TotalSeconds > messageDuration))
            {
                var toast = messageQueue.Dequeue();
                DestroyToast(toast);
            }

            // 更新所有气泡的目标位置
            UpdateTargetPositions();
        }

        public void DestroyToast(IView view)
        {
            var toast = messageQueue.FirstOrDefault(t => t.GameObject == view.GameObject);
            if (toast == null) return;
            messageQueue.Dequeue();
            DestroyToast(toast);
        }

        private void DestroyToast(Toast toast) => Destroy(toast.GameObject);

        private void UpdateTargetPositions()
        {
            //基于目标位置的y轴坐标计算气泡的新位置
            // 遍历所有的气泡
            for (int i = 0; i < messageQueue.Count; i++)
            {
                Toast toast = messageQueue.ElementAt(i);

                // 计算气泡的新目标位置，气泡从上到下排列，每个气泡之间有gap的间隙
                var yAlign = i * toast.rectTransform.sizeDelta.y + i * gap;

                float targetPosition = (GetRectTopPosY(_slidingArea) - toast.rectTransform.sizeDelta.y / 2f) - yAlign;

                // 设置新的目标位置，只改变y轴的值，x轴和z轴的值保持不变
                var newPosition = new Vector3(toast.transform.position.x, targetPosition, toast.transform.position.z);

                // 移动气泡到新的目标位置
                toast.MoveToTargetPositionSmoothly(newPosition);
            }
        }

        private float GetRectBottomPosY(RectTransform rect) => rect.position.y - rect.rect.height / 2;
        private float GetRectTopPosY(RectTransform rect) => rect.position.y + rect.rect.height / 2;

        [Button(ButtonStyle.Box)]
        public IView CreateToast(View prefab)
        {
            var view = Instantiate(prefab, transform);
            // 将新气泡放在Canvas的最后
            view.transform.SetAsLastSibling();

            // 使用SetText和SetIcon来设置消息和图标
            var toast = new Toast(view);
            // 设置气泡的起始位置
            float fromPosition = GetRectBottomPosY(_slidingArea) + toast.rectTransform.sizeDelta.y / 2f;
            toast.SetPosition(new Vector3(toast.transform.position.x,
                fromPosition,
                toast.transform.position.z));

            // 将新的消息气泡添加到队列
            messageQueue.Enqueue(toast);

            // 更新所有气泡的目标位置
            UpdateTargetPositions();
            return view;
        }

        public class Toast
        {
            public DateTime CreateTime { get; private set; } = DateTime.Now;
            public float Timestamp { get; set; } // 消息显示的时间戳
            public Transform transform => GameObject.transform;
            public RectTransform rectTransform { get; }
            private Vector3 targetPosition; // 目标位置
            private float totalTime = 0.0f;
            private float totalDistance = 0.0f;
            private bool IsMoving => totalDistance > 0;
            public IView View { get; }
            public GameObject GameObject { get; }

            public Toast(IView v)
            {
                GameObject = v.GameObject;
                rectTransform = v.RectTransform;
                View = v;
            }

            public void MoveToTargetPositionSmoothly(Vector3 target)
            {
                totalTime = 1f;
                // 更新目标位置
                targetPosition = target;
                totalDistance = Vector3.Distance(transform.position, targetPosition);
            }

            public void SetPosition(Vector3 position)
            {
                transform.position = position;
                Reset();
            }

            public void Update(float deltaTime, AnimationCurve curve = null)
            {
                if (!IsMoving) return;

                var currentPosition = transform.position;
                totalTime += deltaTime;

                if (curve == null)
                {
                    transform.position = Vector3.Lerp(currentPosition, targetPosition, deltaTime);
                    if (Vector3.Distance(currentPosition, targetPosition) < 1f)
                    {
                        transform.position = targetPosition;
                        Reset();
                    }
                }
                else
                {
                    var moveDistance = totalDistance * curve.Evaluate(totalTime);
                    var moveDirection = (targetPosition - currentPosition).normalized;
                    var translate = moveDirection * moveDistance * deltaTime;

                    // Check if the translation would overshoot the target
                    if (Vector3.Distance(currentPosition + translate, targetPosition) > Vector3.Distance(currentPosition, targetPosition))
                    {
                        transform.position = targetPosition;
                        Reset();
                    }
                    else
                    {
                        transform.Translate(translate, Space.World);
                        if (Vector3.Distance(currentPosition, targetPosition) <= 1f)
                        {
                            transform.position = targetPosition;
                            Reset();
                        }
                    }
                }
            }

            private void Reset()
            {
                totalTime = 0.0f; // Reset totalTime when we reach the target
                totalDistance = 0.0f;
            }
        }
    }
}