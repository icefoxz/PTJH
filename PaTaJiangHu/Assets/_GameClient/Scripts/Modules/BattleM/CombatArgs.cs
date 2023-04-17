using System;

/// <summary>
/// 战斗参数
/// </summary>
public class CombatArgs : EventArgs
{
    public DiziCombatUnit Caster { get; }
    public DiziCombatUnit Target { get; }

    public CombatArgs(DiziCombatUnit caster, DiziCombatUnit target)
    {
        Caster = caster;
        Target = target;
    }
}