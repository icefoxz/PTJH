using Server.Configs.Battles;
using System;
using System.Collections.Generic;
/// <summary>
/// 战斗基本接口
/// </summary>
public interface ICombatSet
{
    float GetHardRate(CombatArgs arg);
    float GetHardDamageRatio(CombatArgs arg);
    float GetCriticalRate(CombatArgs arg);
    float GetMpDamage(CombatArgs arg);
    float GetMpCounteract(CombatArgs arg);
    float GetDodgeRate(CombatArgs arg);
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
        var mpDamage = new List<Func<CombatArgs, float>>();
        var mpCounteract = new List<Func<CombatArgs,float>>();
        var dodgeRate = new List<Func<CombatArgs, float>>();
        foreach (var field in combats)
        {
            hardRate.Add(field.GetHardRate);
            hardDamageRatio.Add(field.GetHardDamageRatio);
            criticalRate.Add(field.GetCriticalRate);
            mpDamage.Add(field.GetMpDamage);
            mpCounteract.Add(field.GetMpCounteract);
            dodgeRate.Add(field.GetDodgeRate);
        }
        return new CombatSet(
            hardRate, 
            hardDamageRatio, 
            criticalRate, 
            mpDamage, 
            mpCounteract, 
            dodgeRate);
    }
}