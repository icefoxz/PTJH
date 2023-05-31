
//弟子基本属性类
using System;

public record DiziPropValue
{
    /// <summary>
    /// 基础值
    /// </summary>
    public int BaseValue { get; private set; }
    public float LeveledValue => BaseValue + LevelBonus();
    /// <summary>
    /// 等级加成
    /// </summary>
    public int LevelBonus() => (int)(LevelBonusFunc?.Invoke() ?? 0);
    /// <summary>
    /// 状态加成
    /// </summary>
    public int StateBonus() => (int)(StateBonusFunc?.Invoke() ?? 0);
    /// <summary>
    /// 装备加成
    /// </summary>
    public int EquipmentBonus() => (int)(EquipmentBonusFunc?.Invoke() ?? 0);
    /// <summary>
    /// 技能加成
    /// </summary>
    public int SkillBonus() => (int)(SkillBonusFunc?.Invoke() ?? 0);
    private event Func<float> LevelBonusFunc;
    private event Func<float> StateBonusFunc;
    private event Func<float> EquipmentBonusFunc;
    private event Func<float> SkillBonusFunc;

    public int TotalValue => BaseValue +
                             LevelBonus() +
                             StateBonus() +
                             EquipmentBonus() +
                             SkillBonus();

    public DiziPropValue(int baseValue, Func<float> levelBonus, Func<float> stateBonus, Func<float> equipmentBonus,
        Func<float> skillBonus)
    {
        BaseValue = baseValue;
        LevelBonusFunc = levelBonus;
        StateBonusFunc = stateBonus;
        EquipmentBonusFunc = equipmentBonus;
        SkillBonusFunc = skillBonus;
    }
}