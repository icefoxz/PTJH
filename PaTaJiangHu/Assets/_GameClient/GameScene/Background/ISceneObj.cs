using UnityEngine;

namespace GameClient.GameScene.Background
{
    /// <summary>
    /// 游戏场景物件,用于2d场景物件的统一处理
    /// </summary>
    public interface ISceneObj
    {
        /// <summary>
        /// 物件主要碰撞器
        /// </summary>
        Collider2D Collider { get; }
        SpriteRenderer OriginRenderer { get; }
    }
}