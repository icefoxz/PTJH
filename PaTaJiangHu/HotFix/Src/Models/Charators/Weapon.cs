using BattleM;

namespace HotFix_Project.Models.Charators;

internal class Weapon 
{
    public int Id { get; }
    public string Name { get; }
    public Way.Armed Armed { get; }
    public int Damage { get; }
    public BattleM.Weapon.Injuries Injury { get; }
    public int Grade { get; }
    public int FlingTimes { get; }
}