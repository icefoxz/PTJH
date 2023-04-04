using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 游戏场景实现接口
/// </summary>
public interface IGame2DLand
{
    void PlayBattle(DiziBattle battle, Action callback);
}
/// <summary>
/// 游戏2d场景
/// </summary>
public class Game2DLand : MonoBehaviour,IGame2DLand
{
    [SerializeField] private ParallaxBackgroundController _parallaxBgController;
    [SerializeField] private BattleStage2D _battleStage;

    private BattleStage2D BattleStage => _battleStage;
    private ParallaxBackgroundController ParallaxBgController => _parallaxBgController;

    public void PlayBattle(DiziBattle battle, Action callback) => BattleStage.PlayAutoBattle(battle, callback);
}