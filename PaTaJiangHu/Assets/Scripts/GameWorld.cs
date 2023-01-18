using System.Collections.Generic;
using _GameClient.Models;
using Utls;

/// <summary>
/// 游戏世界，管理所有游戏模型
/// </summary>
public class GameWorld
{
    private Faction _faction;

    /// <summary>
    /// 玩家门派
    /// </summary>
    public Faction Faction => _faction;

    public void SetFaction(Faction faction)
    {
        _faction = faction;
        Game.MessagingManager.Send(eventName: EventString.Faction_Init, obj: new Faction.Dto(f: _faction));
    }
}