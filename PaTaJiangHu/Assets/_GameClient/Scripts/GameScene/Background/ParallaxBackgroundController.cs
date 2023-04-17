using System;
using UnityEngine;

/// <summary>
/// 多段式背景移动器
/// </summary>
public class ParallaxBackgroundController : MonoBehaviour
{
    [SerializeField] private Map[] _maps;
    private Map[] Maps => _maps;
    private void Start() => StopAll();
    public void StopAll()
    {
        foreach (var map in Maps) map.Stop();
    }

    public void StartMove(bool right) => Move(right);
    public void Move(bool right,float speed = 1f)
    {
        foreach (var map in Maps) map.Move(right, speed);
    }

    [Serializable] private class Map
    {
        [SerializeField] private BackgroundMapScroller scroller;
        [SerializeField] private float speed;
        //停止移动
        public void Stop() => scroller.moveSpeed = 0;
        //移动背景,可支持倍数
        public void Move(bool right, float multiplier = 1) => ScrollerMoveSpeed(right, speed * multiplier);
        private float ScrollerMoveSpeed(bool right,float sp) => scroller.moveSpeed = sp * (right ? -1 : 1);
    }
}