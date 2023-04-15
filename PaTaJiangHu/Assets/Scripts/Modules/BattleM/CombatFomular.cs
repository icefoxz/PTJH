using System;
using _GameClient.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    private const float DodgeRatioMax = 0.9f;
    private const int HardDamageMultiply = 3;

    public static (bool isHardDamage, double rate, int ran) HardJudgment(CombatArgs arg, double hardFactor)
    {
        var hardRate = (arg.Caster.Agility - arg.Target.Agility) / hardFactor;
        hardRate += arg.Caster.Combat?.GetHardRatio(arg) ?? 0;
        var ran = Random.Range(0, 100);
        var isHard = ran <= hardRate;
        return (isHard, hardRate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(CombatArgs arg, double dodgeFactor)
    {
        var dodgeRate = (arg.Caster.Agility - arg.Target.Agility) / dodgeFactor;
        dodgeRate += arg.Caster.Dodge?.GetDodgeRatio(arg) ?? 0;
        dodgeRate = Math.Min(dodgeRate, DodgeRatioMax);
        var ran = Random.Range(0, 100);
        return (ran <= dodgeRate, dodgeRate, ran);
    }
    /// <summary>
    /// 重击, 重击伤害=伤害+内力伤害*3(内力不足，会抽取仅剩内力)
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int HardDamage(CombatArgs arg)
    {
        var damage = 0f;
        for (var i = 0; i < HardDamageMultiply; i++) damage += MpDamage(arg);
        return (int)damage;
    }
    public static int MpDamage(CombatArgs arg)
    {
        var mp = (int)(arg.Caster.MaxMp * 0.1);
        if (arg.Caster.Mp > mp)
        {
            arg.Caster.AddMp(-mp);
            return mp;
        }
        return 0;
    }
    /// <summary>
    /// Damage+MpDamage
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static int GeneralDamage(CombatArgs arg) => arg.Caster.Damage + MpDamage(arg);

    public static (int damage, int mpConsume) DamageReduction(int damage, DiziCombatUnit target)
    {
        var mp = target.Mp;
        var maxMp = target.MaxMp;
        var halfDamage = damage / 2;
        var mpOffset = (int)(maxMp * 0.1f);//10%
        var mpConsume = Math.Min(mp, mpOffset);
        var finalDamage = Math.Max(halfDamage - mpConsume, 0) + halfDamage;
        return (finalDamage, mpConsume);
    }
}