using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace HotUpdate._HotUpdate.Monos
{
    /// <summary>
    /// 弟子基本Ui动画演示, 主要实现主要逻辑和方法, 状态逻辑由状态类调用本体方法
    /// </summary>
    public class Player_dizi_animUi : MonoBehaviour
    {
        public enum Working
        {
            Farming,
            Trading,
            Mining
        }
        [SerializeField] private Animator _anim;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private RectTransform boundary;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float leftOffset = 30f;
        [SerializeField] private float rightOffset = 0f;
        [SerializeField] private float topOffset = -20f;
        [SerializeField] private float bottomOffset = 20f;
        [SerializeField] private float minMoveTime = 1f;
        [SerializeField] private float maxMoveTime = 1.5f;
        [SerializeField] private float minWorkTime = 2f;
        [SerializeField] private float maxWorkTime = 4f;

        private Vector2 minBoundary;
        private Vector2 maxBoundary;
        private Vector2 direction;
        private float halfWidth;
        private float halfHeight;
        private Working _working;

        private IDiziUiState _currentState;
        public Animator Animator => _anim;

        private void Start()
        {
            Init(Working.Farming);
        }

        private void Init(Working working)
        {
            _working = working;
            halfWidth = rectTransform.rect.width / 2;
            halfHeight = rectTransform.rect.height / 2;
            minBoundary = boundary.rect.min + new Vector2(leftOffset, bottomOffset);
            maxBoundary = boundary.rect.max - new Vector2(rightOffset, topOffset);

            // 初始化为移动状态
            SwitchToMovingState();
        }

        private void Update()
        {
            _currentState?.Update();
        }

        public void SwitchToMovingState() => ChangeState(new MovingState(this, Random.Range(minMoveTime, maxMoveTime)));
        public void SwitchToWorkingState()
        {
            switch (_working)
            {
                case Working.Farming:
                    ChangeState(new FarmingState(this, Random.Range(minWorkTime, maxWorkTime)));
                    break;
                case Working.Trading:
                case Working.Mining:
                default:
                    throw new ArgumentOutOfRangeException(nameof(_working), _working, null);
            }
        }

        private void ChangeState(IDiziUiState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void PlayMove() => _anim.Play("Move");
        public void PlayFarm() => _anim.Play("Farm");
        public void SetRandomDirection()
        {
            direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        }
        public void Move()
        {
            rectTransform.anchoredPosition += direction * moveSpeed * Time.deltaTime;
            rectTransform.anchoredPosition = new Vector2(
                Mathf.Clamp(rectTransform.anchoredPosition.x, minBoundary.x, maxBoundary.x),
                Mathf.Clamp(rectTransform.anchoredPosition.y, minBoundary.y, maxBoundary.y)
            );
        }

        public void Farm()
        {
            // 农耕逻辑
        }
    }

    public interface IDiziUiState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class MovingState : IDiziUiState
    {
        private Player_dizi_animUi player;
        private float moveTime;
        private float timer;
        private float changeDirectionTimer;

        public MovingState(Player_dizi_animUi handler, float moveTime)
        {
            player = handler;
            this.moveTime = moveTime;
            timer = 0;
            changeDirectionTimer = 0;
        }

        public void Enter()
        {
            player.PlayMove();
            player.SetRandomDirection(); // 随机设置初始方向
        }

        public void Update()
        {
            player.Move();
            timer += Time.deltaTime;
            changeDirectionTimer += Time.deltaTime;

            if (timer >= moveTime) // 当时间耗尽时切换到工作状态
                player.SwitchToWorkingState();
        }

        public void Exit()
        {
        }
    }

    public class FarmingState : IDiziUiState
    {
        private Player_dizi_animUi _character;
        private float workTime;
        private float timer;
        public FarmingState(Player_dizi_animUi character, float workTime)
        {
            _character = character;
            this.workTime = workTime;
            timer = 0;
        }

        public void Enter()
        {
            _character.PlayFarm();
        }

        public void Update()
        {
            _character.Farm();
            timer += Time.deltaTime;
            if (timer >= workTime) // 当时间耗尽时切换到移动状态
                _character.SwitchToMovingState();
        }

        public void Exit()
        {
            // 可以在这里添加退出状态时的逻辑
        }
    }
}