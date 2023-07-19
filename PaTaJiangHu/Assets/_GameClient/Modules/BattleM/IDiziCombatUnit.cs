using AOT.Core.Dizi;
using GameClient.Modules.DiziM;

namespace GameClient.Modules.BattleM
{
    public interface IDiziCombatUnit : ICombatUnit
    {
        string Guid { get; }
        ICombatCondition Mp { get; }
        ICombatAttribute Strength { get; }
        ICombatAttribute Agility { get; }
        ICombatSet GetCombatSet();
        IDiziEquipment Equipment { get; }
        ICombatGifted Gifted { get; }
        ICombatArmedAptitude ArmedAptitude { get; }
        ISkillMap<ISkillInfo> ForceInfo { get; }
        ISkillMap<ICombatSkillInfo> CombatInfo { get; }
        ISkillMap<ISkillInfo> DodgeInfo { get; }
    }
}