using System;
using Random = UnityEngine.Random;

public static class CombatFormula
{
    public static (bool isCritical, double criticalRatio, int criticalRan) CriticalJudgment(CombatArgs arg)
    {
        var criticalRate = arg.Caster.GetCombatSet().GetCriticalRate(arg);
        var criticalRateMax = arg.Caster.Gifted.CritRateMax;
        var rate = Math.Min(criticalRate, criticalRateMax); // 会心率最大值
        var criticalRan = Random.Range(0, 100);
        var isCritical = criticalRan <= rate;
        return (isCritical, rate, criticalRan);
    }
    public static (bool isHardDamage, double rate, int ran) HardJudgment(CombatArgs arg, double hardFactor)
    {
        var hardRate = (arg.Caster.Agility.Value - arg.Target.Agility.Value) / hardFactor;
        hardRate += arg.Caster.GetCombatSet().GetHardRate(arg);
        var hardRateMax = arg.Caster.Gifted.HardRateMax;
        var rate = Math.Min(hardRate, hardRateMax); // 重击率最大值
        var ran = Random.Range(0, 100);
        var isHard = ran <= rate;
        return (isHard, rate, ran);
    }

    public static (bool isDodged, double rate, int ran) DodgeJudgment(CombatArgs arg, double dodgeFactor)
    {
        var dodgeRate = (arg.Caster.Agility.Value - arg.Target.Agility.Value) / dodgeFactor;
        dodgeRate += arg.Caster.GetCombatSet().GetDodgeRate(arg);
        var dodgeRateMax = arg.Caster.Gifted.DodgeRateMax;
        dodgeRate = Math.Min(dodgeRate, dodgeRateMax); // 闪避率最大值
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
        var hardRatio = (1 + arg.Caster.GetCombatSet().GetHardDamageRatioAddOn(arg) // 技能加成
                           + arg.Caster.Gifted.HardDamageRate / 100f); // 天赋
        var damage = arg.Caster.GetDamage() * damageRatio * hardRatio;
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
        var criticalRatio = (1 + arg.Caster.GetCombatSet().GetCriticalDamageRatioAddOn(arg) // 技能加成
                               + arg.Caster.Gifted.CritDamageRate / 100f);// 天赋
        var dmg = arg.Caster.GetDamage();
        var mpUses = (int)arg.Caster.GetCombatSet().GetMpUses(arg) * criticalRatio;
        var mpConsume = (int)Math.Min(mpUses, arg.Caster.Mp.Value);
        // 内力伤害转化率
        var mpConvertAddOn =
            arg.Caster.GetCombatSet().GetMpDamageConvertRateAddOn(arg) + // 技能转化率加成
            arg.Caster.Gifted.MpDamageRate; // 加上天赋
        var mpRatio = mpConvertAddOn / 100f;
        var mpDamage = (int)(mpConsume * (1 + mpRatio));
        var damage = (int)(dmg * damageRatio) + mpDamage;
        return (damage, mpConsume);
    }

    // 一般Mp伤害, 内力不足，会抽取仅剩内力, 会心伤害不可以使用此公式, 因为涉及内力扣除
    private static (int mpDamage,int mpConsume) GetMpDamage(CombatArgs arg)
    {
        var mpConsume = (int)arg.Caster.GetCombatSet().GetMpUses(arg);
        mpConsume = Math.Min(mpConsume, arg.Caster.Mp.Value);
        // 内力伤害转化率
        var mpConvertAddOn =
            arg.Caster.GetCombatSet().GetMpDamageConvertRateAddOn(arg) + // 技能转化率加成
            arg.Caster.Gifted.MpDamageRate; // 加上天赋
        var mpRatio = mpConvertAddOn / 100f;
        var mpDamage = (int)(mpConsume * (1 + mpRatio));
        return ((int)mpDamage, mpConsume);
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
        var combatSet = arg.Target.GetCombatSet();
        var mp = arg.Target.Mp.Value; // 当前内力值
        var maxMpCounteract = (int)(combatSet.GetDamageMpArmorRate(arg) * damage / 100f); // 根据占比计算可用的最大内力护甲
        var mpArmor = combatSet.GetMpArmor(arg); // 获取内力护甲
        var availableMpArmor = (int)Math.Min(mp, mpArmor); // 可用内力护甲
        var mpConsume = Math.Min(maxMpCounteract, availableMpArmor); // 内力消耗
        var mpRate = combatSet.GetMpArmorConvertRateAddOn(arg) + // 内力护甲转化率加成
                     arg.Target.Gifted.MpArmorRate; // 加上天赋
        var mpShield = (int)(mpConsume * (100 + mpRate) / 100f); // 内力护甲转化率
        var finalDamage = damage - mpShield;
        return (finalDamage, mpConsume);
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