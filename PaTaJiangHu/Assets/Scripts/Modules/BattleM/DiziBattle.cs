using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utls;

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
/// <summary>
/// 弟子战斗器, 轮询回合, 不牵涉核心战斗逻辑
/// </summary>
public class DiziBattle
{
    /// <summary>
    /// 战斗事件, 用于定义战斗配置
    /// </summary>
    public enum Events
    {
        Perform,
        Response,
        RoundEnd,
        BattleEnd,
    }
    /// <summary>
    /// 战斗反馈, 用于定义战斗配置
    /// </summary>
    public enum Responses
    {
        Suffer,
        Dodge,
        Defeat
    }

    public bool IsPlayerWin { get; private set; }
    public List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>> Rounds { get; }
    public BuffManager<DiziCombatUnit> BuffManager { get; private set; }
    public DiziCombatUnit[] Fighters { get; }
    public bool IsFinalized { get; private set; }
    private static int CombatUnitSeed { get; set; }
    public int RoundLimit { get; private set; }

    private DiziBattle(int roundLimit, params DiziCombatUnit[] combats)
    {
        RoundLimit = roundLimit;
        foreach (var unit in combats) unit.SetInstanceId(++CombatUnitSeed);
        BuffManager = new BuffManager<DiziCombatUnit>(combats.ToList());
        Rounds = new List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo>>();
        Fighters = combats;
    }

    private void Finalize(bool isPlayerWin)
    {
        IsPlayerWin = isPlayerWin;
        IsFinalized = true;
    }

    public DiziRoundInfo ExecuteRound()
    {
        var round = new DiziCombatRound(Fighters.ToList(), BuffManager);
        var info = round.Execute();
        Rounds.Add(info);
        if (Rounds.Count >= RoundLimit)
        {
            Finalize(AverageHpHigherWin(Fighters));
            return info;
        }
        if (Fighters.Where(f => f.IsAlive).GroupBy(f => f.TeamId, f => f).Count() > 1) return info;
        var aliveTeam = Fighters.FirstOrDefault(f => f.IsAlive)?.TeamId ?? -1;
        if (aliveTeam == -1) throw new NotImplementedException("没有活着的单位!");
        Finalize(aliveTeam == 0);//玩家单位是0
        return info;
    }

    private static bool AverageHpHigherWin(DiziCombatUnit[] fighters)
    {
        return fighters.Where(f => f.TeamId == 0).Average(f => f.HpRatio) >
               fighters.Where(f => f.TeamId == 1).Average(f => f.HpRatio);
    }

    public static DiziBattle Instance(DiziCombatUnit[] units, int roundLimit = 20) => new(roundLimit, units);

    public static DiziBattle StartAuto(DiziCombatUnit playerCombat, DiziCombatUnit enemyCombat, int roundLimit)
    {
        var battle = new DiziBattle(roundLimit, playerCombat, enemyCombat);
        for (var i = 0; i < roundLimit; i++)
        {
            if (battle.IsFinalized) break;
            battle.ExecuteRound();
        }

        if (!battle.IsFinalized)//根据队伍的平均血量比率判断获胜
            battle.Finalize(AverageHpHigherWin(battle.Fighters));
#if UNITY_EDITOR
        PrintLog(battle);
#endif
        return battle;
    }

    private static void PrintLog(DiziBattle battle)
    {
        var teams = battle.Fighters.GroupBy(f => f.TeamId).ToArray();
        Log($"战斗开始! {string.Join(',', teams[0])} vs {string.Join(',', teams[1])}");
        for (var i = 0; i < battle.Rounds.Count; i++)
        {
            var round = battle.Rounds[i];
            RoundLog(i + 1, round);
        }

        Log($"战斗结束! {(battle.IsPlayerWin ? "玩家获胜!" : "玩家战败!")}");
    }

    private static void RoundLog(int roundNum, RoundInfo<DiziCombatUnit, DiziCombatPerformInfo, DiziCombatInfo> round)
    {
        XDebug.Log($"[回合({roundNum})]");
        foreach (var (unit, performs) in round.UnitInfoMap)
        foreach (var perform in performs)
            XDebug.Log($"{unit.Name}{GetStateText(perform.Performer)}{GetPerformText(perform)}!");
    }

    private static string GetPerformText(DiziCombatPerformInfo perform)
    {
        var response = perform.Response;
        var target = response.Target;
        var performSb = new StringBuilder("攻击");
        performSb.Append(target.Name);
        if (perform.IsHard)
            performSb.Append("(重击)");
        if (perform.IsCritical)
            performSb.Append("(暴击)");
        performSb.Append(GetResponseText(response));
        return performSb.ToString();
    }

    private static string GetResponseText(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response)
    {
        var target = response.Target.Name + GetStateText(response.Target);
        var finalDmg = response.FinalDamage;
        if (response.IsDodged) return $"但{target}闪避了!";
        return $" 伤害:{finalDmg}, {target}";
    }

    private static string GetStateText(CombatUnitInfo<DiziCombatUnit> unit)
    {
        var targetState = $"[{unit.Hp}/{unit.MaxHp}]";
        return targetState;
    }

    private static void Log(string text) => XDebug.Log(text);
}