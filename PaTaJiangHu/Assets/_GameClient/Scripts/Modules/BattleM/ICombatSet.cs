using Server.Configs.Battles;
using System;
using System.Collections.Generic;
/// <summary>
/// 战斗基本接口
/// </summary>
public interface ICombatSet
{
    float GetHardRate(CombatArgs arg);//重击触发率
    float GetHardDamageRatio(CombatArgs arg);//重击倍率
    float GetCriticalRate(CombatArgs arg);//会心触发率
    float GetCriticalDamageRatio(CombatArgs arg);//会心倍率
    float GetMpDamage(CombatArgs arg);//内力消耗
    float GetMpCounteract(CombatArgs arg);//内力抵消
    float GetDodgeRate(CombatArgs arg);//闪避率
}

public static class CombatExtension
{
    /// <summary>
    /// 综合所有战斗设定的结果
    /// </summary>
    /// <param name="combats"></param>
    /// <returns></returns>
    public static ICombatSet Combine(this IEnumerable<ICombatSet> combats)
    {
        var hardRate = new List<Func<CombatArgs, float>>();
        var hardDamageRatio = new List<Func<CombatArgs, float>>();
        var criticalRate = new List<Func<CombatArgs, float>>();
        var criticalMultiplier = new List<Func<CombatArgs, float>>();
        var mpDamage = new List<Func<CombatArgs, float>>();
        var mpCounteract = new List<Func<CombatArgs,float>>();
        var dodgeRate = new List<Func<CombatArgs, float>>();
        foreach (var field in combats)
        {
            hardRate.Add(field.GetHardRate);
            hardDamageRatio.Add(field.GetHardDamageRatio);
            criticalRate.Add(field.GetCriticalRate);
            criticalMultiplier.Add(field.GetCriticalDamageRatio);
            mpDamage.Add(field.GetMpDamage);
            mpCounteract.Add(field.GetMpCounteract);
            dodgeRate.Add(field.GetDodgeRate);
        }
        return new CombatSet(
            hardRate: hardRate, 
            hardDamageRatio: hardDamageRatio, 
            criticalRate: criticalRate,
            criticalDamageRatio: criticalMultiplier,
            mpDamage: mpDamage, 
            mpCounteract: mpCounteract, 
            dodgeRate: dodgeRate);
    }
}