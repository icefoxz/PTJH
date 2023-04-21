﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utls;

/**
 * DiziBattle 类：它管理弟子单位之间的战斗。该类有如下属性：IsPlayerWin（是否玩家胜利）、Rounds（回合信息列表）、BuffManager（Buff管理器）、Fighters（战斗单位列表）、IsFinalized（是否结束）等。
 * 此外，它还有一些方法，如 ExecuteRound（执行回合）、Finalize（结束战斗）等。
 * Events 枚举：表示战斗事件，用于定义战斗配置。包括 Perform（执行）、Response（反馈）、RoundEnd（回合结束）和 BattleEnd（战斗结束）。
 * Responses 枚举：表示战斗反馈，用于定义战斗配置。包括 Suffer（受击）、Dodge（闪避）和 Defeat（击败）。
 * ExecuteRound 方法：执行一个回合的战斗。创建一个 DiziCombatRound 对象，根据战斗结果更新回合信息列表，并根据条件判断战斗是否结束。
 * Instance 静态方法：创建一个新的 DiziBattle 实例。
 * StartAuto 静态方法：自动开始并执行战斗。创建一个新的 DiziBattle 实例，并在回合限制内执行战斗回合。根据队伍的平均血量比率判断获胜。
 * PrintLog 静态方法：打印战斗日志。输出战斗开始、回合执行和战斗结束的信息。
 */
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

    // 使用Action<>定义事件
    //public event Action OnRoundStart;
    //public event Action OnRoundEnd;
    //public event Action OnBattleEnd;

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
        // 触发回合开始事件
        //OnRoundStart?.Invoke();

        var round = new DiziCombatRound(Fighters.ToList(), BuffManager);
        var info = round.Execute();
        Rounds.Add(info);

        // 触发回合结束事件
        //OnRoundEnd?.Invoke();

        if (Rounds.Count >= RoundLimit)
        {
            Finalize(AverageHpHigherWin(Fighters));
            // 触发战斗结束事件
            //OnBattleEnd?.Invoke();
            return info;
        }

        if (Fighters.Where(f => f.IsAlive).GroupBy(f => f.TeamId, f => f).Count() > 1) return info;

        var aliveTeam = Fighters.FirstOrDefault(f => f.IsAlive)?.TeamId ?? -1;
        if (aliveTeam == -1)
            throw new InvalidOperationException("没有活着的单位!");
        Finalize(aliveTeam == 0);//玩家单位是0

        // 触发战斗结束事件
        //OnBattleEnd?.Invoke();

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