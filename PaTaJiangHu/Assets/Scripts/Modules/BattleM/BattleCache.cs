using System.Linq;

/// <summary>
/// 战斗缓存, 用于存储战斗信息, 用于战斗回放
/// </summary>
public class BattleCache
{
    private DiziBattle Battle { get; set; }

    public void SetBattle(DiziBattle battle)=> Battle = battle;

    public CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> GetLastResponse(int performerId,int performIndex)
    {
        var infos = Battle.Rounds.Last();
        var performInfos = infos.UnitInfoMap.First(x => x.Key.InstanceId == performerId).Value;
        var perform = performInfos[performIndex];
        return perform.Response;
    }

    public DiziCombatUnit[] GetFighters(int teamId) => Battle.Fighters.Where(f => f.TeamId == teamId).ToArray();
    public DiziCombatUnit GetDizi(string guid) => Battle.Fighters.SingleOrDefault(f => f.Guid == guid);
}