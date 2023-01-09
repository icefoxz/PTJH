using System.Collections.Generic;
using _GameClient.Models;
using Utls;

/// <summary>
/// 游戏世界，管理所有游戏模型
/// </summary>
public class GameWorld
{
    /// <summary>
    /// 玩家门派
    /// </summary>
    public Faction Faction { get; set; }
    
    public void TestFaction()
    {
        Faction = new Faction(silver: 10000, yuanBao: 500, actionLing: 1, diziMap: new List<Dizi>());
        XDebug.Log("TestFaction Init!");
        Game.MessagingManager.Send(eventName: EventString.Faction_Init, obj: new Faction.Dto(f: Faction));
    }
}