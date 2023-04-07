using System;
using _GameClient.Models;
using UnityEngine;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    public static (bool isHardDamage, double rate, int ran) HardJudgment(DiziCombatUnit op, DiziCombatUnit tar, double hardFactor)
    {
        var hardRate = (op.Agility - tar.Agility) / hardFactor;
        var ran = Random.Range(0, 100);
        return (ran <= hardRate, hardRate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(DiziCombatUnit op, DiziCombatUnit tar, double dodgeFactor)
    {
        var dodgeRate = (op.Agility - tar.Agility) / dodgeFactor;
        var ran = Random.Range(0, 100);
        return (ran <= dodgeRate, dodgeRate, ran);
    }
    public static int HardDamage(DiziCombatUnit unit) => unit.Damage * 2;
    public static int MpDamage(DiziCombatUnit unit)
    {
        var mp = (int)(unit.MaxMp * 0.1);
        if (unit.Mp > mp) return mp;
        return 0;
    }

    public static int Attack(DiziCombatUnit unit) => unit.Damage + MpDamage(unit);

    public static (int damage, int mpConsume) DamageReduction(int damage, int mp, int maxMp)
    {
        var halfDamage = damage / 2;
        var mpOffset = (int)(maxMp * 0.1f);//10%
        var offset = Math.Min(mp, mpOffset);
        var finalDamage = Math.Max(halfDamage - offset, 0) + halfDamage;
        return (finalDamage, offset);
    }
}