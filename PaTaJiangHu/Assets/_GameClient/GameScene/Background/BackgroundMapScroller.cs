using System.Linq;
using UnityEngine;

namespace GameClient.GameScene.Background
{
    /// <summary>
    /// 背景地图无限滚动器,<br/>
    /// 提供<see cref="BackgroundMapElement"/>组提供(地图边界)并逐一更新所有地图的位置修正
    /// </summary>
    public class BackgroundMapScroller : MonoBehaviour
    {
        /// <summary>
        /// 直接赋值可以移动背景,负数向右,正数向左
        /// </summary>
        public float moveSpeed = 0;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private BackgroundMapElement[] _maps;
        private BackgroundMapElement[] Maps => _maps;

        private Camera MainCamera => _mainCamera;
        private float TotalWidth => Maps.Sum(m => m.transform.localScale.x);

        private void MoveBackground()
        {
            var velocity = new Vector3(moveSpeed, 0, 0);
            transform.Translate(velocity * Time.deltaTime);
        }

        public void Update()
        {
            MoveBackground();
            foreach (var map in Maps)
                map.PositionAlignment(MainCamera, TotalWidth);
        }
    }
}