using System;
using _GameClient.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    private const int DodgeRateMax = 90;
    private const int CriticalDamageMultiply = 3;

    public static (bool isCritical, double criticalRatio, int criticalRan) CriticalJudgment(CombatArgs arg)
    {
        var criticalRate = arg.Caster.Combat?.GetCriticalRate(arg) ?? 0;
        var criticalRan = Random.Range(0, 100);
        return (criticalRan <= criticalRate, criticalRate, criticalRan);
    }
    public static (bool isHardDamage, double rate, int ran) HardJudgment(CombatArgs arg, double hardFactor)
    {
        var hardRate = (arg.Caster.Agility - arg.Target.Agility) / hardFactor;
        hardRate += arg.Caster.Combat?.GetHardRate(arg) ?? 0;
        var ran = Random.Range(0, 100);
        var isHard = ran <= hardRate;
        return (isHard, hardRate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(CombatArgs arg, double dodgeFactor)
    {
        var dodgeRate = (arg.Caster.Agility - arg.Target.Agility) / dodgeFactor;
        dodgeRate += arg.Caster.Dodge?.GetDodgeRate(arg) ?? 0;
        dodgeRate = Math.Min(dodgeRate, DodgeRateMax);
        var ran = Random.Range(0, 100);
        return (ran <= dodgeRate, dodgeRate, ran);
    }
    /// <summary>
    /// 重击, 重击伤害=伤害*重击伤害率 +内力伤害
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int HardDamage(CombatArgs arg)
    {
        var damage = arg.Caster.Damage * arg.Caster.Combat.GetHardDamageRatio(arg);
        var mp = MpDamage(arg);
        arg.Caster.AddMp(-mp);
        return (int)(mp + damage);
    }

    /// <summary>
    /// 会心, 会心伤害=伤害*3(内力不足，会抽取仅剩内力)
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int CriticalDamage(CombatArgs arg)
    {
        var damage = arg.Caster.Damage;
        var mpMax = (int)arg.Caster.Force.GetMpDamage(arg) * CriticalDamageMultiply;
        var mp = Math.Min(mpMax, arg.Caster.Mp);
        arg.Caster.AddMp(-mp);
        return mp + damage;
    }

    public static int MpDamage(CombatArgs arg)
    {
        var mp = (int)arg.Caster.Force.GetMpDamage(arg);
        if (arg.Caster.Mp <= mp) return 0;
        arg.Caster.AddMp(-mp);
        return mp;
    }
    /// <summary>
    /// Damage+MpDamage
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int GeneralDamage(CombatArgs arg) => arg.Caster.Damage + MpDamage(arg);

    public static (int damage, int mpConsume) DamageReduction(int damage, DiziCombatUnit target)
    {
        if (damage == 0) return default;
        var mp = target.Mp;
        var maxMp = target.MaxMp;
        var halfDamage = damage / 2;
        var mpOffset = (int)(maxMp * 0.1f);//10%
        var mpConsume = Math.Min(mp, mpOffset);
        var finalDamage = Math.Max(halfDamage - mpConsume, 0) + halfDamage;
        return (finalDamage, mpConsume);
    }
}