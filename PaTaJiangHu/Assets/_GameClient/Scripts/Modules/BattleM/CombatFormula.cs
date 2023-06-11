using System;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    public static (bool isCritical, double criticalRatio, int criticalRan) CriticalJudgment(CombatArgs arg)
    {
        var criticalRate = arg.Caster.GetCombatSet().GetCriticalRate(arg);
        var criticalRan = Random.Range(0, 100);
        return (criticalRan <= criticalRate, criticalRate, criticalRan);
    }
    public static (bool isHardDamage, double rate, int ran) HardJudgment(CombatArgs arg, double hardFactor)
    {
        var hardRate = (arg.Caster.Agility.Value - arg.Target.Agility.Value) / hardFactor;
        hardRate += arg.Caster.GetCombatSet().GetHardRate(arg);
        var ran = Random.Range(0, 100);
        var isHard = ran <= hardRate;
        return (isHard, hardRate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(CombatArgs arg, double dodgeFactor)
    {
        var dodgeRate = (arg.Caster.Agility.Value - arg.Target.Agility.Value) / dodgeFactor;
        dodgeRate += arg.Caster.GetCombatSet().GetDodgeRate(arg);
        //dodgeRate = Math.Min(dodgeRate, DodgeRateMax);
        var ran = Random.Range(0, 100);
        return (ran <= dodgeRate, dodgeRate, ran);
    }

    /// <summary>
    /// 重击, 重击伤害=伤害*(1 + 重击伤害率) + 内力伤害
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="damageRatio"></param>
    /// <returns></returns>
    public static (int damage, int mpConsume) HardDamage(CombatArgs arg, float damageRatio)
    {
        var damage = arg.Caster.GetDamage() * damageRatio * (1 + arg.Caster.GetCombatSet().GetHardDamageRatioAddOn(arg));
        var (mpDamage, mpConsume) = GetMpDamage(arg);
        var finalDamage = (int)(damage + mpDamage);
        return (finalDamage, mpConsume);
    }

    /// <summary>
    /// 会心, 会心伤害=伤害*(1 + 倍率)(内力不足，会抽取仅剩内力)
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="damageRatio"></param>
    /// <returns></returns>
    public static (int damage, int mpConsume) CriticalDamage(CombatArgs arg, float damageRatio)
    {
        var dmg = arg.Caster.GetDamage();
        var mpUses = (int)arg.Caster.GetCombatSet().GetMpUses(arg) *
                    (1 + arg.Caster.GetCombatSet().GetCriticalDamageRatioAddOn(arg));
        var mpConsume = (int)Math.Min(mpUses, arg.Caster.Mp.Value);
        // 内力伤害转化率
        var mpConvertAddOn = arg.Caster.GetCombatSet().GetMpDamageConvertRateAddOn(arg) * 0.01f;
        var mpDamage = (int)(mpConsume * (1 + mpConvertAddOn));
        var damage = (int)(dmg * damageRatio) + mpDamage;
        return (damage, mpConsume);
    }

    // 一般Mp伤害, 内力不足，会抽取仅剩内力, 会心伤害不可以使用此公式, 因为涉及内力扣除
    private static (int mpDamage,int mpConsume) GetMpDamage(CombatArgs arg)
    {
        var mpConsume = (int)arg.Caster.GetCombatSet().GetMpUses(arg);
        mpConsume = Math.Min(mpConsume, arg.Caster.Mp.Value);
        // 内力伤害转化率
        var mpConvertAddOn = arg.Caster.GetCombatSet().GetMpDamageConvertRateAddOn(arg) * 0.01f; 
        var mpDamage = mpConsume * (1 + mpConvertAddOn);
        return (mpConsume, (int)mpDamage);
    }

    /// <summary>
    /// Damage+MpDamage
    /// </summary>
    /// <param name="arg"></param>
    /// <param name="damageRatio"></param>
    /// <returns></returns>
    public static (int damage, int mpConsume) GeneralDamage(CombatArgs arg, float damageRatio)
    {
        var (mpDamage, mpConsume) = GetMpDamage(arg);
        var damage = (int)(arg.Caster.GetDamage() * damageRatio + mpDamage);
        return (damage, mpConsume);
    }

    /// <summary>
    /// 内力护甲抵消策略
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="arg"></param>
    /// <returns></returns>
    public static (int damage, int mpConsume) DamageReduction(int damage, CombatArgs arg)
    {
        if (damage == 0) return default;
        var maxMpArmor = arg.Target.Mp.Value;
        var maxMpDamage = damage * arg.Target.GetCombatSet().GetMpArmorRate(arg) * 0.01f;
        var mpConsume = 0f;
        var mpShield = 0f;
        if (maxMpArmor > maxMpDamage)//内力护甲大于内力(占比)分配伤害
        {
            //内力护甲基于伤害来计算消耗
            mpConsume = maxMpDamage * (0.5f - arg.Target.GetCombatSet().GetMpArmorConvertRateAddOn(arg) * 0.01f);
            mpShield = maxMpDamage;
        }
        else
        {
            //内力护甲基于护甲来计算伤害
            mpConsume = maxMpArmor * (0.5f - arg.Target.GetCombatSet().GetMpArmorConvertRateAddOn(arg) * 0.01f);
            mpShield = maxMpArmor;
        }
        
        var finalDamage = damage - (int)mpShield;
        return (finalDamage, (int)mpConsume);
    }

    /// <summary>
    /// 武器损坏率,差值百分比
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static float WeaponBrokenJudgment(int weapon, int comparer)
    {
        return (comparer - weapon) / 500f;
    }

    /// <summary>
    /// 装备损坏率,差值百分比
    /// </summary>
    /// <param name="equipment"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static float ArmorBrokenJudgment(int equipment, int comparer)
    {
        return (comparer - equipment) / 500f;
    }
    
    /// <summary>
    /// 装备损坏率,差值百分比
    /// </summary>
    /// <param name="equipment"></param>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public static float EquipmentBrokenJudgment(int equipment, int comparer)
    {
        return (comparer - equipment) / 1.20f + 5;
    }
}