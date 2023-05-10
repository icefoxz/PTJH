using System;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    public static (bool isCritical, double criticalRatio, int criticalRan) CriticalJudgment(CombatArgs arg)
    {
        var criticalRate = arg.Caster.CombatSet.GetCriticalRate(arg);
        var criticalRan = Random.Range(0, 100);
        return (criticalRan <= criticalRate, criticalRate, criticalRan);
    }
    public static (bool isHardDamage, double rate, int ran) HardJudgment(CombatArgs arg, double hardFactor)
    {
        var hardRate = (arg.Caster.Agility - arg.Target.Agility) / hardFactor;
        hardRate += arg.Caster.CombatSet.GetHardRate(arg);
        var ran = Random.Range(0, 100);
        var isHard = ran <= hardRate;
        return (isHard, hardRate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(CombatArgs arg, double dodgeFactor)
    {
        var dodgeRate = (arg.Caster.Agility - arg.Target.Agility) / dodgeFactor;
        dodgeRate += arg.Caster.CombatSet.GetDodgeRate(arg);
        //dodgeRate = Math.Min(dodgeRate, DodgeRateMax);
        var ran = Random.Range(0, 100);
        return (ran <= dodgeRate, dodgeRate, ran);
    }
    /// <summary>
    /// 重击, 重击伤害=伤害*(1 + 重击伤害率) + 内力伤害
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int HardDamage(CombatArgs arg)
    {
        var dmg = arg.Caster.Damage;
        var damage = dmg * (1 + arg.Caster.CombatSet.GetHardDamageRatio(arg));
        var mp = MpDamage(arg);
        arg.Caster.AddMp(-mp);
        return (int)(mp + damage);
    }

    /// <summary>
    /// 会心, 会心伤害=伤害*(1 + 倍率)(内力不足，会抽取仅剩内力)
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int CriticalDamage(CombatArgs arg)
    {
        var mpMax = (int)arg.Caster.CombatSet.GetMpDamage(arg) * (1 + arg.Caster.CombatSet.GetCriticalDamageRatio(arg));
        var mp = (int)Math.Min(mpMax, arg.Caster.Mp);
        arg.Caster.AddMp(-mp);
        return mp;
    }

    public static int MpDamage(CombatArgs arg)
    {
        var mp = (int)arg.Caster.CombatSet.GetMpDamage(arg);
        mp = Math.Min(mp, arg.Caster.Mp);
        arg.Caster.AddMp(-mp);
        return mp;
    }
    /// <summary>
    /// Damage+MpDamage
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int GeneralDamage(CombatArgs arg) => arg.Caster.Damage + MpDamage(arg);

    public static (int damage, int mpConsume) DamageReduction(int damage, CombatArgs arg)
    {
        if (damage == 0) return default;
        var mp = arg.Target.Mp;
        var halfDamage = damage / 2;
        var mpCounteract = arg.Target.CombatSet.GetMpCounteract(arg);
        var mpConsume = (int)Math.Min(mp, mpCounteract);
        var finalDamage = Math.Max(halfDamage - mpConsume, 0) + halfDamage;
        return (finalDamage, mpConsume);
    }
}