using System.Collections.Generic;
using _GameClient.Models;

/// <summary>
/// 游戏世界，管理所有游戏模型
/// </summary>
internal class GameWorld
{
    /// <summary>
    /// 玩家门派
    /// </summary>
    public Faction Faction { get; set; }
    
    public void TestFaction()
    {
        Faction = new Faction(10000, 100, 1, new List<Dizi>());
    }
}