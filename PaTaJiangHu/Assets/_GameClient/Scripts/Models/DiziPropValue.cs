
//弟子基本属性类
using System;

public class DiziPropValue
{
    /// <summary>
    /// 基础值
    /// </summary>
    public int BaseValue { get; private set; }
    public int LeveledValue => BaseValue + LevelBonus();
    /// <summary>
    /// 等级加成
    /// </summary>
    public int LevelBonus() => LevelBonusFunc?.Invoke() ?? 0;
    /// <summary>
    /// 状态加成
    /// </summary>
    public int StateBonus() => StateBonusFunc?.Invoke() ?? 0;
    /// <summary>
    /// 装备加成
    /// </summary>
    public int EquipmentBonus() => EquipmentBonusFunc?.Invoke() ?? 0;
    /// <summary>
    /// 技能加成
    /// </summary>
    public int SkillBonus() => SkillBonusFunc?.Invoke() ?? 0;
    private event Func<int> LevelBonusFunc;
    private event Func<int> StateBonusFunc;
    private event Func<int> EquipmentBonusFunc;
    private event Func<int> SkillBonusFunc;

    public int TotalValue => BaseValue +
                             LevelBonus() +
                             StateBonus() +
                             EquipmentBonus() +
                             SkillBonus();

    public DiziPropValue(int baseValue, Func<int> levelBonus, Func<int> stateBonus,Func<int> equipmentBonus, Func<int> skillBonus)
    {
        BaseValue = baseValue;
        LevelBonusFunc = levelBonus;
        StateBonusFunc = stateBonus;
        EquipmentBonusFunc = equipmentBonus;
        SkillBonusFunc = skillBonus;
    }
}