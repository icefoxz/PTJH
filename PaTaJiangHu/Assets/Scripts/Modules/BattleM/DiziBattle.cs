﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utls;

/// <summary>
/// 弟子战斗器, 轮询回合, 不牵涉核心战斗逻辑
/// </summary>
public class DiziBattle
{
    public bool IsPlayerWin { get; private set; }
    public List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo>> Rounds { get; }
    public BuffManager<DiziCombatUnit> BuffManager { get; private set; }
    public DiziCombatUnit[] Fighters { get; }
    public bool IsFinalized { get; private set; }
    private static int CombatUnitSeed { get; set; }
    private DiziBattle(List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo>> rounds,
        params DiziCombatUnit[] combats)
    {
        foreach (var unit in combats) unit.SetInstanceId(++CombatUnitSeed);
        BuffManager = new BuffManager<DiziCombatUnit>(combats.ToList());
        Rounds = rounds;
        Fighters = combats;
    }

    public void Finalize(bool isPlayerWin)
    {
        IsPlayerWin = isPlayerWin;
        IsFinalized = true;
    }

    public RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> ExecuteRound()
    {
        var round = new DiziCombatRound(Fighters.ToList(), BuffManager);
        var info = round.Execute();
        Rounds.Add(info);
        if (Fighters.GroupBy(f => f.TeamId, f => f).Count() > 1) return info;
        var aliveTeam = Fighters.FirstOrDefault(f => f.IsAlive)?.TeamId ?? -1;
        if (aliveTeam == -1) throw new NotImplementedException("没有活着的单位!");
        Finalize(aliveTeam == 0);//玩家单位是0
        return info;
    }

    public static DiziBattle Instance(DiziCombatUnit[] units) =>
        new(new List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo>>(), units);

    public static DiziBattle StartAuto(DiziCombatUnit playerCombat, DiziCombatUnit enemyCombat, int roundLimit)
    {
        var combatUnits = new List<DiziCombatUnit>();
        var buffMgr = new BuffManager<DiziCombatUnit>(combatUnits);
        combatUnits.Add(playerCombat);
        combatUnits.Add(enemyCombat);
        var rounds = new List<RoundInfo<DiziCombatUnit, DiziCombatPerformInfo>>();
        var isPlayerWin = false;
        for (var i = 0; i < roundLimit; i++)
        {
            if (enemyCombat.IsDead || playerCombat.IsDead)
            {
                isPlayerWin = playerCombat.IsAlive;
                break;
            }

            var r = new DiziCombatRound(combatUnits, buffMgr);
            rounds.Add(r.Execute());
        }
        var battle = new DiziBattle(rounds, playerCombat, enemyCombat);
        battle.Finalize(isPlayerWin);
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

    private static void RoundLog(int roundNum, RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> round)
    {
        XDebug.Log($"[回合({roundNum})]");
        foreach (var (unit, performs) in round.UnitInfoMap)
        foreach (var perform in performs)
            XDebug.Log($"{unit.Name}{GetStateText(perform.Performer)}{GetPerformText(perform)}!");
    }

    private static string GetPerformText(DiziCombatPerformInfo perform)
    {
        var response = perform.Reponse;
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

    private static string GetResponseText(CombatResponseInfo<DiziCombatUnit> response)
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