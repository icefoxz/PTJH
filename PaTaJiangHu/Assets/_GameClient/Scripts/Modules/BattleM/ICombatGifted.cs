using Models;

/// <summary>
/// 战斗天赋
/// </summary>
public interface ICombatGifted
{
    /// <summary>
    /// 闪避触发(基础值100)
    /// </summary>
    float DodgeRateMax { get; }
    /// <summary>
    /// 会心触发(基础值100)
    /// </summary>
    float CritRateMax { get; }
    /// <summary>
    /// 重击触发(基础值100)
    /// </summary>
    float HardRateMax { get; }
    /// <summary>
    /// 会心伤害比率(基础值3)
    /// </summary>
    float CritDamageRatioMax { get; }
    /// <summary>
    /// 重击伤害比率(基础值3)
    /// </summary>
    float HardDamageRatioMax { get; }
    /// <summary>
    /// 内力伤害转化(基础值100)
    /// </summary>
    float MpDamageRate { get; }
    /// <summary>
    /// 内力抵消转化(基础值100)
    /// </summary>
    float MpArmorRate { get; }
}