using UnityEngine;

namespace GameClient.GameScene.Background
{
    /// <summary>
    /// 背景地图, 用于无限滚动地图
    /// </summary>
    public class BackgroundMapElement : MonoBehaviour
    {
        /// <summary>
        /// 地图位置修正,根据提供的<see cref="totalWidth"/>(总地图尺寸)把自己的位置向x方向挪
        /// </summary>
        /// <param name="mainCamera"></param>
        /// <param name="totalWidth"></param>
        public void PositionAlignment(Camera mainCamera, float totalWidth)
        {
            var floorPosition = transform.position;
            var mapsCenter = totalWidth / 2;//总地图的中心点
            //根据总地图的中心点+自身(中心)位置=自身在总中心的偏移点(向右)
            //向右偏移点
            var rightOffset = transform.position.x + mapsCenter;
            //向左偏移点
            var leftOffset = transform.position.x - mapsCenter;
            //如果当前摄像头的位置大于当前向右偏移点
            if (mainCamera.transform.position.x > rightOffset)
            {
                floorPosition.x += totalWidth;
                transform.position = floorPosition;
            }
            else if (mainCamera.transform.position.x < leftOffset)
            {
                floorPosition.x -= totalWidth;
                transform.position = floorPosition;
            }
        }
    }
}